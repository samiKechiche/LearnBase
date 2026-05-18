import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';

import { API_BASE_URL } from '../../../core/api.config';
import { ApiResponse } from '../../../core/models/api-response.model';
import { Lesson, CreateLessonRequest, UpdateLessonRequest } from '../models/lesson.model';

@Injectable({ providedIn: 'root' })
export class LessonService {
  private readonly apiUrl = `${API_BASE_URL}/lessons`;

  constructor(private readonly http: HttpClient) {}

  getLessons(search = ''): Observable<Lesson[]> {
    let params = new HttpParams();

    if (search.trim()) {
      params = params.set('search', search.trim());
    }

    return this.http
      .get<ApiResponse<Lesson[]>>(this.apiUrl, { params })
      .pipe(map(res => this.unwrap(res, [])));
  }

  getLesson(id: string): Observable<Lesson> {
    return this.http
      .get<ApiResponse<Lesson>>(`${this.apiUrl}/${id}`)
      .pipe(map(res => this.unwrap(res)));
  }

  createLesson(payload: CreateLessonRequest): Observable<Lesson> {
    return this.http
      .post<ApiResponse<Lesson>>(this.apiUrl, payload)
      .pipe(map(res => this.unwrap(res)));
  }

  updateLesson(id: string, payload: UpdateLessonRequest): Observable<Lesson> {
    return this.http
      .put<ApiResponse<Lesson>>(`${this.apiUrl}/${id}`, payload)
      .pipe(map(res => this.unwrap(res)));
  }

  deleteLesson(id: string): Observable<boolean> {
    return this.http
      .delete<ApiResponse<boolean>>(`${this.apiUrl}/${id}`)
      .pipe(map(res => this.unwrap(res, false)));
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