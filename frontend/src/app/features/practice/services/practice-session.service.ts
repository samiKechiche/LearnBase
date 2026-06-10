import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';

import { API_BASE_URL } from '../../../core/api.config';
import { ApiResponse } from '../../../core/models/api-response.model';
import {
  PracticeSession,
  PracticeStats,
  SessionExerciseResult,
  SessionSummary,
  SkipExerciseRequest,
  StartSessionRequest,
  SubmitAnswerRequest,
} from '../models/practice-session.model';

@Injectable({ providedIn: 'root' })
export class PracticeSessionService {
  private readonly apiUrl = `${API_BASE_URL}/practicesessions`;

  constructor(private readonly http: HttpClient) {}

  startSession(payload: StartSessionRequest): Observable<PracticeSession> {
    return this.http
      .post<ApiResponse<PracticeSession>>(`${this.apiUrl}/start`, payload)
      .pipe(map((response) => this.unwrap(response)));
  }

  getSessions(activeOnly?: boolean): Observable<SessionSummary[]> {
    let params = new HttpParams();

    if (activeOnly !== undefined) {
      params = params.set('activeOnly', String(activeOnly));
    }

    return this.http
      .get<ApiResponse<SessionSummary[]>>(this.apiUrl, { params })
      .pipe(map((response) => this.unwrap(response, [])));
  }

  getSession(sessionId: string): Observable<PracticeSession> {
    return this.http
      .get<ApiResponse<PracticeSession>>(`${this.apiUrl}/${sessionId}`)
      .pipe(map((response) => this.unwrap(response)));
  }

  getStats(): Observable<PracticeStats> {
    return this.http
      .get<ApiResponse<PracticeStats>>(`${this.apiUrl}/stats`)
      .pipe(map((response) => this.unwrap(response)));
  }

  submitAnswer(sessionId: string, payload: SubmitAnswerRequest): Observable<SessionExerciseResult> {
    return this.http
      .post<ApiResponse<SessionExerciseResult>>(`${this.apiUrl}/${sessionId}/submit`, payload)
      .pipe(map((response) => this.unwrap(response)));
  }

  skipExercise(sessionId: string, payload: SkipExerciseRequest): Observable<SessionExerciseResult> {
    return this.http
      .post<ApiResponse<SessionExerciseResult>>(`${this.apiUrl}/${sessionId}/skip`, payload)
      .pipe(map((response) => this.unwrap(response)));
  }

  endSession(sessionId: string): Observable<PracticeSession> {
    return this.http
      .post<ApiResponse<PracticeSession>>(`${this.apiUrl}/${sessionId}/end`, {})
      .pipe(map((response) => this.unwrap(response)));
  }

  deleteSession(sessionId: string): Observable<boolean> {
    return this.http
      .delete<ApiResponse<boolean>>(`${this.apiUrl}/${sessionId}`)
      .pipe(map((response) => this.unwrap(response, false)));
  }

  deleteAllSessions(): Observable<boolean> {
    return this.http
      .delete<ApiResponse<boolean>>(`${this.apiUrl}/all`)
      .pipe(map((response) => this.unwrap(response, false)));
  }

  private unwrap<T>(response: ApiResponse<T>, fallback?: T): T {
    if (!response.success) {
      throw new Error(response.errors?.join('\n') || response.message || 'Request failed');
    }

    if (response.data === undefined || response.data === null) {
      return fallback as T;
    }

    return response.data;
  }
}
