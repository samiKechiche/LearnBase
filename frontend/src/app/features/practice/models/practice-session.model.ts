export enum PracticeOrder {
  Default = 0,
  Randomized = 1,
}

export enum ResultStatus {
  Correct = 0,
  Incorrect = 1,
  Skipped = 2,
}

export const RESULT_STATUS_LABELS: Record<ResultStatus, string> = {
  [ResultStatus.Correct]: 'Correct',
  [ResultStatus.Incorrect]: 'Incorrect',
  [ResultStatus.Skipped]: 'Skipped',
};

export interface StartSessionRequest {
  practiceSetId: string;
  practiceOrder: PracticeOrder;
}

export interface SubmitAnswerRequest {
  exerciseId: string;
  userAnswer: string;
}

export interface SkipExerciseRequest {
  exerciseId: string;
}

export interface SessionExerciseResult {
  sessionExerciseResultId: string;
  exerciseId?: string | null;
  question: string;
  userAnswer?: string | null;
  correctAnswer: string;
  resultStatus: ResultStatus;
  orderIndex: number;
}

export interface PracticeSession {
  sessionId: string;
  practiceSetId: string;
  practiceSetTitle: string;
  practiceOrder: PracticeOrder;
  startedAt: string;
  endedAt?: string | null;
  isActive: boolean;
  totalExercises: number;
  correctCount: number;
  incorrectCount: number;
  skippedCount: number;
  answeredCount: number;
  scorePercentage?: number | null;
  results: SessionExerciseResult[];
}

export interface SessionSummary {
  sessionId: string;
  practiceSetTitle: string;
  practiceOrder: PracticeOrder;
  startedAt: string;
  endedAt?: string | null;
  isActive: boolean;
  totalExercises: number;
  correctCount: number;
  incorrectCount: number;
  skippedCount: number;
  scorePercentage?: number | null;
}

export interface PracticeStats {
  message?: string | null;
  totalCompletedSessions: number;
  totalExercisesPracticed: number;
  totalCorrect: number;
  totalIncorrect: number;
  totalSkipped: number;
  overallAccuracyPercentage?: number | null;
  bestPracticeSetName?: string | null;
  bestPracticeSetAccuracy?: number | null;
  averageExercisesPerSession: number;
  lastPracticedAt?: string | null;
}
