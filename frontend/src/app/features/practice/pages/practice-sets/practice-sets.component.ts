import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormControl, ReactiveFormsModule, UntypedFormBuilder, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatListModule } from '@angular/material/list';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';

import { apiErrorMessage } from '../../../../core/http/api-error';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';
import {
  EXERCISE_TYPE_LABELS,
  Exercise,
  ExerciseType,
} from '../../../exercises/models/exercise.model';
import { Tag } from '../../../exercises/models/tag.model';
import { ExerciseService } from '../../../exercises/services/exercise.service';
import { TagService } from '../../../exercises/services/tag.service';
import {
  CreationType,
  ExerciseTypeSummary,
  PracticeSet,
  PracticeSetExerciseSummary,
} from '../../models/practice-set.model';
import { PracticeSetService } from '../../services/practice-set.service';

@Component({
  selector: 'app-practice-sets',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatChipsModule,
    MatDialogModule,
    MatDividerModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatListModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatSnackBarModule,
    MatTooltipModule,
  ],
  templateUrl: './practice-sets.component.html',
  styleUrl: './practice-sets.component.css',
})
export class PracticeSetsComponent implements OnInit {
  private readonly fb = inject(UntypedFormBuilder);

  readonly selectedExerciseControl = new FormControl('', { nonNullable: true });

  setForm = this.fb.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    description: [''],
  });

  generateForm = this.fb.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    description: [''],
    tagIds: [[]],
  });

  practiceSets: PracticeSet[] = [];
  exercises: Exercise[] = [];
  tags: Tag[] = [];
  selectedSet: PracticeSet | null = null;
  editingSetId: string | null = null;
  loading = false;
  saving = false;

  constructor(
    private readonly practiceSetService: PracticeSetService,
    private readonly exerciseService: ExerciseService,
    private readonly tagService: TagService,
    private readonly dialog: MatDialog,
    private readonly snackBar: MatSnackBar,
  ) {}

  ngOnInit(): void {
    this.loadPageData();
  }

  loadPageData(): void {
    this.loading = true;

    forkJoin({
      practiceSets: this.practiceSetService.getPracticeSets(),
      exercises: this.exerciseService.getExercises(),
      tags: this.tagService.getTags(),
    }).subscribe({
      next: ({ practiceSets, exercises, tags }) => {
        this.practiceSets = practiceSets;
        this.exercises = exercises;
        this.tags = tags;
        this.selectedSet =
          practiceSets.find((set) => set.practiceSetId === this.selectedSet?.practiceSetId) ??
          practiceSets[0] ??
          null;
        this.loading = false;
      },
      error: (error) => this.showError(error),
    });
  }

  selectSet(practiceSet: PracticeSet): void {
    this.selectedSet = practiceSet;
    this.selectedExerciseControl.reset('');
  }

  startEdit(practiceSet: PracticeSet): void {
    this.editingSetId = practiceSet.practiceSetId;
    this.setForm.patchValue({
      title: practiceSet.title,
      description: practiceSet.description ?? '',
    });
  }

  cancelEdit(): void {
    this.editingSetId = null;
    this.setForm.reset({ title: '', description: '' });
  }

  saveSet(): void {
    this.setForm.markAllAsTouched();
    if (this.setForm.invalid) {
      return;
    }

    const payload = {
      title: String(this.setForm.value.title ?? '').trim(),
      description: String(this.setForm.value.description ?? '').trim() || null,
      creationType: CreationType.Manual,
    };

    const request = this.editingSetId
      ? this.practiceSetService.updatePracticeSet(this.editingSetId, payload)
      : this.practiceSetService.createPracticeSet(payload);

    this.saving = true;
    request.subscribe({
      next: (practiceSet) => {
        this.saving = false;
        this.snackBar.open('Practice set saved', 'Close', { duration: 2400 });
        this.cancelEdit();
        this.selectedSet = practiceSet;
        this.loadPageData();
      },
      error: (error) => this.showError(error),
    });
  }

  deleteSet(practiceSet: PracticeSet): void {
    if (!this.canDeleteSet(practiceSet)) {
      this.snackBar.open('Delete this set after removing its session history.', 'Close', {
        duration: 3500,
      });
      return;
    }

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '430px',
      data: {
        title: 'Delete practice set',
        message: 'Practice sets with session history cannot be deleted.',
        confirmText: 'Delete',
      },
    });

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (!confirmed) {
        return;
      }

      this.practiceSetService.deletePracticeSet(practiceSet.practiceSetId).subscribe({
        next: () => {
          this.snackBar.open('Practice set deleted', 'Close', { duration: 2400 });
          this.selectedSet = null;
          this.loadPageData();
        },
        error: (error) => this.showError(error),
      });
    });
  }

  addExercise(): void {
    if (!this.selectedSet || !this.selectedExerciseControl.value) {
      return;
    }

    this.practiceSetService
      .addExercise(this.selectedSet.practiceSetId, {
        exerciseId: this.selectedExerciseControl.value,
      })
      .subscribe({
        next: (practiceSet) => {
          this.selectedSet = practiceSet;
          this.selectedExerciseControl.reset('');
          this.snackBar.open('Exercise added', 'Close', { duration: 2200 });
          this.loadPageData();
        },
        error: (error) => this.showError(error),
      });
  }

  removeExercise(exercise: PracticeSetExerciseSummary): void {
    if (!this.selectedSet) {
      return;
    }

    this.practiceSetService
      .removeExercise(this.selectedSet.practiceSetId, exercise.exerciseId)
      .subscribe({
        next: () => {
          this.snackBar.open('Exercise removed', 'Close', { duration: 2200 });
          this.loadPageData();
        },
        error: (error) => this.showError(error),
      });
  }

  generateFromTags(): void {
    this.generateForm.markAllAsTouched();
    if (this.generateForm.invalid) {
      return;
    }

    const tagIds = (this.generateForm.value.tagIds ?? []) as string[];
    if (tagIds.length === 0) {
      this.snackBar.open('Select at least one tag', 'Close', { duration: 3000 });
      return;
    }

    this.practiceSetService
      .generateFromTags({
        title: String(this.generateForm.value.title ?? '').trim(),
        description: String(this.generateForm.value.description ?? '').trim() || null,
        tagIds,
      })
      .subscribe({
        next: (practiceSet) => {
          this.selectedSet = practiceSet;
          this.generateForm.reset({ title: '', description: '', tagIds: [] });
          this.snackBar.open('Practice set generated', 'Close', { duration: 2400 });
          this.loadPageData();
        },
        error: (error) => this.showError(error),
      });
  }

  availableExercises(): Exercise[] {
    const usedIds = new Set(
      this.selectedSet?.exercises?.map((exercise) => exercise.exerciseId) ?? [],
    );
    return this.exercises.filter((exercise) => !usedIds.has(exercise.exerciseId));
  }

  canDeleteSet(practiceSet: PracticeSet): boolean {
    return practiceSet.sessionCount === 0;
  }

  exerciseTypeLabel(type: ExerciseType | ExerciseTypeSummary): string {
    return EXERCISE_TYPE_LABELS[type as ExerciseType] ?? 'Exercise';
  }

  private showError(error: unknown): void {
    this.loading = false;
    this.saving = false;
    const message = apiErrorMessage(error, 'Something went wrong while loading practice sets.');
    this.snackBar.open(message, 'Close', { duration: 4500 });
  }
}
