import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';

import { API_BASE_URL } from '../../../core/api.config';
import { ApiResponse } from '../../../core/models/api-response.model';
import {
  AddExerciseToSetRequest,
  CreationType,
  GeneratePracticeSetRequest,
  PracticeSet,
  PracticeSetExerciseSummary,
  SavePracticeSetRequest,
} from '../models/practice-set.model';

@Injectable({ providedIn: 'root' })
export class PracticeSetService {
  private readonly apiUrl = `${API_BASE_URL}/practicesets`;

  constructor(private readonly http: HttpClient) {}

  getPracticeSets(search = ''): Observable<PracticeSet[]> {
    let params = new HttpParams().set('sortBy', 'date').set('ascending', 'false');

    if (search.trim()) {
      params = params.set('search', search.trim());
    }

    return this.http
      .get<ApiResponse<PracticeSet[]>>(this.apiUrl, { params })
      .pipe(map((response) => this.unwrap(response, [])));
  }

  getPracticeSet(practiceSetId: string): Observable<PracticeSet> {
    return this.http
      .get<ApiResponse<PracticeSet>>(`${this.apiUrl}/${practiceSetId}`)
      .pipe(map((response) => this.unwrap(response)));
  }

  createPracticeSet(payload: SavePracticeSetRequest): Observable<PracticeSet> {
    const body = { ...payload, creationType: payload.creationType ?? CreationType.Manual };

    return this.http
      .post<ApiResponse<PracticeSet>>(this.apiUrl, body)
      .pipe(map((response) => this.unwrap(response)));
  }

  updatePracticeSet(practiceSetId: string, payload: SavePracticeSetRequest): Observable<PracticeSet> {
    return this.http
      .put<ApiResponse<PracticeSet>>(`${this.apiUrl}/${practiceSetId}`, {
        title: payload.title,
        description: payload.description,
      })
      .pipe(map((response) => this.unwrap(response)));
  }

  deletePracticeSet(practiceSetId: string): Observable<boolean> {
    return this.http
      .delete<ApiResponse<boolean>>(`${this.apiUrl}/${practiceSetId}`)
      .pipe(map((response) => this.unwrap(response, false)));
  }

  getSetExercises(practiceSetId: string): Observable<PracticeSetExerciseSummary[]> {
    return this.http
      .get<ApiResponse<PracticeSetExerciseSummary[]>>(`${this.apiUrl}/${practiceSetId}/exercises`)
      .pipe(map((response) => this.unwrap(response, [])));
  }

  addExercise(practiceSetId: string, payload: AddExerciseToSetRequest): Observable<PracticeSet> {
    return this.http
      .post<ApiResponse<PracticeSet>>(`${this.apiUrl}/${practiceSetId}/exercises`, payload)
      .pipe(map((response) => this.unwrap(response)));
  }

  removeExercise(practiceSetId: string, exerciseId: string): Observable<boolean> {
    return this.http
      .delete<ApiResponse<boolean>>(`${this.apiUrl}/${practiceSetId}/exercises/${exerciseId}`)
      .pipe(map((response) => this.unwrap(response, false)));
  }

  generateFromTags(payload: GeneratePracticeSetRequest): Observable<PracticeSet> {
    return this.http
      .post<ApiResponse<PracticeSet>>(`${this.apiUrl}/generate`, payload)
      .pipe(map((response) => this.unwrap(response)));
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
