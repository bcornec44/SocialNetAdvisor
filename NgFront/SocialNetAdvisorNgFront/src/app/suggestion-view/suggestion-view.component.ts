import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-suggestion-view',
  templateUrl: './suggestion-view.component.html',
  styleUrls: ['./suggestion-view.component.css']
})
export class SuggestionViewComponent {
  @Input() selectedTextHtml: string | undefined;
  @Input() suggestionHtml: string | undefined;

  buttons = [
    { label: 'Autorenew', color: 'lightgray', commandParameter: 'Renew' },
    { label: 'Happy', color: 'yellow', commandParameter: 'Happy' },
    { label: 'Agree', color: 'green', commandParameter: 'Agree' },
    // Add other buttons similarly
  ];

  cancel() {
    // Implement cancel functionality
  }

  settings() {
    // Implement settings functionality
  }

  copy() {
    // Implement copy functionality
  }

  changePersonality(personality: string) {
    // Handle personality change
    console.log('Change personality to:', personality);
  }
}
