import { Component } from '@angular/core';
import { SessionService } from '../../services/session.service';
import { NotificationSnackbarService } from '../../services/notification-snackbar.service';
import { ResponseApi } from '../../models/response-api.model';
import { Router } from '@angular/router';

@Component({
    selector: 'app-federation',
    templateUrl: './federation.component.html',
    standalone: false
})
export class FederationComponent {
  constructor(private sessionService: SessionService, private notification: NotificationSnackbarService, private router: Router) {
    // Se obtiene los userclaims y el jwt del api
    this.sessionService.setSessionUser().subscribe((response: ResponseApi) => {
      if (response.succeeded) {
        const session = this.sessionService.createSessionUser(response);
        if (session) {
          this.notification.openSnackBar('Se obtiene el token del API', 'primary');
          this.router.navigate(['/home']);
        } else {
          this.notification.openSnackBar('Error al crear la sesión en el API', 'warning');
        }
      }
      else {
        this.notification.openSnackBar(response.message, 'error');
      }
    });
  }

  onButtonPrimary($event) {
    console.log('onButtonPrimary:', $event);
  }
  onButtonSecondary($event) {
    console.log('onButtonSecondary:', $event);
  }
}
