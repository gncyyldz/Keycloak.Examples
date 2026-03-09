import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import keycloak from './configurations/keycloak.configurations';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  template: `
  <button (click)="loginWithGoogle()">Google ile giriş</button>
  `
})
export class App {
  async loginWithGoogle() {
    await keycloak.login({
      idpHint: 'google',
      redirectUri: window.location.origin
    });
  }
}
