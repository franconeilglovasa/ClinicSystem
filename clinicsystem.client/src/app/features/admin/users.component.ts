import { Component, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';
import { User } from '../../core/models/models';

@Component({
  selector: 'app-users-admin',
  templateUrl: './users.component.html'
})
export class UsersAdminComponent implements OnInit {
  users: User[] = [];
  showForm = false;
  saving = false;
  error = '';

  userForm = this.fb.group({
    fullName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required],
    role: ['Nurse', Validators.required],
    specialty: [''],
    licenseNumber: ['']
  });

  constructor(private api: ApiService, private fb: FormBuilder) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.api.getUsers().subscribe(u => this.users = u);
  }

  createUser(): void {
    if (this.userForm.invalid) return;
    this.saving = true;
    this.error = '';
    this.api.registerUser(this.userForm.value as any).subscribe({
      next: () => {
        this.saving = false;
        this.showForm = false;
        this.userForm.reset({ role: 'Nurse' });
        this.load();
      },
      error: err => {
        this.error = err.error?.message ?? 'Failed to create user.';
        this.saving = false;
      }
    });
  }

  toggleActive(userId: string): void {
    this.api.toggleUserActive(userId).subscribe(() => this.load());
  }
}
