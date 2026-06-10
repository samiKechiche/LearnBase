import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';

import { apiErrorMessage } from '../../../../core/http/api-error';
import { LessonService } from '../../services/lesson.service';

import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

@Component({
  selector: 'app-lesson-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    

    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatDividerModule,
    MatSnackBarModule
  ],
  templateUrl: './lesson-form.component.html',
  styleUrl: './lesson-form.component.css',
})
export class LessonFormComponent implements OnInit {

  loading = false;

  isEditMode = false;
  lessonId: string | null = null;

  form = new FormGroup({
    title: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    description: new FormControl(''),
  });

  constructor(
    private readonly lessonService: LessonService,
    private readonly router: Router,
    private readonly route: ActivatedRoute,
    private readonly snackBar: MatSnackBar,
  ) {}

  ngOnInit(): void {
    this.lessonId = this.route.snapshot.paramMap.get('id');
    this.isEditMode = !!this.lessonId;

    if (this.lessonId) {
      this.loadLesson(this.lessonId);
    }
  }

  submit(): void {
    if (this.form.invalid) return;

    this.loading = true;

    const raw = this.form.getRawValue();

    const payload = {
      title: raw.title,
      description: raw.description || undefined,
    };

    const request = this.lessonId
      ? this.lessonService.updateLesson(this.lessonId, payload)
      : this.lessonService.createLesson(payload);

    request.subscribe({
      next: () => {
        this.loading = false;
        this.snackBar.open('Lesson saved', 'Close', { duration: 2500 });
        this.router.navigate(['/lessons']);
      },
      error: (error) => {
        this.showError(error);
        this.loading = false;
      },
    });
  }

  private loadLesson(id: string): void {
    this.loading = true;

    this.lessonService.getLesson(id).subscribe({
      next: (lesson) => {
        this.form.patchValue({
          title: lesson.title,
          description: lesson.description ?? '',
        });
        this.loading = false;
      },
      error: (error) => {
        this.showError(error);
        this.loading = false;
      },
    });
  }

  private showError(error: unknown): void {
    const message = apiErrorMessage(error, 'Lesson operation failed.');
    this.snackBar.open(message, 'Close', { duration: 4500 });
  }

  goBack(): void {
    this.router.navigate(['/lessons']);
  }
}