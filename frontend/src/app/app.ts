import { Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatToolbarModule } from '@angular/material/toolbar';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, MatButtonModule, MatIconModule, MatToolbarModule],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  readonly navLinks = [
  { label: 'Exercises', path: '/exercises', icon: 'quiz' },
  { label: 'Lessons', path: '/lessons', icon: 'menu_book' }, // ✅ ADD THIS
  { label: 'Practice sets', path: '/practice-sets', icon: 'playlist_play' },
  { label: 'Start practice', path: '/practice/start', icon: 'play_arrow' },
  { label: 'History', path: '/practice/history', icon: 'insights' },
];
  readonly loggedIn = !!localStorage.getItem('token');

  logout(): void {
    localStorage.removeItem('token');
    location.reload();
  }
}
