import { Component } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html'
})
export class LoginComponent {
  readonly seededAdmin = {
    username: 'admin@clinicsystem.com',
    password: 'Admin@123'
  };

  loginForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required]
  });
  loading = false;
  error = '';

  constructor(private fb: FormBuilder, private auth: AuthService, private router: Router) {
    if (this.auth.isLoggedIn()) this.router.navigate(['/app/dashboard']);
  }

  useDefaultAdmin(): void {
    this.loginForm.patchValue({
      email: this.seededAdmin.username,
      password: this.seededAdmin.password
    });
  }

  onSubmit(): void {
    if (this.loginForm.invalid) return;
    this.loading = true;
    this.error = '';

    this.auth.login(this.loginForm.value as any).subscribe({
      next: () => this.router.navigate(['/app/dashboard']),
      error: err => {
        this.error = err.error?.message ?? 'Login failed. Please check your credentials.';
        this.loading = false;
      }
    });
  }
}
