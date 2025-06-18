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
  private pbiService: powerbi.service.Service;

  constructor(
    private reportBIService: ReportBIService,
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.pbiService = new powerbi.service.Service(
      powerbi.factories.hpmFactory,
      powerbi.factories.wpmpFactory,
      powerbi.factories.routerFactory
    );
  }

  ngOnInit() {
    const type = this.route.snapshot.paramMap.get('type');
    switch(type){
      case "1":
        this.loadCurrentReport();
        break;
      case "2":
        this.loadArchivedReport();
        break;
    }
  }

  private async loadReport(reportType: 'current' | 'archived') {
    try {
      console.log('Iniciando carga de reporte:', reportType);
      
      // Primero, limpiar el reporte anterior usando reset
      await this.pbiService.reset(this.reportContainer.nativeElement);
      console.log('Reporte anterior limpiado');
      
      // Obtener el nuevo reporte
      const response = await this.reportBIService[reportType === 'current' ? 'loadCurrentReport' : 'loadArchivedReport']<Report>().toPromise();
      console.log('Nuevo reporte obtenido:', response);
      
      if (response) {
        const config: powerbi.IEmbedConfiguration = {
          type: 'report',
          id: response.reportId,
          embedUrl: response.embedUrl,
          accessToken: response.embedToken,
          tokenType: powerbi.models.TokenType.Embed,
          settings: {
            panes: {
              filters: { visible: false },
              pageNavigation: { visible: true }
            }
          }
        };
        console.log('Configuración del reporte:', config);
        
        // Incrustar el nuevo reporte
        await this.pbiService.embed(this.reportContainer.nativeElement, config);
        console.log('Reporte incrustado exitosamente');
      }
    } catch (error) {
      console.error('Error loading report:', error);
    }
  }

  loadCurrentReport() {
    this.loadReport('current');
  }

  loadArchivedReport() {
    this.loadReport('archived');
  }

  goBack(){
    this.router.navigate(['/home']);
  }
}
