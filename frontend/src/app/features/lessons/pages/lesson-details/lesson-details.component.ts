import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';

import { LessonService } from '../../services/lesson.service';
import { Lesson } from '../../models/lesson.model';

import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { AppFile } from '../../models/lesson.model';
import { Note } from '../../../notes/models/note.model';
import { NoteService } from '../../../notes/services/note.service';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

@Component({
  selector: 'app-lesson-details',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatDividerModule,
    FormsModule,
  ],
  templateUrl: './lesson-details.component.html',
  styleUrl: './lesson-details.component.css',
})
export class LessonDetailsComponent implements OnInit {

  lessonId!: string;
  loading = false;

  lesson: any = null;
  notes: Note[] = [];
  files: AppFile[] = [];

  constructor(
    private readonly route: ActivatedRoute,
    private readonly lessonService: LessonService,
    private readonly noteService: NoteService,
    private readonly router: Router
  ) {}
  editingNoteId: string | null = null;

  ngOnInit(): void {
    this.lessonId = this.route.snapshot.paramMap.get('id')!;
    this.loadDetails();
  }

  loadDetails(): void {
    this.loading = true;

    this.lessonService.getLessonDetails(this.lessonId).subscribe({
      next: (res) => {
        this.lesson = res.lesson;
        this.notes = res.notes.data ?? res.notes;
        this.files = res.files;
        this.loading = false;
      },
      error: (err) => {
        console.error(err);
        this.loading = false;
      }
    });
  }
  onFileSelected(event: Event): void {
  const input = event.target as HTMLInputElement;

  if (!input.files?.length) {
    return;
  }

  const file = input.files[0];

  this.lessonService.uploadFile(this.lessonId, file)
    .subscribe({
      next: () => {
        this.loadDetails();
      },
      error: (err) => {
        console.error(err);
      }
    });
}
downloadFile(file: AppFile): void {
  this.lessonService.downloadFile(file.fileId)
    .subscribe(blob => {

      const url = window.URL.createObjectURL(blob);

      const a = document.createElement('a');
      a.href = url;
      a.download = file.fileName;

      document.body.appendChild(a);
      a.click();
      a.remove();

      window.URL.revokeObjectURL(url);
    });
}
uploadFile(event: Event): void {
  const input = event.target as HTMLInputElement;
  const file = input.files?.[0];

  if (!file) return;

  this.lessonService.uploadFile(this.lessonId, file).subscribe({
    next: () => {
      this.loadDetails(); // refresh files
    },
    error: (err) => {
      console.error('Upload failed', err);
    }
  });
  

  input.value = '';
}
openFile(file: any) {
  const url = this.lessonService.getFileViewUrl(file);
  window.open(url, '_blank');
}
deleteFile(file: AppFile): void {
    console.log('Deleting file:', file);
  this.lessonService.deleteFile(file.fileId).subscribe({
    next: () => {
      this.files = this.files.filter(f => f.fileId !== file.fileId);
    }
  });
}

editNote(noteId: string): void {
  this.router.navigate([
    '/lessons',
    this.lessonId,
    'notes',
    noteId,
    'edit'
  ]);
}

deleteNote(id: string): void {
  this.noteService.deleteNote(id)
    .subscribe(() => this.loadDetails());
}
getPreview(html: string): string {
  if (!html) return '';

  const div = document.createElement('div');
  div.innerHTML = html;

  const firstParagraph = div.querySelector('p');

  return firstParagraph?.textContent?.trim() || '';
}
previewFile(file: AppFile): void {
  const url = this.lessonService.getFileViewUrl(file);
  window.open(url, '_blank');
}
}