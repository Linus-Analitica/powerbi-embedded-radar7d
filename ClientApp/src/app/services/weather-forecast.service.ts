import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { WeatherForecast } from '../models/weather-forecast';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class WeatherForecastService {

  constructor(private http: HttpClient) { }

  public getWeatherForecast(): Observable<WeatherForecast> {
    return this.http.get<WeatherForecast>(
      `${environment.urlApi}Home`
    );
  }
}
