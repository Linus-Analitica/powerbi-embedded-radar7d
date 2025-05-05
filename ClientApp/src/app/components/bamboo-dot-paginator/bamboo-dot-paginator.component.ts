import { Component } from '@angular/core';
export interface Target {
  target: string;
  index: number;
}

@Component({
  selector: 'app-bamboo-dot-paginator',
  templateUrl: './bamboo-dot-paginator.component.html'
})
export class BambooDotPaginatorComponent {
  myActiveDotIndex: number = 0;
  myTotalDots: number = 5;
  myTargets: Target[] = [
    { target: '#item1', index: 0 },
    { target: '#item2', index: 1 },
  ];

  onDotPress(index: number): void {
    this.myActiveDotIndex = index;
  }
}
