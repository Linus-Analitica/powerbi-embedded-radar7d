import { Component, Inject, AfterViewInit, ViewChild } from '@angular/core';
import { BmbToastComponent } from '@ti-tecnologico-de-monterrey-oficial/ds-ng';
import { MAT_SNACK_BAR_DATA } from '@angular/material/snack-bar';

@Component({
    selector: 'app-bamboo-toast',
    templateUrl: './bamboo-toast.component.html',
    standalone: false
})
export class BambooToastComponent implements AfterViewInit {
  @ViewChild(BmbToastComponent)
  private toastComponent!: BmbToastComponent;
  constructor(@Inject(MAT_SNACK_BAR_DATA) public data: any) {

  }

  ngAfterViewInit() {
    this.toastComponent.openToast();
  }

}
