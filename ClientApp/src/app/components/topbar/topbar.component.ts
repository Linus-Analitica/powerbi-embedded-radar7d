import { Component, EventEmitter, Output, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { SessionService } from '../../services/session.service';
import { UserClaims } from '../../models/user-claims.model';

@Component({
  selector: 'app-top-bar',
  templateUrl: './topbar.component.html',
  styleUrls: ['./topbar.component.css']
})
export class TopBarComponent {
  @Output() logoutClicked = new EventEmitter<void>();
  public session: UserClaims;

  constructor(private sessionService: SessionService) {
    this.session=this.sessionService.sessionData;
  }

  logOut(_: Event): void {
    this.sessionService.logout();
    this.logoutClicked.emit();
  }

}