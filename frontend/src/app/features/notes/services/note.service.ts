import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { API_BASE_URL } from '../../../core/api.config';
import { Note } from '../models/note.model';

@Injectable({ providedIn: 'root' })
export class NoteService {

  private readonly baseUrl = `${API_BASE_URL}/notes`;

  constructor(private http: HttpClient) {}

  getNotesByLesson(lessonId: string) {
    return this.http.get<any>(`${this.baseUrl}/lesson/${lessonId}`);
  }

  createNote(lessonId: string, content: string) {
    return this.http.post<any>(this.baseUrl, {
      lessonId,
      content
    });
  }
  getNote(noteId: string) {
  return this.http.get<any>(`${API_BASE_URL}/notes/${noteId}`);
}

  deleteNote(id: string) {
    return this.http.delete(`${this.baseUrl}/${id}`);
  }

  updateNote(id: string, content: string) {
    return this.http.put(`${this.baseUrl}/${id}`, {
      content
    });
  }
}