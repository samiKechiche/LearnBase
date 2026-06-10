import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';

import { API_BASE_URL } from '../../../core/api.config';
import { ApiResponse } from '../../../core/models/api-response.model';
import { SaveTagRequest, Tag } from '../models/tag.model';

@Injectable({ providedIn: 'root' })
export class TagService {
  private readonly apiUrl = `${API_BASE_URL}/tags`;

  constructor(private readonly http: HttpClient) {}

  getTags(search = ''): Observable<Tag[]> {
    let params = new HttpParams().set('sortBy', 'name').set('ascending', 'true');

    if (search.trim()) {
      params = params.set('search', search.trim());
    }

    return this.http
      .get<ApiResponse<Tag[]>>(this.apiUrl, { params })
      .pipe(map((response) => this.unwrap(response, [])));
  }

  createTag(payload: SaveTagRequest): Observable<Tag> {
    return this.http
      .post<ApiResponse<Tag>>(this.apiUrl, payload)
      .pipe(map((response) => this.unwrap(response)));
  }

  updateTag(tagId: string, payload: SaveTagRequest): Observable<Tag> {
    return this.http
      .put<ApiResponse<Tag>>(`${this.apiUrl}/${tagId}`, payload)
      .pipe(map((response) => this.unwrap(response)));
  }

  deleteTag(tagId: string): Observable<boolean> {
    return this.http
      .delete<ApiResponse<boolean>>(`${this.apiUrl}/${tagId}`)
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
