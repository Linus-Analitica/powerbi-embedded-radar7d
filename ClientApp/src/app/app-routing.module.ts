import { NgModule } from '@angular/core';
import { PreloadAllModules, RouterModule, Routes } from '@angular/router';
import { FederationComponent } from './components/federation/federation.component';
import { LayoutComponent } from './components/layout/layout.component';
import { HomeComponent } from './components/home/home.component';
import { NotFoundPageComponent } from './components/not-found-page/not-found-page.component';
import { userSessionGuard } from './guard/user-session.guard';

const routes: Routes = [
  {
    path: '',
    component: FederationComponent,
    pathMatch: 'full',
  },
  {
    path: '',
    component: LayoutComponent,
    canActivate: [userSessionGuard],
    children: [
      {
        path: 'home',
        component: HomeComponent,
      },
      //{
      //  path: 'administration',
      //  loadChildren: () => import('@modules/configuration/configuration.module').then((m) => m.ConfigurationModule),
      //  canActivate: [UserSessionGuard]
      //},
    ]
  },
  {
    path: '**',
    component: NotFoundPageComponent,
  },
];

@NgModule({
  declarations: [],
  imports: [
    RouterModule.forRoot(routes, {
      preloadingStrategy: PreloadAllModules
    }),
  ],
  exports: [RouterModule],
})
export class AppRoutingModule { }
