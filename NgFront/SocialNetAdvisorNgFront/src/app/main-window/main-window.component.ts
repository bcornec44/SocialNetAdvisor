import { Component } from '@angular/core';

@Component({
  selector: 'app-main-window',
  templateUrl: './main-window.component.html',
  styleUrls: ['./main-window.component.css']
})
export class MainWindowComponent {
  public url: string = '';
  webViewUrl: string = '';
  showSuggestions: boolean = false;

  constructor() { }

  suggest() {
    // Implement suggestion logic
  }

  home() {
    // Logic to handle home command
  }

  goToUrl() {
    this.webViewUrl = this.url;
  }

  goTo(url: string) {
    this.webViewUrl = url;
  }
}
