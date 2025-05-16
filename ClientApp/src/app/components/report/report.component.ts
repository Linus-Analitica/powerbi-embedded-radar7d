import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { models, Report } from 'powerbi-client';
import { PowerBIEmbedModule } from 'powerbi-client-angular';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { PowerBiService, EmbedInfo } from '../../services/powerbi.service';
import { SessionService } from '../../services/session.service';

@Component( {
  selector: 'app-report',
  standalone: true,
  imports: [ CommonModule, PowerBIEmbedModule ],
  templateUrl: './report.component.html',
  styleUrls: [ './report.component.css' ]
} )
export class ReportComponent implements OnInit, OnDestroy {
  embedInfo: EmbedInfo | null = null;
  error = '';
  reportConfig: any;
  isLoadingReport = false;
  private destroy$ = new Subject<void>();

  constructor(
    private powerBiService: PowerBiService,
    private sessionService: SessionService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.fetchEmbedInfo();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
/*
*/
  fetchEmbedInfo(): void {
    this.isLoadingReport = false;
    this.error = '';

    this.powerBiService.getEmbedInfo().pipe( takeUntil( this.destroy$ ) ).subscribe( {
      next: ( response ) => {
        this.isLoadingReport = false;
        if ( response.accessToken && response.embedUrl ) {
          this.embedInfo = response;
          this.reportConfig = {
            type: 'report',
            embedUrl: 'https://app.powerbi.com/reportEmbed?reportId=38ae0459-3488-436d-bdfc-ec209e61d3fa&groupId=7d59785b-b611-4e91-9d03-c821b7ad002e&w=2&config=eyJjbHVzdGVyVXJsIjoiaHR0cHM6Ly9XQUJJLVBBQVMtMS1TQ1VTLXJlZGlyZWN0LmFuYWx5c2lzLndpbmRvd3MubmV0IiwiZW1iZWRGZWF0dXJlcyI6eyJ1c2FnZU1ldHJpY3NWTmV4dCI6dHJ1ZX19',
            accessToken: 'H4sIAAAAAAAEACXTN66EVgBA0b38FkvkZMnFwCMNmSF3wJBzDpb37m-5v9WR7t8_VnJ3Y_L9-fOnS4L6LucWkmGqXrrqWbumlT2toQ6rvIFGn9j-MCnvMMeZOA54DYm16dQXackaUwR-pk3v_Rbp4-aDTcFUMoLj5aUcTG05n8VhSlG9afq1y7GNGtuoDkQijhM46FDZ-qBtAdvVHs_GUYizQrN907wmdC1PV5Tt5gaUZcOl-veY-2LOs4LhrmEZMfCt5odKJUHp3rOxXqNCpQrTSwqtqMuwvRwPOkOMnSwA9xhfd9w8wqDDDbnJZ9U1YK0xhcB5GGV6akjJNRVBZrfiIMyn1j2u7Jbdn2w2uQX3865qbYZwIiVbL1Ddeopm8btCg50BH3HA3iJohPuSTEaQTb9xH-iG5N2xzxAIsKFdirQPXzRAPHJ7Uod4PDZRtm-AEGWEk03F0WHmFWuqcLKPBNORundZq8oBgW7PAw0RG-FAPqTSqVSIy4w0G8_cbCbZ13pGdgVjwJHCQB6623VpKODtbJiz6_TxBqEUw0_kQSOQq9UH8KfAXLI2bVJkXe46k0HODryr9rL2Y67dBeu9fgFfW2f40SKnOvrN4NZvt3pIipjfANorH55Ym8vppEKMw-z61GvxGoc_Sh95j4uhpQj47HO8RLio05xVwcQX72jbNAR5kWuDDMDU1FLbxmrq5tZLZKHHAvoYes7UO9Vye-0iDJ7aYvKrM0Wb-HrGwPEu4H74Vn2MDfNgLu6mwK1qe3aZ3YzO3R2_I2D6xA0vxCDs6axU0YLsMRkFRnGPjA1U1z7yiXPezK-AbtuX-pautyaPz0MLz1wIV2yJrN2zsGuj5w7ZH5lmhQIjtXflB-n4LGgsHZrsUH4Ky7lCTGi_RoxdZ3GQVzZDJhmiPLJtFUZxr5UHJ5KQBGwW-svovkIkhDuQbPhonRe9ULrkm4cSt2-BTiWF5bRJV8fp1a5P86VX-puZBvrt_E3rEzk9SSjkDc2FnW67lM1TOGdyJvOmIP5o0c55ZU_w88cPv9zT71n5_bvz8E1CPcIv3kiqKxI1ddceqrst9IJMyv02lf41xDaNCNIOnmsZum0atlSW40B-nWrszISfMKB6Ec3dKjJBaXJOgpykI0VG7sAZmd_94cKFm0lVwbHpbxBRl8CWuaUji28tKKUGCVUDjdl8iDkuHa3d2DKO9pIOsquPN0tMMUOVYj-ymOuLyV6M3CPKNlEwT9wXzWee9a5subWyRQYTScRH0c6X9UYbkWeJJCJOUlbdi_BiBD572yL3ULKy1eo8XH4A69GOBMGSpwlxwdwgN8uEjcuUH6dxtfnlDm5vptWoq66yD_uhshURQs6JjW0hP9vkq3ibSRxYJFLvs8fPHPgqCfuvv_5jvqcqXxT_V1mICzinpF07YySmlGWiBffzf_WpyyHZ9iX_zdTbfug-nh8dF1ob7bZ3X3i-hgI04aDAYRMtOOwwjSO8kDsjkOLM5FmCwjm5b7QwOEbhsrtqAVbwVOWJD3TEd5eggq3vxpcDl8nhvbZaWmnLXXMrq75-fa2y-MKIxweinxVmi2jtDIaCpVhpOQTrtaeXRNBoUnUnNEZsagYOlF4xrmvZ-5RhN37hksAj3civDUgnttrMzPNj6sDz3DuApnNgFOn7umrnYvZ71ficgY5TkYRgBAQhsjh7paR2UMe5C4yqL6QP-cdQQAN1YrPGlPCdx7MlxG7YstOraHA9DjgugozwuwkJSNOj04rIVHjFkdh471YMaQ3305hndE-STq7lL_M__wKnVfngwgYAAA==.eyJjbHVzdGVyVXJsIjoiaHR0cHM6Ly9XQUJJLVBBQVMtMS1TQ1VTLXJlZGlyZWN0LmFuYWx5c2lzLndpbmRvd3MubmV0IiwiZXhwIjoxNzQ2MDYwNzkxLCJhbGxvd0FjY2Vzc092ZXJQdWJsaWNJbnRlcm5ldCI6dHJ1ZX0=',
            tokenType: models.TokenType.Embed,
            settings: {
              panes: {
                pageNavigation: { position: models.PageNavigationPosition.Left },
                filters: { expanded: false, visible: false }
              }
            }
          };
        } else {
          this.error = 'Error al obtener información del informe de Power BI.';
        }
      },
      error: ( e ) => {
        this.isLoadingReport = false;
        if (
          e?.status === 401 ||
          ( e?.message && e.message.includes( 'Error al solicitar el token de incrustación' ) )
        ) {
          this.sessionService.logout();
        } else {
          this.error = 'Error al solicitar el token de incrustación. Por favor, inténtelo de nuevo más tarde.';
          // console.error( 'Error fetching embed info:', e );
        }
      }
    } );
    console.log('Embed URL:', this.embedInfo.embedUrl);
    console.log('Access Token:', this.embedInfo.accessToken);

  }

  onEmbedded( report: Report ): void {
    // console.log( 'Report embedded:', report );
  }

  onLoadFailed( errorEvent: any ): void {
    // console.error( 'Report load failed:', errorEvent );
    this.error = 'Error al cargar el informe de Power BI. Por favor, inténtelo de nuevo más tarde.';
  }
}