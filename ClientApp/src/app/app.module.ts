import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule, provideHttpClient, withInterceptors } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { AppComponent } from './app.component';
import { HomeComponent } from './components/home/home.component';
import { BambooBadgeComponent } from './components/bamboo-badge/bamboo-badge.component';
//Start Bamboo components
import { BmbBadgeComponent } from '@ti-tecnologico-de-monterrey-oficial/ds-ng';
import { BmbButtonDirective } from '@ti-tecnologico-de-monterrey-oficial/ds-ng';
import { BmbDotPaginatorComponent } from '@ti-tecnologico-de-monterrey-oficial/ds-ng';
import { BmbDividerComponent } from '@ti-tecnologico-de-monterrey-oficial/ds-ng';
import { BmbToastComponent } from '@ti-tecnologico-de-monterrey-oficial/ds-ng';
import { BmbTablesComponent } from '@ti-tecnologico-de-monterrey-oficial/ds-ng';
import { BmbLoaderComponent } from '@ti-tecnologico-de-monterrey-oficial/ds-ng';
//End Bamboo components
import { BambooButtonComponent } from './components/bamboo-button/bamboo-button.component';
import { BambooDividerComponent } from './components/bamboo-divider/bamboo-divider.component';
import { BambooDotPaginatorComponent } from './components/bamboo-dot-paginator/bamboo-dot-paginator.component';
import { tokenApiInterceptor } from './interceptor/token-api.interceptor';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { BambooToastComponent } from './components/bamboo-toast/bamboo-toast.component';
import { FederationComponent } from './components/federation/federation.component';
import { NotFoundPageComponent } from './components/not-found-page/not-found-page.component';
import { LayoutComponent } from './components/layout/layout.component';
import { AppRoutingModule } from './app-routing.module';



@NgModule({
  declarations: [
    AppComponent,
    HomeComponent,
    BambooBadgeComponent,
    BambooButtonComponent,
    BambooDividerComponent,
    BambooDotPaginatorComponent,
    BambooToastComponent,
    FederationComponent,
    NotFoundPageComponent,
    LayoutComponent,
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    BrowserAnimationsModule,
    HttpClientModule,
    FormsModule,
    BmbBadgeComponent,
    BmbButtonDirective,
    BmbDotPaginatorComponent,
    BmbDividerComponent,
    BmbToastComponent,
    BmbLoaderComponent,
    BmbTablesComponent,
    AppRoutingModule,
  ],
  providers: [
    provideHttpClient(withInterceptors([tokenApiInterceptor])),
  ],
  bootstrap: [ AppComponent ]
})
export class AppModule { }
