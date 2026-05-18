import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { forkJoin, map, Observable, of } from 'rxjs';

import { API_BASE_URL } from '../../../core/api.config';
import { ApiResponse } from '../../../core/models/api-response.model';
import { Exercise, SaveExerciseRequest } from '../models/exercise.model';
import { Tag } from '../models/tag.model';

@Injectable({ providedIn: 'root' })
export class ExerciseService {
  private readonly apiUrl = `${API_BASE_URL}/exercises`;

  constructor(private readonly http: HttpClient) {}

  getExercises(search = ''): Observable<Exercise[]> {
    let params = new HttpParams().set('sortBy', 'date').set('ascending', 'false');

    if (search.trim()) {
      params = params.set('search', search.trim());
    }

    return this.http
      .get<ApiResponse<Exercise[]>>(this.apiUrl, { params })
      .pipe(map((response) => this.unwrap(response, [])));
  }

  getExercise(exerciseId: string): Observable<Exercise> {
    return this.http
      .get<ApiResponse<Exercise>>(`${this.apiUrl}/${exerciseId}`)
      .pipe(map((response) => this.unwrap(response)));
  }

  createExercise(payload: SaveExerciseRequest): Observable<Exercise> {
    return this.http
      .post<ApiResponse<Exercise>>(this.apiUrl, payload)
      .pipe(map((response) => this.unwrap(response)));
  }

  updateExercise(exerciseId: string, payload: SaveExerciseRequest): Observable<Exercise> {
    return this.http
      .put<ApiResponse<Exercise>>(`${this.apiUrl}/${exerciseId}`, payload)
      .pipe(map((response) => this.unwrap(response)));
  }

  deleteExercise(exerciseId: string): Observable<boolean> {
    return this.http
      .delete<ApiResponse<boolean>>(`${this.apiUrl}/${exerciseId}`)
      .pipe(map((response) => this.unwrap(response, false)));
  }

  getExerciseTags(exerciseId: string): Observable<Tag[]> {
    return this.http
      .get<ApiResponse<Tag[]>>(`${this.apiUrl}/${exerciseId}/tags`)
      .pipe(map((response) => this.unwrap(response, [])));
  }

  addTagToExercise(exerciseId: string, tagId: string): Observable<Tag> {
    return this.http
      .post<ApiResponse<Tag>>(`${this.apiUrl}/${exerciseId}/tags/${tagId}`, {})
      .pipe(map((response) => this.unwrap(response)));
  }

  removeTagFromExercise(exerciseId: string, tagId: string): Observable<boolean> {
    return this.http
      .delete<ApiResponse<boolean>>(`${this.apiUrl}/${exerciseId}/tags/${tagId}`)
      .pipe(map((response) => this.unwrap(response, false)));
  }

  syncExerciseTags(
    exerciseId: string,
    currentTagIds: string[],
    selectedTagIds: string[],
  ): Observable<void> {
    const current = new Set(currentTagIds);
    const selected = new Set(selectedTagIds);

    const removals = currentTagIds
      .filter((tagId) => !selected.has(tagId))
      .map((tagId) => this.removeTagFromExercise(exerciseId, tagId));

    const additions = selectedTagIds
      .filter((tagId) => !current.has(tagId))
      .map((tagId) => this.addTagToExercise(exerciseId, tagId));

    const requests = [...removals, ...additions];

    if (requests.length === 0) {
      return of(undefined);
    }

    return forkJoin(requests).pipe(map(() => undefined));
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
