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
  searchControl = new FormControl('');

  readonly displayedColumns = ['title', 'description', 'updatedAt', 'actions'];

  constructor(
    private readonly lessonService: LessonService,
    private readonly snackBar: MatSnackBar,
    private readonly dialog: MatDialog,
  ) {}

  ngOnInit(): void {
    this.loadLessons();
  }

  loadLessons(): void {
    this.loading = true;

    this.lessonService.getLessons().subscribe({
      next: (data) => {
        this.lessons = data;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.snackBar.open('Failed to load lessons', 'Close', {
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
}