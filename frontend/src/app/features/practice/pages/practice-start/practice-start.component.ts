import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { ReactiveFormsModule, UntypedFormBuilder, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatRadioModule } from '@angular/material/radio';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Router, RouterLink } from '@angular/router';

import { PracticeSet } from '../../models/practice-set.model';
import { PracticeOrder } from '../../models/practice-session.model';
import { PracticeSetService } from '../../services/practice-set.service';
import { PracticeSessionService } from '../../services/practice-session.service';

@Component({
  selector: 'app-practice-start',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatRadioModule,
    MatSelectModule,
    MatSnackBarModule,
  ],
  templateUrl: './practice-start.component.html',
  styleUrl: './practice-start.component.css',
})
export class PracticeStartComponent implements OnInit {
  private readonly fb = inject(UntypedFormBuilder);

  readonly PracticeOrder = PracticeOrder;

  form = this.fb.group({
    practiceSetId: ['', Validators.required],
    practiceOrder: [PracticeOrder.Default, Validators.required],
  });

  practiceSets: PracticeSet[] = [];
  loading = false;
  starting = false;

  constructor(
    private readonly practiceSetService: PracticeSetService,
    private readonly practiceSessionService: PracticeSessionService,
    private readonly router: Router,
    private readonly snackBar: MatSnackBar,
  ) {}

  ngOnInit(): void {
    this.loading = true;

    this.practiceSetService.getPracticeSets().subscribe({
      next: (practiceSets) => {
        this.practiceSets = practiceSets;
        const firstReadySet = practiceSets.find((set) => set.exerciseCount > 0);
        if (firstReadySet) {
          this.form.patchValue({ practiceSetId: firstReadySet.practiceSetId });
        }
        this.loading = false;
      },
      error: (error) => this.showError(error),
    });
  }

  startSession(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) {
      return;
    }

    this.starting = true;

    this.practiceSessionService
      .startSession({
        practiceSetId: String(this.form.value.practiceSetId),
        practiceOrder: Number(this.form.value.practiceOrder) as PracticeOrder,
      })
      .subscribe({
        next: (session) => {
          sessionStorage.removeItem(`learnbase_skipped_${session.sessionId}`);
          this.router.navigate(['/practice/session', session.sessionId]);
        },
        error: (error) => this.showError(error),
      });
  }

  selectedSet(): PracticeSet | undefined {
    return this.practiceSets.find((set) => set.practiceSetId === this.form.value.practiceSetId);
  }

  private showError(error: unknown): void {
    this.loading = false;
    this.starting = false;
    const message =
      error instanceof Error ? error.message : 'Something went wrong while starting practice.';
    this.snackBar.open(message, 'Close', { duration: 4500 });
  }
}
