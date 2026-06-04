import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';

import { API_BASE_URL } from '../../../core/api.config';
import { ApiResponse } from '../../../core/models/api-response.model';
import { Lesson, CreateLessonRequest, UpdateLessonRequest } from '../models/lesson.model';

@Injectable({ providedIn: 'root' })
export class LessonService {
  private readonly apiUrl = `${API_BASE_URL}/lessons`;
  private readonly importExportUrl = `${API_BASE_URL}/ImportExport`;

  constructor(private readonly http: HttpClient) {}

  getLessons(search = ''): Observable<Lesson[]> {
    let params = new HttpParams();

    if (search.trim()) {
      params = params.append('search', search.trim());
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

  exportLesson(lessonId: string): Observable<Blob> {
    return this.http.get(`${this.importExportUrl}/lesson/${lessonId}/export`, {
      responseType: 'blob' as 'blob',
    });
  }

  importLesson(file: File): Observable<Lesson> {
    const payload = new FormData();
    payload.append('file', file);

    return this.http
      .post<unknown>(`${this.importExportUrl}/lesson/import`, payload)
      .pipe(map((response) => this.parseImportResponse<Lesson>(response)));
  }

  private parseImportResponse<T>(response: unknown): T {
    if (response && typeof response === 'object') {
      const apiResponse = response as Partial<ApiResponse<T>>;

      if (typeof apiResponse.success === 'boolean') {
        if (!apiResponse.success) {
          throw new Error(apiResponse.errors?.join('\n') || apiResponse.message || 'Request failed');
        }

        if (apiResponse.data !== undefined) {
          return apiResponse.data as T;
        }
      }

      return response as T;
    }

    throw new Error('Invalid import response');
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