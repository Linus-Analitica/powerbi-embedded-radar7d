import { Component, EventEmitter, Output } from '@angular/core';
import { RouterModule } from '@angular/router';
import { SessionService } from '../../services/session.service';

@Component( {
  selector: 'app-top-bar',
  templateUrl: './topbar.component.html',
  styleUrls: [ './topbar.component.css' ]
} )
export class TopBarComponent {
  @Output() logoutClicked = new EventEmitter<void>();

  constructor(
    private sessionService: SessionService,
  ) { }

  logOut( _: Event ): void {
    this.sessionService.logout();
    this.logoutClicked.emit();
  }
}