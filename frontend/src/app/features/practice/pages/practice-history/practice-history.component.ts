import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';

import { apiErrorMessage } from '../../../../core/http/api-error';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';
import { PracticeStats, SessionSummary } from '../../models/practice-session.model';
import { PracticeSessionService } from '../../services/practice-session.service';

@Component({
  selector: 'app-practice-history',
  imports: [
    CommonModule,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatDialogModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatTableModule,
    MatTooltipModule,
  ],
  templateUrl: './practice-history.component.html',
  styleUrl: './practice-history.component.css',
})
export class PracticeHistoryComponent implements OnInit {
  readonly displayedColumns = [
    'practiceSetTitle',
    'startedAt',
    'score',
    'counts',
    'status',
    'actions',
  ];

  stats: PracticeStats | null = null;
  sessions: SessionSummary[] = [];
  loading = false;

  constructor(
    private readonly practiceSessionService: PracticeSessionService,
    private readonly dialog: MatDialog,
    private readonly snackBar: MatSnackBar,
  ) {}

  ngOnInit(): void {
    this.loadHistory();
  }

  loadHistory(): void {
    this.loading = true;

    forkJoin({
      stats: this.practiceSessionService.getStats(),
      sessions: this.practiceSessionService.getSessions(),
    }).subscribe({
      next: ({ stats, sessions }) => {
        this.stats = stats;
        this.sessions = sessions;
        this.loading = false;
      },
      error: (error) => this.showError(error),
    });
  }

  deleteSession(session: SessionSummary): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '420px',
      data: {
        title: 'Delete session',
        message: 'This practice history entry will be removed.',
        confirmText: 'Delete',
      },
    });

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (!confirmed) {
        return;
      }

      this.practiceSessionService.deleteSession(session.sessionId).subscribe({
        next: () => {
          this.snackBar.open('Session deleted', 'Close', { duration: 2400 });
          this.loadHistory();
        },
        error: (error) => this.showError(error),
      });
    });
  }

  deleteAllSessions(): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '440px',
      data: {
        title: 'Delete all sessions',
        message: 'All practice history entries for your account will be removed.',
        confirmText: 'Delete all',
      },
    });

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (!confirmed) {
        return;
      }

      this.practiceSessionService.deleteAllSessions().subscribe({
        next: () => {
          this.snackBar.open('Practice history deleted', 'Close', { duration: 2400 });
          this.loadHistory();
        },
        error: (error) => this.showError(error),
      });
    });
  }

  scoreLabel(session: SessionSummary): string {
    if (session.isActive) {
      return 'In progress';
    }

    return session.scorePercentage === null || session.scorePercentage === undefined
      ? 'No score'
      : `${session.scorePercentage}%`;
  }

  accuracyLabel(): string {
    return this.stats?.overallAccuracyPercentage === null ||
      this.stats?.overallAccuracyPercentage === undefined
      ? 'No score'
      : `${this.stats.overallAccuracyPercentage}%`;
  }

  resultSummary(session: SessionSummary): string {
    if (session.isActive) {
      const answered = session.correctCount + session.incorrectCount;
      return `${answered} answered of ${session.totalExercises}`;
    }

    return `${session.correctCount} correct, ${session.incorrectCount} incorrect, ${session.skippedCount} skipped`;
  }

  private showError(error: unknown): void {
    this.loading = false;
    const message = apiErrorMessage(error, 'Something went wrong while loading history.');
    this.snackBar.open(message, 'Close', { duration: 4500 });
  }
}
