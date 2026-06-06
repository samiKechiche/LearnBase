import { Component, AfterViewInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import Quill from 'quill';

import { NoteService } from '../../services/note.service';

@Component({
  selector: 'app-note-editor',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    MatCardModule,
    MatButtonModule
  ],
  templateUrl: './note-editor.component.html',
  styles: [`
    /* Ensure Quill editor has proper height and styling */
    ::ng-deep .ql-container {
      min-height: 250px;
    }
    
    ::ng-deep .ql-editor {
      min-height: 250px;
    }
  `]
})
export class NoteEditorComponent implements AfterViewInit, OnDestroy {

  lessonId!: string;
  quill!: Quill;
  private pendingContent: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private noteService: NoteService
  ) {
    this.lessonId = this.route.snapshot.paramMap.get('id')!;
  }
  noteId: string | null = null;

  ngOnInit(): void {
  this.lessonId = this.route.snapshot.paramMap.get('id')!;
  this.noteId = this.route.snapshot.paramMap.get('noteId');

  if (this.noteId) {
    this.loadNote();
  }
}
  ngAfterViewInit(): void {
    this.quill = new Quill('#editor', {
      theme: 'snow',
      modules: {
  toolbar: [
    [{ header: [1, 2, 3, false] }],

    [{ size: ['small', false, 'large', 'huge'] }],

    ['bold', 'italic', 'underline', 'strike'],

    [{ color: [] }, { background: [] }],

    [{ align: [] }],

    [{ list: 'ordered' }, { list: 'bullet' }],

    ['link'],

    ['clean']
  ]
},
      placeholder: 'Write your note here...'
    });

    if (this.pendingContent !== null) {
      this.quill.root.innerHTML = this.pendingContent;
      this.pendingContent = null;
    }
  }

  save(): void {
  const content = this.quill.root.innerHTML;

  if (this.noteId) {
    this.noteService.updateNote(this.noteId, content)
      .subscribe(() => this.router.navigate(['/lessons', this.lessonId]));
  } else {
    this.noteService.createNote(this.lessonId, content)
      .subscribe(() => this.router.navigate(['/lessons', this.lessonId]));
  }
}
  loadNote(): void {
  this.noteService.getNote(this.noteId!).subscribe(res => {
    const content = res.data.content;
    if (this.quill) {
      this.quill.root.innerHTML = content;
      return;
    }

    this.pendingContent = content;
  });
}
  ngOnDestroy(): void {
    // Clean up Quill instance if needed
    if (this.quill) {
      this.quill = null as any;
    }
  }
}
