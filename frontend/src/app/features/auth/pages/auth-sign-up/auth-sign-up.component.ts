import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { RouterLink, RouterLinkActive } from '@angular/router';

import { AuthService } from '../../services/auth.service';

import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import {merge} from 'rxjs';
import {takeUntilDestroyed} from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-auth-sign-up',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    RouterLinkActive,

    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatDividerModule
  ],
  templateUrl: './auth-sign-up.component.html',
  styleUrl: './auth-sign-up.component.css',
})
export class AuthSignUpComponent
{
  loading = false;
  emailErrorMessage = signal('');


  form = new FormGroup({
    username: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    email: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.email] }),
    password: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
  });
  
  hide = signal(true);
  clickEvent(event: MouseEvent) {
    this.hide.set(!this.hide());
    event.stopPropagation();
  }
  
  updateErrorMessage() {
    if (this.form.get('email')?.hasError('required')) {
      this.emailErrorMessage.set('You must enter a value');
    } else if (this.form.get('email')?.hasError('email')) {
      this.emailErrorMessage.set('Not a valid email');
    } else {
      this.emailErrorMessage.set('');
    }
  }

  
  constructor(
    private readonly authService: AuthService,
    private readonly router: Router,
    private readonly route: ActivatedRoute,
  )
  {
    merge(this.form.get('email')!.statusChanges, this.form.get('email')!.valueChanges)
      .pipe(takeUntilDestroyed())
      .subscribe(() => this.updateErrorMessage());
  }

  ngOnInit(): void {
    if (localStorage.getItem('token')) {
      this.router.navigate(['/..'], { relativeTo: this.route });
    }
  }

  submit(): void {
    if (this.form.invalid) return;

    this.loading = true;

    const raw = this.form.getRawValue();

    const payload = {
      email: raw.email,
      password: raw.password,
      username: raw.username,
    };

    this.authService.signUp(payload).subscribe({
      next: (res: any) => {
        this.loading = false;
        localStorage.setItem('token', res.token);
        this.router.navigate(['/..'], { relativeTo: this.route });
        location.reload();
      },
      error: (err) => {
        console.error(err);
        this.loading = false;
      },
    });
  }

}
