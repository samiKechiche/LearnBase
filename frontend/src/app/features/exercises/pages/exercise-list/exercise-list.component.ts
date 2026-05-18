import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterLink } from '@angular/router';
import { debounceTime, distinctUntilChanged } from 'rxjs';

import { apiErrorMessage } from '../../../../core/http/api-error';
import { EXERCISE_TYPE_LABELS, Exercise, ExerciseType } from '../../models/exercise.model';
import { Tag } from '../../models/tag.model';
import { ExerciseService } from '../../services/exercise.service';
import { TagService } from '../../services/tag.service';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-exercise-list',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    MatButtonModule,
    MatChipsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatTableModule,
    MatTooltipModule,
  ],
  templateUrl: './exercise-list.component.html',
  styleUrl: './exercise-list.component.css',
})
export class ExerciseListComponent implements OnInit {
  readonly searchControl = new FormControl('', { nonNullable: true });
  readonly displayedColumns = ['question', 'type', 'tags', 'updatedAt', 'actions'];
  readonly typeLabels = EXERCISE_TYPE_LABELS;

  exercises: Exercise[] = [];
  tags: Tag[] = [];
  loading = false;

  constructor(
    private readonly exerciseService: ExerciseService,
    private readonly tagService: TagService,
    private readonly dialog: MatDialog,
    private readonly snackBar: MatSnackBar,
  ) {}

  ngOnInit(): void {
    this.loadTags();
    this.loadExercises();

    this.searchControl.valueChanges
      .pipe(debounceTime(250), distinctUntilChanged())
      .subscribe((search) => this.loadExercises(search));
  }

  loadExercises(search = this.searchControl.value): void {
    this.loading = true;

    this.exerciseService.getExercises(search).subscribe({
      next: (exercises) => {
        this.exercises = exercises;
        this.loading = false;
      },
      error: (error) => this.showError(error),
    });
  }

  loadTags(): void {
    this.tagService.getTags().subscribe({
      next: (tags) => (this.tags = tags),
      error: (error) => this.showError(error),
    });
  }

  deleteExercise(exercise: Exercise): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '420px',
      data: {
        title: 'Delete exercise',
        message: 'This exercise will be removed from your exercise bank.',
        confirmText: 'Delete',
      },
    });

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (!confirmed) {
        return;
      }

      this.exerciseService.deleteExercise(exercise.exerciseId).subscribe({
        next: () => {
          this.snackBar.open('Exercise deleted', 'Close', { duration: 2500 });
          this.loadExercises();
        },
        error: (error) => this.showError(error),
      });
    });
  }

  tagNames(exercise: Exercise): string[] {
    const ids = new Set(exercise.tagIds ?? []);
    return this.tags.filter((tag) => ids.has(tag.tagId)).map((tag) => tag.name);
  }

  typeLabel(type: ExerciseType): string {
    return this.typeLabels[type] ?? 'Exercise';
  }

  hasSearch(): boolean {
    return Boolean(this.searchControl.value.trim());
  }

  private showError(error: unknown): void {
    this.loading = false;
    const message = apiErrorMessage(error, 'Something went wrong while loading exercises.');
    this.snackBar.open(message, 'Close', { duration: 4500 });
  }
}
