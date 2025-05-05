import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { BambooToastComponent } from '../components/bamboo-toast/bamboo-toast.component';

@Injectable({
  providedIn: 'root'
})
export class NotificationSnackbarService {

  constructor(private snackBar: MatSnackBar) { }
  public openSnackBar(message: string, appearance: string) {

    this.snackBar.openFromComponent(BambooToastComponent, {
      horizontalPosition: 'end',
      verticalPosition: 'top',
      duration: 6000,
      data: { message: message, appearance: appearance }
    });
  }
}
