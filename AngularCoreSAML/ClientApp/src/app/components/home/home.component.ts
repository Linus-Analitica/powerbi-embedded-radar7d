import { Component, OnInit } from '@angular/core';

// Models.
import { SessionUser, UserClaims } from '../../models/user-claims.model';

// Services.
import { SessionService } from '../../services/session.service';
import { ResponseApi } from '../../models/response-api.model';
import { NotificationSnackbarService } from '../../services/notification-snackbar.service';
import { WeatherForecastService } from '../../services/weather-forecast.service';
import { WeatherForecast } from '../../models/weather-forecast';

@Component({
    selector: 'app-home',
    templateUrl: './home.component.html',
    standalone: false
})
export class HomeComponent implements OnInit {

  userClaims: UserClaims;
  weatherForecast: WeatherForecast;
  columns = [
    { def: 'date', label: 'Fecha', dataKey: 'date' },
    { def: 'temperatureC', label: 'Temperatura (°C)', dataKey: 'temperatureC' },
    { def: 'temperatureF', label: 'Temperatura (°F)', dataKey: 'temperatureF' },
    { def: 'summary', label: 'Resumen', dataKey: 'summary' },
  ];

  constructor(private sessionService: SessionService, private notification: NotificationSnackbarService, private weatherForecastService: WeatherForecastService) {

  }

  ngOnInit() {
    const usuario: SessionUser = this.sessionService.sessionData;
    this.userClaims = usuario.user;
    this.getData();
  }

  logOut() {
    this.sessionService.logout();
  }

  getData() {
    this.weatherForecastService.getWeatherForecast().subscribe((response: WeatherForecast) => {
      this.weatherForecast = response;
      this.notification.openSnackBar('Se obtienen los datos del API', 'primary');
    });
  }

  select($event) {
    console.log('selected:', $event);
  }
}
