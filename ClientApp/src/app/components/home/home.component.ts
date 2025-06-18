import { Component, OnInit, Input, AfterViewInit, ElementRef, ViewChild } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: "app-home",
  templateUrl: "./home.component.html",
  styleUrl: "./home.component.css"
})
export class HomeComponent implements OnInit {


  constructor( private router: Router) { }

  ngOnInit() {
    this.gotoCurrentReport();
  }

  gotoCurrentReport(){
  this.router.navigate(['/report', 1]);
  }
  gotoArchiveReport(){
  this.router.navigate(['/report', 2]);
  }
}
