import { Component, Input, AfterViewInit, ElementRef, ViewChild } from '@angular/core';

// Models.
import { UserClaims } from "../../models/user-claims.model";

// Services.
import { SessionService } from "../../services/session.service";
import { Report } from "../../models/report.model";
import { NotificationSnackbarService } from "../../services/notification-snackbar.service";
import { ReportBIService } from "../../services/reportBI.service";

import * as powerbi from 'powerbi-client';

@Component({
  selector: "app-home",
  templateUrl: "./home.component.html",
})
export class HomeComponent implements OnInit {
   @ViewChild('reportContainer', { static: true }) reportContainer!: ElementRef;
  userClaims: UserClaims;
  weatherForecast = [];
  columns = [
    { def: "date", label: "Fecha", dataKey: "date" },
    { def: "temperatureC", label: "Temperatura (°C)", dataKey: "temperatureC" },
    { def: "temperatureF", label: "Temperatura (°F)", dataKey: "temperatureF" },
    { def: "summary", label: "Resumen", dataKey: "summary" },
  ];

  constructor(
    private sessionService: SessionService,
    private notification: NotificationSnackbarService,
    private reportBIService: ReportBIService
  ) {}

  ngOnInit() {
    this.userClaims == this.sessionService.sessionData;
    this.getData();
  }

  logOut() {
    this.sessionService.logout();
  }

  getData() {
    this.reportBIService.loadCurrentReport<Report>().subscribe((response) => {
       const config: powerbi.IEmbedConfiguration = {
          type: 'report',
          id: response.ReportId,
          embedUrl:response.EmbedUrl,
          accessToken: response.EmbedToken,
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

  select($event) {
    console.log("selected:", $event);
  }
}
