export enum ExerciseType {
  MCQ = 0,
  FillBlank = 1,
  Flashcard = 2,
}

export const EXERCISE_TYPE_LABELS: Record<ExerciseType, string> = {
  [ExerciseType.MCQ]: 'MCQ',
  [ExerciseType.FillBlank]: 'Fill blank',
  [ExerciseType.Flashcard]: 'Flashcard',
};

export interface ExerciseOptionInput {
  content: string;
  orderIndex: number;
}

export interface ExerciseOption extends ExerciseOptionInput {
  optionId: string;
}

export interface Exercise {
  exerciseId: string;
  type: ExerciseType;
  question: string;
  answer: string;
  correctOptionId?: string | null;
  options?: ExerciseOption[] | null;
  tagIds?: string[] | null;
  createdAt: string;
  updatedAt: string;
}

export interface SaveExerciseRequest {
  type: ExerciseType;
  question: string;
  answer: string;
  options?: ExerciseOptionInput[] | null;
  correctOptionIndex?: number | null;
}
