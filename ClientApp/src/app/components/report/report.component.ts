import { Component, OnInit,Input, AfterViewInit, ElementRef, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Report } from "../../models/report.model";
import { ReportBIService } from "../../services/reportBI.service";
import { Router } from '@angular/router';
import * as powerbi from 'powerbi-client';

@Component({
  selector: "app-report",
  templateUrl: "./report.component.html",
  styleUrl:'./report.component.css'
})
export class ReportComponent implements OnInit {
   @ViewChild('reportContainer', { static: true }) reportContainer!: ElementRef;


  constructor(
    private reportBIService: ReportBIService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit() {
    const type = this.route.snapshot.paramMap.get('type');
    switch(type){
      case "1":
        this.loadCurrentReport();
        break
      case "2":
        this.loadArchivedReport();
        break
    }
  }



  loadCurrentReport() {
    this.reportBIService.loadCurrentReport<Report>().subscribe((response) => {
      console.log(response);
       const config: powerbi.IEmbedConfiguration = {
          type: 'report',
          id: response.reportId,
          embedUrl:response.embedUrl,
          accessToken: response.embedToken,
          tokenType: powerbi.models.TokenType.Embed,
          settings: {
            panes: {
              filters: { visible: false },
              pageNavigation: { visible: true }
            }
          }
        };

    const pbiService = new powerbi.service.Service(
      powerbi.factories.hpmFactory,
      powerbi.factories.wpmpFactory,
      powerbi.factories.routerFactory
    );

    pbiService.embed(this.reportContainer.nativeElement, config);

      console.log(response);
    });
  }

   loadArchivedReport() {
    this.reportBIService.loadArchivedReport<Report>().subscribe((response) => {
      console.log(response);
       const config: powerbi.IEmbedConfiguration = {
          type: 'report',
          id: response.reportId,
          embedUrl:response.embedUrl,
          accessToken: response.embedToken,
          tokenType: powerbi.models.TokenType.Embed,
          settings: {
            panes: {
              filters: { visible: false },
              pageNavigation: { visible: true }
            }
          }
        };

    const pbiService = new powerbi.service.Service(
      powerbi.factories.hpmFactory,
      powerbi.factories.wpmpFactory,
      powerbi.factories.routerFactory
    );

    pbiService.embed(this.reportContainer.nativeElement, config);

      console.log(response);
    });
  }
  goBack(){
      this.router.navigate(['/home']);

  }
}
