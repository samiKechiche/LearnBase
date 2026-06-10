import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';

import { apiErrorMessage } from '../../../../core/http/api-error';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';
import {
  PracticeSession,
  RESULT_STATUS_LABELS,
  ResultStatus,
  SessionExerciseResult,
} from '../../models/practice-session.model';
import { PracticeSessionService } from '../../services/practice-session.service';

@Component({
  selector: 'app-session-detail',
  imports: [
    CommonModule,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatChipsModule,
    MatDialogModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatTableModule,
  ],
  templateUrl: './session-detail.component.html',
  styleUrl: './session-detail.component.css',
})
export class SessionDetailComponent implements OnInit {
  readonly displayedColumns = ['question', 'userAnswer', 'correctAnswer', 'resultStatus'];
  readonly statusLabels = RESULT_STATUS_LABELS;
  readonly ResultStatus = ResultStatus;

  session: PracticeSession | null = null;
  loading = false;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly practiceSessionService: PracticeSessionService,
    private readonly dialog: MatDialog,
    private readonly snackBar: MatSnackBar,
  ) {}

  ngOnInit(): void {
    const sessionId = this.route.snapshot.paramMap.get('id');
    if (!sessionId) {
      this.router.navigateByUrl('/practice/history');
      return;
    }

    this.loadSession(sessionId);
  }

  deleteSession(): void {
    if (!this.session) {
      return;
    }

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '420px',
      data: {
        title: 'Delete session',
        message: 'This practice history entry will be removed.',
        confirmText: 'Delete',
      },
    });

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (!confirmed || !this.session) {
        return;
      }

      this.practiceSessionService.deleteSession(this.session.sessionId).subscribe({
        next: () => {
          this.snackBar.open('Session deleted', 'Close', { duration: 2400 });
          this.router.navigateByUrl('/practice/history');
        },
        error: (error) => this.showError(error),
      });
    });
  }

  statusClass(status: ResultStatus): string {
    return `status-${status}`;
  }

  statusLabel(result: SessionExerciseResult): string {
    if (
      this.session?.isActive &&
      result.resultStatus === ResultStatus.Skipped &&
      !result.userAnswer
    ) {
      return 'Pending';
    }

    return this.statusLabels[result.resultStatus] ?? 'Skipped';
  }

  answerLabel(result: SessionExerciseResult): string {
    if (
      this.session?.isActive &&
      result.resultStatus === ResultStatus.Skipped &&
      !result.userAnswer
    ) {
      return 'Not answered';
    }

    return result.userAnswer || 'Skipped';
  }

  scoreLabel(): string {
    if (this.session?.isActive) {
      return 'In progress';
    }

    return this.session?.scorePercentage === null || this.session?.scorePercentage === undefined
      ? 'No score'
      : `${this.session.scorePercentage}%`;
  }

  private loadSession(sessionId: string): void {
    this.loading = true;

    this.practiceSessionService.getSession(sessionId).subscribe({
      next: (session) => {
        this.session = {
          ...session,
          results: [...session.results].sort((a, b) => a.orderIndex - b.orderIndex),
        };
        this.loading = false;
      },
      error: (error) => this.showError(error),
    });
  }

  private showError(error: unknown): void {
    this.loading = false;
    const message = apiErrorMessage(error, 'Something went wrong while loading session details.');
    this.snackBar.open(message, 'Close', { duration: 4500 });
  }
}
