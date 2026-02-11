import { Injectable } from '@angular/core';
import Keycloak from 'keycloak-js';

@Injectable({
  providedIn: 'root',
})
export class KeycloakService {
  static keycloak: Keycloak = new Keycloak({
    url: 'http://127.0.0.1:8080/',
    realm: 'master',
    clientId: 'angular-client',
  });
}