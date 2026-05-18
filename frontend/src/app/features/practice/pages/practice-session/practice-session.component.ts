import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatRadioModule } from '@angular/material/radio';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';

import { apiErrorMessage } from '../../../../core/http/api-error';
import { Exercise, ExerciseOption, ExerciseType } from '../../../exercises/models/exercise.model';
import { ExerciseService } from '../../../exercises/services/exercise.service';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';
import {
  PracticeSession,
  ResultStatus,
  SessionExerciseResult,
} from '../../models/practice-session.model';
import { PracticeSessionService } from '../../services/practice-session.service';

@Component({
  selector: 'app-practice-session',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatDialogModule,
    MatDividerModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressBarModule,
    MatProgressSpinnerModule,
    MatRadioModule,
    MatSnackBarModule,
  ],
  templateUrl: './practice-session.component.html',
  styleUrl: './practice-session.component.css',
})
export class PracticeSessionComponent implements OnInit {
  readonly ExerciseType = ExerciseType;
  readonly ResultStatus = ResultStatus;

  answerControl = new FormControl('', { nonNullable: true, validators: [Validators.required] });
  selectedOptionControl = new FormControl('', {
    nonNullable: true,
    validators: [Validators.required],
  });

  session: PracticeSession | null = null;
  exerciseMap = new Map<string, Exercise>();
  currentIndex = 0;
  loading = false;
  submitting = false;
  skipping = false;
  ending = false;
  showFlashcardAnswer = false;
  private skippedResultIds = new Set<string>();

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly practiceSessionService: PracticeSessionService,
    private readonly exerciseService: ExerciseService,
    private readonly dialog: MatDialog,
    private readonly snackBar: MatSnackBar,
  ) {}

  ngOnInit(): void {
    const sessionId = this.route.snapshot.paramMap.get('id');
    if (!sessionId) {
      this.router.navigateByUrl('/practice/start');
      return;
    }

    this.loadSession(sessionId);
  }

  currentResult(): SessionExerciseResult | null {
    return this.session?.results[this.currentIndex] ?? null;
  }

  currentExercise(): Exercise | undefined {
    const exerciseId = this.currentResult()?.exerciseId;
    return exerciseId ? this.exerciseMap.get(exerciseId) : undefined;
  }

  currentOptions(): ExerciseOption[] {
    return [...(this.currentExercise()?.options ?? [])].sort((a, b) => a.orderIndex - b.orderIndex);
  }

  currentExerciseType(): ExerciseType {
    return this.currentExercise()?.type ?? ExerciseType.FillBlank;
  }

  completedCount(): number {
    return this.session?.results.filter((result) => this.isComplete(result)).length ?? 0;
  }

  correctCount(): number {
    return (
      this.session?.results.filter((result) => result.resultStatus === ResultStatus.Correct)
        .length ?? 0
    );
  }

  incorrectCount(): number {
    return (
      this.session?.results.filter((result) => result.resultStatus === ResultStatus.Incorrect)
        .length ?? 0
    );
  }

  skippedCount(): number {
    return (
      this.session?.results.filter((result) =>
        this.skippedResultIds.has(result.sessionExerciseResultId),
      ).length ?? 0
    );
  }

  progressValue(): number {
    const total = this.session?.totalExercises ?? 0;
    return total === 0 ? 0 : Math.round((this.completedCount() / total) * 100);
  }

  isComplete(result: SessionExerciseResult): boolean {
    return (
      result.resultStatus === ResultStatus.Correct ||
      result.resultStatus === ResultStatus.Incorrect ||
      this.skippedResultIds.has(result.sessionExerciseResultId)
    );
  }

  submitAnswer(): void {
    const result = this.currentResult();
    if (!this.session || !result?.exerciseId || this.isComplete(result)) {
      return;
    }

    const answer =
      this.currentExerciseType() === ExerciseType.MCQ
        ? this.selectedOptionControl.value
        : this.answerControl.value.trim();

    if (!answer) {
      this.answerControl.markAsTouched();
      this.selectedOptionControl.markAsTouched();
      return;
    }

    this.submitting = true;
    this.practiceSessionService
      .submitAnswer(this.session.sessionId, {
        exerciseId: result.exerciseId,
        userAnswer: answer,
      })
      .subscribe({
        next: (updatedResult) => {
          this.replaceResult(updatedResult);
          this.submitting = false;
          this.snackBar.open(
            updatedResult.resultStatus === ResultStatus.Correct ? 'Correct' : 'Incorrect',
            'Close',
            {
              duration: 1800,
            },
          );
          this.goToNextPending();
        },
        error: (error) => this.showError(error),
      });
  }

  submitFlashcard(knewAnswer: boolean): void {
    const result = this.currentResult();
    if (!result || this.isComplete(result) || this.submitting) {
      return;
    }

    this.answerControl.setValue(knewAnswer ? result.correctAnswer : '[missed]');
    this.submitAnswer();
  }

  skipCurrent(): void {
    const result = this.currentResult();
    if (!this.session || !result?.exerciseId || this.isComplete(result)) {
      return;
    }

    this.skipping = true;
    this.practiceSessionService
      .skipExercise(this.session.sessionId, { exerciseId: result.exerciseId })
      .subscribe({
        next: (updatedResult) => {
          this.replaceResult(updatedResult);
          this.skippedResultIds.add(updatedResult.sessionExerciseResultId);
          this.saveSkippedIds();
          this.skipping = false;
          this.goToNextPending();
        },
        error: (error) => this.showError(error),
      });
  }

  goToPrevious(): void {
    this.currentIndex = Math.max(0, this.currentIndex - 1);
    this.resetAnswerControls();
  }

  goToNext(): void {
    if (!this.session) {
      return;
    }

    this.currentIndex = Math.min(this.session.results.length, this.currentIndex + 1);
    this.resetAnswerControls();
  }

  endSession(confirm = true): void {
    if (!this.session) {
      return;
    }

    if (!confirm) {
      this.finishSession();
      return;
    }

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '430px',
      data: {
        title: 'End session',
        message: 'Your current progress will be saved in practice history.',
        confirmText: 'End session',
        color: 'primary',
      },
    });

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (!confirmed || !this.session) {
        return;
      }

      this.finishSession();
    });
  }

  resultStatusLabel(result: SessionExerciseResult): string {
    if (result.resultStatus === ResultStatus.Correct) {
      return 'Correct';
    }

    if (result.resultStatus === ResultStatus.Incorrect) {
      return 'Incorrect';
    }

    return 'Skipped';
  }

  private loadSession(sessionId: string): void {
    this.loading = true;
    this.loadSkippedIds(sessionId);

    forkJoin({
      session: this.practiceSessionService.getSession(sessionId),
      exercises: this.exerciseService.getExercises(),
    }).subscribe({
      next: ({ session, exercises }) => {
        if (!session.isActive) {
          this.router.navigate(['/practice/results', session.sessionId]);
          return;
        }

        this.session = {
          ...session,
          results: [...session.results].sort((a, b) => a.orderIndex - b.orderIndex),
        };
        this.exerciseMap = new Map(exercises.map((exercise) => [exercise.exerciseId, exercise]));
        this.currentIndex = this.firstPendingIndex();
        this.resetAnswerControls();
        this.loading = false;
      },
      error: (error) => this.showError(error),
    });
  }

  private replaceResult(updatedResult: SessionExerciseResult): void {
    if (!this.session) {
      return;
    }

    this.session = {
      ...this.session,
      results: this.session.results.map((result) =>
        result.sessionExerciseResultId === updatedResult.sessionExerciseResultId
          ? updatedResult
          : result,
      ),
    };
  }

  private goToNextPending(): void {
    this.currentIndex = this.firstPendingIndex(this.currentIndex + 1);
    this.resetAnswerControls();
  }

  private firstPendingIndex(startIndex = 0): number {
    if (!this.session) {
      return 0;
    }

    const results = this.session.results;
    const orderedIndexes = [...results.keys()]
      .slice(startIndex)
      .concat([...results.keys()].slice(0, startIndex));
    const nextIndex = orderedIndexes.find((index) => !this.isComplete(results[index]));

    return nextIndex ?? results.length;
  }

  private resetAnswerControls(): void {
    this.answerControl.reset('');
    this.selectedOptionControl.reset('');
    this.showFlashcardAnswer = false;
  }

  private loadSkippedIds(sessionId: string): void {
    const stored = sessionStorage.getItem(this.storageKey(sessionId));
    try {
      this.skippedResultIds = new Set(stored ? (JSON.parse(stored) as string[]) : []);
    } catch {
      this.skippedResultIds = new Set<string>();
    }
  }

  private saveSkippedIds(): void {
    if (!this.session) {
      return;
    }

    sessionStorage.setItem(
      this.storageKey(this.session.sessionId),
      JSON.stringify([...this.skippedResultIds]),
    );
  }

  private storageKey(sessionId: string): string {
    return `learnbase_skipped_${sessionId}`;
  }

  private finishSession(): void {
    if (!this.session || this.ending) {
      return;
    }

    this.ending = true;
    this.practiceSessionService.endSession(this.session.sessionId).subscribe({
      next: (session) => {
        sessionStorage.removeItem(this.storageKey(session.sessionId));
        this.router.navigate(['/practice/results', session.sessionId]);
      },
      error: (error) => this.showError(error),
    });
  }

  private showError(error: unknown): void {
    this.loading = false;
    this.submitting = false;
    this.skipping = false;
    this.ending = false;
    const message = apiErrorMessage(error, 'Something went wrong during the practice session.');
    this.snackBar.open(message, 'Close', { duration: 4500 });
  }
}
