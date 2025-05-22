import { HttpClient, HttpParams, HttpHeaders } from "@angular/common/http";
import { Inject, Injectable } from "@angular/core";
import CustomStore from "devextreme/data/custom_store";
import DataSource from "devextreme/data/data_source";
import { LoadOptions, SearchOperation } from "devextreme/data/index";
import { map, catchError, Observable, filter } from "rxjs";
import { CustomStoreOptions } from "devextreme/data/custom_store";
import { GroupItem, LoadFunctionResult, LoadResult, LoadResultObject } from "devextreme/common/data";
import { lastValueFrom } from 'rxjs';
import { DxExtendedPromise } from "devextreme/core/utils/deferred";
import { DeepPartial } from "devextreme/core";

import * as AspNetData from 'devextreme-aspnet-data-nojquery';




@Injectable({
  providedIn: "root",
})
export abstract class Resource<T> {

    protected headers: { [key: string]: string } = {};

  constructor(
    protected httpClient: HttpClient,
    protected endPoint: string,
    protected mClass: new (data: any) => T
  ) {
  }

  protected getHeaders(): HttpHeaders {
    const headers = new HttpHeaders(this.headers ?? {});
    this.headers = {};
    return headers;
  }
  public _load(options: Object= {}): Observable<LoadResultObject<T>> {
    Object.assign(options, this.loadOptions);
    const params = this.BuildParams(options);
    
    return this.httpClient
      .get<LoadResultObject<T>>(`${this.endPoint}${this.pathSuffix}`, {
        params: params,
        headers: this.getHeaders(),
      })
      .pipe(
        map((response: LoadResultObject<T>) => {
          if (options.group) {
            this.setSummaryArray(response.data, options)
            response.data = this.toModelGroupArray<T>((response.data as GroupItem<T>[]), this.mClass);
          } else {
            response.data = this.mapToModelArray<T>(response.data, this.mClass);
          }
          return response as LoadResultObject<T>;

        })
      );
  }
  public _update(id: Number, resource: T): Observable<T> {
    return this.httpClient
      .put<any>(`${this.endPoint}/${id}${this.pathSuffix}`, resource, {
        headers: this.getHeaders(),
      })
      .pipe(
        map((response: any) => {
          return new this.mClass(response.data);
        })
      );
  }
  public _insert(resource: T): Observable<T> {
    return this.httpClient
      .post<any>(`${this.endPoint}${this.pathSuffix}`, resource, {
        headers: this.getHeaders(),
      })
      .pipe(
        map((response: any) => {
          return new this.mClass(response.data);
        })
      );
  }
  public _remove(id: Number): Observable<void> {
    return this.httpClient.delete<any>(
      `${this.endPoint}/${id}${this.pathSuffix}`,
      {
        headers: this.getHeaders(),
      }
    );
  }
  public _byKey(id: Number): Observable<T> {
    return this.httpClient
      .get<any>(`${this.endPoint}/${id}${this.pathSuffix}`, {
        headers: this.getHeaders(),
      })
      .pipe(
        map((response: any) => {
          return new this.mClass(response.data) as T;
        })
      );
  }

}

