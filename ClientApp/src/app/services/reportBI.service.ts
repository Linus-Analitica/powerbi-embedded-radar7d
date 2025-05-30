import { HttpClient} from "@angular/common/http";
import {Injectable } from "@angular/core";
import { map, Observable } from "rxjs";
import {ResponseApi} from "../models/response-api.model";


@Injectable({
  providedIn: "root",
})
export  class ReportBIService {
  constructor(
    protected httpClient: HttpClient
  ) {
  }


public loadCurrentReport<T>(options: { [key: string]: any } = {}): Observable<T> {
  return this.httpClient
    .get<ResponseApi<T>>('/PowerBI/loadCurrentReport', { params: options })
    .pipe(
      map((response) => response.data as T)
    );
}

public loadArchivedReport<T>(options: { [key: string]: any } = {}): Observable<T> {
  return this.httpClient
    .get<ResponseApi<T>>('/PowerBI/loadArchivedReport', { params: options })
    .pipe(
      map((response) => response.data)
    );
}
}

