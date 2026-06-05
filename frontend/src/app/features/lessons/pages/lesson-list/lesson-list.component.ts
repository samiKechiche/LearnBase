import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';

import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged } from 'rxjs';

import { apiErrorMessage } from '../../../../core/http/api-error';
import { LessonService } from '../../services/lesson.service';
import { Lesson } from '../../models/lesson.model';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';



@Component({
  selector: 'app-lesson-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatSnackBarModule,
    MatTooltipModule,
    MatDialogModule,
    MatProgressSpinnerModule,
    MatFormFieldModule,
    MatInputModule,
    ReactiveFormsModule,
    
  ],
  templateUrl: './lesson-list.component.html',
  styleUrl: './lesson-list.component.css',
})
export class LessonListComponent implements OnInit {
  lessons: Lesson[] = [];
  loading = false;
  importing = false;
  exportingLessonId: string | null = null;
  searchControl = new FormControl('');

  readonly displayedColumns = ['title', 'description', 'updatedAt', 'actions'];

  constructor(
    private readonly lessonService: LessonService,
    private readonly snackBar: MatSnackBar,
    private readonly dialog: MatDialog,
  ) {}

  ngOnInit(): void {
    this.loadLessons();

    this.searchControl.valueChanges
      .pipe(debounceTime(250), distinctUntilChanged())
      .subscribe((search) => this.loadLessons(search ?? ''));
  }

  loadLessons(search = this.searchControl.value ?? ''): void {
    this.loading = true;

    this.lessonService.getLessons(search).subscribe({
      next: (data) => {
        this.lessons = data;
        this.loading = false;
      },
      error: (error) => {
        this.loading = false;
        this.snackBar.open(apiErrorMessage(error, 'Failed to load lessons'), 'Close', {
          duration: 3000,
        });
      },
    });
  }

  deleteLesson(lesson: Lesson): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '420px',
      data: {
        title: 'Delete lesson',
        message: 'This lesson will be permanently removed.',
        confirmText: 'Delete',
      },
    });

    dialogRef.afterClosed().subscribe((confirmed: boolean) => {
      if (!confirmed) return;

      this.lessonService.deleteLesson(lesson.lessonId).subscribe({
        next: () => {
          this.snackBar.open('Lesson deleted', 'Close', {
            duration: 2500,
          });
          this.loadLessons();
        },
        error: () => {
          this.snackBar.open('Delete failed', 'Close', {
            duration: 3000,
          });
        },
      });
    });
  }

  exportLesson(lesson: Lesson): void {
    this.exportingLessonId = lesson.lessonId;

    this.lessonService.exportLesson(lesson.lessonId).subscribe({
      next: (blob) => {
        this.exportingLessonId = null;
        const fileName = `${lesson.title?.replace(/[^a-z0-9_-]/gi, '_') || 'lesson'}-${lesson.lessonId}.json`;
        this.downloadBlob(blob, fileName);
      },
      error: () => {
        this.exportingLessonId = null;
        this.snackBar.open('Export failed', 'Close', { duration: 3000 });
      },
    });
  }

  handleLessonImport(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (!file) {
      return;
    }

    this.importing = true;
    this.lessonService.importLesson(file).subscribe({
      next: () => {
        this.importing = false;
        this.snackBar.open('Lesson imported', 'Close', { duration: 3200 });
        this.loadLessons();
      },
      error: (err) => {
        console.error('Import error:', err);
        this.importing = false;
        this.snackBar.open('Import failed', 'Close', { duration: 3000 });
      },
    });
  }

  private downloadBlob(blob: Blob, filename: string): void {
    const url = window.URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = filename;
    document.body.appendChild(anchor);
    anchor.click();
    anchor.remove();
    window.URL.revokeObjectURL(url);
  }
}
