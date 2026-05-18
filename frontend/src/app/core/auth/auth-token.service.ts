import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class AuthTokenService {
  private readonly tokenKeys = ['learnbase_token', 'token', 'jwt', 'authToken'];

  getToken(): string | null {
    for (const key of this.tokenKeys) {
      const token = localStorage.getItem(key);
      if (token) {
        return token;
      }
    }

    return null;
  }

  setToken(token: string): void {
    localStorage.setItem(this.tokenKeys[0], token);
  }

  clearToken(): void {
    for (const key of this.tokenKeys) {
      localStorage.removeItem(key);
    }
  }
}
