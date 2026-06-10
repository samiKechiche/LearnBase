export interface Lesson {
  lessonId: string;
  title: string;
  description?: string;
  userId: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateLessonRequest {
  title: string;
  description?: string;
}

export interface UpdateLessonRequest {
  title: string;
  description?: string;
}
export interface AppFile {
  fileId: string;
  fileName: string;
  fileType: string;
  fileSizeBytes: number;
  uploadedAt: string;
  lessonId: string;
}