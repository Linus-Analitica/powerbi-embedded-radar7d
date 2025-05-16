import { Component, EventEmitter, Input, Output } from '@angular/core';
import { RouterModule } from '@angular/router';
import { UserClaims } from '../../models/user-claims.model';
// import { AuthService } from '../../services/auth.service';

@Component( {
  selector: 'app-top-bar',
  providers: [ RouterModule ],
  templateUrl: './topbar.component.html',
  styleUrls: [ './topbar.component.css' ]
} )
export class TopBarComponent {
  @Output() logoutClicked = new EventEmitter<void>();
  @Input() currentUserClaims: UserClaims | null = null;

  constructor(
    // private authService: AuthService,
  ) { }

  logOut( _: Event ): void {
    // this.authService.logout();
    this.logoutClicked.emit();
  }
}