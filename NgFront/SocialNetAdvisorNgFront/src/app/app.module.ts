import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { MainWindowComponent } from './main-window/main-window.component';
import { SafeUrlPipe } from './safe-url.pipe';
import { SuggestionViewComponent } from './suggestion-view/suggestion-view.component';

@NgModule({
  declarations: [
    AppComponent,
    MainWindowComponent,
    SuggestionViewComponent,
    SafeUrlPipe
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    FormsModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
