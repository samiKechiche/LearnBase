export interface Tag {
  tagId: string;
  name: string;
  exerciseIds?: string[] | null;
  createdAt: string;
}

export interface SaveTagRequest {
  name: string;
}
