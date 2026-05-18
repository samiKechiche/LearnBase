import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import {
  FormControl,
  ReactiveFormsModule,
  UntypedFormArray,
  UntypedFormBuilder,
  UntypedFormGroup,
  Validators,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatRadioModule } from '@angular/material/radio';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { map, switchMap } from 'rxjs';

import {
  EXERCISE_TYPE_LABELS,
  Exercise,
  ExerciseType,
  SaveExerciseRequest,
} from '../../models/exercise.model';
import { Tag } from '../../models/tag.model';
import { ExerciseService } from '../../services/exercise.service';
import { TagService } from '../../services/tag.service';

@Component({
  selector: 'app-exercise-form',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatChipsModule,
    MatDividerModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatRadioModule,
    MatSelectModule,
    MatSnackBarModule,
  ],
  templateUrl: './exercise-form.component.html',
  styleUrl: './exercise-form.component.css',
})
export class ExerciseFormComponent implements OnInit {
  private readonly fb = inject(UntypedFormBuilder);

  readonly ExerciseType = ExerciseType;
  readonly typeOptions = [
    { value: ExerciseType.MCQ, label: EXERCISE_TYPE_LABELS[ExerciseType.MCQ] },
    { value: ExerciseType.FillBlank, label: EXERCISE_TYPE_LABELS[ExerciseType.FillBlank] },
    { value: ExerciseType.Flashcard, label: EXERCISE_TYPE_LABELS[ExerciseType.Flashcard] },
  ];
  readonly newTagControl = new FormControl('', { nonNullable: true });

  form: UntypedFormGroup = this.fb.group({
    type: [ExerciseType.MCQ, Validators.required],
    question: ['', [Validators.required, Validators.maxLength(2000)]],
    answer: ['', [Validators.maxLength(1000)]],
    correctOptionIndex: [1],
    tagIds: [[]],
    options: this.fb.array([]),
  });

  tags: Tag[] = [];
  loading = false;
  saving = false;
  exerciseId: string | null = null;
  private currentTagIds: string[] = [];

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly exerciseService: ExerciseService,
    private readonly tagService: TagService,
    private readonly snackBar: MatSnackBar,
  ) {}

  get isEditMode(): boolean {
    return Boolean(this.exerciseId);
  }

  get isMcq(): boolean {
    return Number(this.form.get('type')?.value) === ExerciseType.MCQ;
  }

  get options(): UntypedFormArray {
    return this.form.get('options') as UntypedFormArray;
  }

  ngOnInit(): void {
    this.exerciseId = this.route.snapshot.paramMap.get('id');
    this.loadTags();
    this.watchTypeChanges();

    if (this.exerciseId) {
      this.loadExercise(this.exerciseId);
    } else {
      this.ensureMinimumOptions();
    }
  }

  addOption(content = '', orderIndex = this.options.length + 1): void {
    if (this.options.length >= 5) {
      return;
    }

    this.options.push(
      this.fb.group({
        content: [content, [Validators.required, Validators.maxLength(500)]],
        orderIndex: [orderIndex],
      }),
    );

    this.normalizeOrderIndexes();
  }

  removeOption(index: number): void {
    if (this.options.length <= 2) {
      return;
    }

    this.options.removeAt(index);
    this.normalizeOrderIndexes();

    const currentCorrectIndex = Number(this.form.get('correctOptionIndex')?.value);
    if (currentCorrectIndex > this.options.length) {
      this.form.patchValue({ correctOptionIndex: 1 });
    }
  }

  createTag(): void {
    const name = this.newTagControl.value.trim();
    if (!name) {
      return;
    }

    this.tagService.createTag({ name }).subscribe({
      next: (tag) => {
        this.tags = [...this.tags, tag].sort((a, b) => a.name.localeCompare(b.name));
        const selected = new Set(this.selectedTagIds());
        selected.add(tag.tagId);
        this.form.patchValue({ tagIds: Array.from(selected) });
        this.newTagControl.reset('');
        this.snackBar.open('Tag created', 'Close', { duration: 2200 });
      },
      error: (error) => this.showError(error),
    });
  }

  save(): void {
    if (!this.validateBeforeSave()) {
      return;
    }

    const payload = this.buildPayload();
    const selectedTagIds = this.selectedTagIds();
    const saveRequest = this.exerciseId
      ? this.exerciseService.updateExercise(this.exerciseId, payload)
      : this.exerciseService.createExercise(payload);

    this.saving = true;
    saveRequest
      .pipe(
        switchMap((exercise) =>
          this.exerciseService
            .syncExerciseTags(exercise.exerciseId, this.currentTagIds, selectedTagIds)
            .pipe(map(() => exercise)),
        ),
      )
      .subscribe({
        next: () => {
          this.saving = false;
          this.snackBar.open('Exercise saved', 'Close', { duration: 2500 });
          this.router.navigateByUrl('/exercises');
        },
        error: (error) => this.showError(error),
      });
  }

  private loadTags(): void {
    this.tagService.getTags().subscribe({
      next: (tags) => (this.tags = tags),
      error: (error) => this.showError(error),
    });
  }

  private loadExercise(exerciseId: string): void {
    this.loading = true;

    this.exerciseService.getExercise(exerciseId).subscribe({
      next: (exercise) => {
        this.patchExercise(exercise);
        this.loading = false;
      },
      error: (error) => this.showError(error),
    });
  }

  private patchExercise(exercise: Exercise): void {
    this.currentTagIds = exercise.tagIds ?? [];
    this.options.clear();

    this.form.patchValue({
      type: exercise.type,
      question: exercise.question,
      answer: exercise.answer,
      tagIds: this.currentTagIds,
      correctOptionIndex: 1,
    });

    if (exercise.type === ExerciseType.MCQ) {
      const options = [...(exercise.options ?? [])].sort((a, b) => a.orderIndex - b.orderIndex);
      for (const option of options) {
        this.addOption(option.content, option.orderIndex);
      }
      this.ensureMinimumOptions();

      const correctOption = options.find((option) => option.optionId === exercise.correctOptionId);
      const fallbackOption = options.find((option) => option.content === exercise.answer);
      this.form.patchValue({
        correctOptionIndex: correctOption?.orderIndex ?? fallbackOption?.orderIndex ?? 1,
      });
    }
  }

  private watchTypeChanges(): void {
    this.form.get('type')?.valueChanges.subscribe((type) => {
      if (Number(type) === ExerciseType.MCQ) {
        this.ensureMinimumOptions();
      }
    });
  }

  private ensureMinimumOptions(): void {
    while (this.options.length < 2) {
      this.addOption();
    }
  }

  private normalizeOrderIndexes(): void {
    this.options.controls.forEach((control, index) => {
      control.get('orderIndex')?.setValue(index + 1);
    });
  }

  private validateBeforeSave(): boolean {
    this.form.markAllAsTouched();

    if (this.isMcq) {
      this.ensureMinimumOptions();
      this.options.markAllAsTouched();

      if (this.form.get('question')?.invalid || this.options.invalid) {
        return false;
      }

      return true;
    }

    const answer = String(this.form.get('answer')?.value ?? '').trim();
    if (!answer) {
      this.form.get('answer')?.setErrors({ required: true });
      return false;
    }

    return this.form.valid;
  }

  private buildPayload(): SaveExerciseRequest {
    const question = String(this.form.get('question')?.value ?? '').trim();
    const type = Number(this.form.get('type')?.value) as ExerciseType;

    if (type === ExerciseType.MCQ) {
      const options = this.options.controls.map((control) => ({
        content: String(control.get('content')?.value ?? '').trim(),
        orderIndex: Number(control.get('orderIndex')?.value),
      }));
      const correctOptionIndex = Number(this.form.get('correctOptionIndex')?.value);
      const correctOption = options.find((option) => option.orderIndex === correctOptionIndex);

      return {
        type,
        question,
        answer: correctOption?.content ?? options[0]?.content ?? '',
        options,
        correctOptionIndex,
      };
    }

    return {
      type,
      question,
      answer: String(this.form.get('answer')?.value ?? '').trim(),
      options: null,
      correctOptionIndex: null,
    };
  }

  private selectedTagIds(): string[] {
    return (this.form.get('tagIds')?.value ?? []) as string[];
  }

  private showError(error: unknown): void {
    this.loading = false;
    this.saving = false;
    const message =
      error instanceof Error ? error.message : 'Something went wrong while saving the exercise.';
    this.snackBar.open(message, 'Close', { duration: 4500 });
  }
}
