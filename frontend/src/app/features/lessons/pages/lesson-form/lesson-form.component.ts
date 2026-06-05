import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';

import { LessonService } from '../../services/lesson.service';

import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';

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
    MatDividerModule
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
  ) {}

  ngOnInit(): void {
    this.lessonId = this.route.snapshot.paramMap.get('id');
    this.isEditMode = !!this.lessonId;
  }

  submit(): void {
    if (this.form.invalid) return;

    this.loading = true;

    const raw = this.form.getRawValue();

    const payload = {
  title: raw.title,
  description: raw.description || undefined
};

    this.lessonService.createLesson(payload).subscribe({
      next: () => {
        this.loading = false;
        this.router.navigate(['/lessons']);
      },
      error: (err) => {
        console.error(err);
        this.loading = false;
      },
    });
  }
}