import { Component, inject, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { KeycloakService } from './services/keycloak.service.';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  template: `
  <h1>{{ hi() }}</h1>

  <button (click)="getHelloWord()">getHelloWord()</button>
  `
})
export class App {
  httpClient: HttpClient = inject(HttpClient);

  hi = signal("");
  getHelloWord() {
    this.httpClient.get("https://localhost:7297/", {
      headers: {
        Authorization: `Bearer ${KeycloakService.keycloak.token}`
      }
    }).subscribe((res) => this.hi.set(res.toString()));
  }
}