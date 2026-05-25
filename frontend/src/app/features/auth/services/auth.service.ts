import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';

import { ApiResponse } from '../../../core/models/api-response.model';
import { User, SignInRequest, SignUpRequest } from '../models/user.model';

@Injectable({ providedIn: 'root' })
export class AuthService
{
  private readonly apiUrl = `http://localhost:5077`;

  constructor(private readonly http: HttpClient) {}

  signIn(payload: SignInRequest): Observable<User> {
    return this.http
      .post<ApiResponse<User>>(`${this.apiUrl}/login`, payload)
      .pipe(map(res => this.unwrap(res)));
  }

  signUp(payload: SignUpRequest): Observable<User> {
    return this.http
      .post<ApiResponse<User>>(`${this.apiUrl}/register`, payload)
      .pipe(map(res => this.unwrap(res)));
  }
  

  private unwrap<T>(response: ApiResponse<T>, fallback?: T): T {
    if (!response.success) {
      throw new Error(response.errors?.join('\n') || response.message || 'Request failed');
    }

    if (response.data === null || response.data === undefined) {
      return fallback as T;
    }

    return response.data;
  }
}