import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
})
export class LoginComponent {
  readonly loading = signal(false);
  readonly showPassword = signal(false);
  readonly errorMessage = signal('');
  readonly form;

  constructor(formBuilder: FormBuilder, private readonly auth: AuthService, private readonly router: Router) {
    this.form = formBuilder.nonNullable.group({
      email: ['admin@inventario.local', [Validators.required, Validators.email]],
      password: ['InventoryDemo2026!', [Validators.required, Validators.minLength(8)]],
    });
  }

  submit(): void {
    this.errorMessage.set('');
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.auth.login(this.form.controls.email.value, this.form.controls.password.value).subscribe({
      next: () => this.router.navigate(['/dashboard']),
      error: (error) => {
        this.errorMessage.set(error?.error?.detail ?? 'No pudimos conectar con el servidor. Inténtalo de nuevo.');
        this.loading.set(false);
      },
      complete: () => this.loading.set(false),
    });
  }

  togglePassword(): void {
    this.showPassword.update((visible) => !visible);
  }
}
