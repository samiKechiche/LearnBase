import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'exercises', pathMatch: 'full' },

  // =======================
  // EXERCISES
  // =======================
  {
    path: 'exercises',
    loadComponent: () =>
      import('./features/exercises/pages/exercise-list/exercise-list.component').then(
        (component) => component.ExerciseListComponent,
      ),
  },
  {
    path: 'exercises/new',
    loadComponent: () =>
      import('./features/exercises/pages/exercise-form/exercise-form.component').then(
        (component) => component.ExerciseFormComponent,
      ),
  },
  {
    path: 'exercises/:id/edit',
    loadComponent: () =>
      import('./features/exercises/pages/exercise-form/exercise-form.component').then(
        (component) => component.ExerciseFormComponent,
      ),
  },

  // =======================
  // LESSONS
  // =======================
  {
    path: 'lessons',
    loadComponent: () =>
      import('./features/lessons/pages/lesson-list/lesson-list.component').then(
        (c) => c.LessonListComponent,
      ),
  },
  {
    path: 'lessons/new',
    loadComponent: () =>
      import('./features/lessons/pages/lesson-form/lesson-form.component').then(
        (c) => c.LessonFormComponent,
      ),
  },
  {
    path: 'lessons/:id/edit',
    loadComponent: () =>
      import('./features/lessons/pages/lesson-form/lesson-form.component').then(
        (c) => c.LessonFormComponent,
      ),
  },
  {
  path: 'lessons/:id/notes/new',
  loadComponent: () =>
    import('./features/notes/components/note-editor/note-editor.component')
      .then(m => m.NoteEditorComponent)
},
  {
  path: 'lessons/:id',
  loadComponent: () =>
    import('./features/lessons/pages/lesson-details/lesson-details.component')
      .then(m => m.LessonDetailsComponent)
  },
  {
  path: 'lessons/:id/notes/:noteId/edit',
  loadComponent: () =>
    import('./features/notes/components/note-editor/note-editor.component')
      .then(m => m.NoteEditorComponent)
},

  // =======================
  // PRACTICE
  // =======================
  {
    path: 'practice-sets',
    loadComponent: () =>
      import('./features/practice/pages/practice-sets/practice-sets.component').then(
        (component) => component.PracticeSetsComponent,
      ),
  },
  {
    path: 'practice/start',
    loadComponent: () =>
      import('./features/practice/pages/practice-start/practice-start.component').then(
        (component) => component.PracticeStartComponent,
      ),
  },
  {
    path: 'practice/session/:id',
    loadComponent: () =>
      import('./features/practice/pages/practice-session/practice-session.component').then(
        (component) => component.PracticeSessionComponent,
      ),
  },
  {
    path: 'practice/history',
    loadComponent: () =>
      import('./features/practice/pages/practice-history/practice-history.component').then(
        (component) => component.PracticeHistoryComponent,
      ),
  },
  {
    path: 'practice/history/:id',
    loadComponent: () =>
      import('./features/practice/pages/session-detail/session-detail.component').then(
        (component) => component.SessionDetailComponent,
      ),
  },
  {
    path: 'practice/results/:id',
    loadComponent: () =>
      import('./features/practice/pages/session-detail/session-detail.component').then(
        (component) => component.SessionDetailComponent,
      ),
  },

  // =======================
  // AUTH
  // =======================
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/pages/auth-sign-in/auth-sign-in.component').then(
        (component) => component.AuthSignInComponent,
      ),
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./features/auth/pages/auth-sign-up/auth-sign-up.component').then(
        (component) => component.AuthSignUpComponent,
      ),
  },

  { path: '**', redirectTo: 'exercises' },
];