import {
  AfterViewInit,
  Component
} from '@angular/core';

import Quill from 'quill';

@Component({
  selector: 'app-note-editor',
  standalone: true,
  templateUrl: './note-editor.component.html',
  styleUrl: './note-editor.component.css',
})
export class NoteEditorComponent implements AfterViewInit {

  private quill!: Quill;

  ngAfterViewInit(): void {
    this.quill = new Quill('#editor', {
      theme: 'snow',
      placeholder: 'Write your note...',
      modules: {
        toolbar: [
          ['bold', 'italic', 'underline'],
          [{ header: [1, 2, 3, false] }],
          [{ list: 'ordered' }, { list: 'bullet' }],
          ['link', 'code-block']
        ]
      }
    });
  }
}