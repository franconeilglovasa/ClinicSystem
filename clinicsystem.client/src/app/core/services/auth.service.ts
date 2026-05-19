import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { LoginRequest, LoginResponse, User } from '../models/models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private currentUserSubject = new BehaviorSubject<LoginResponse | null>(this.loadFromStorage());
  currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient, private router: Router) {}

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>('/api/auth/login', request).pipe(
      tap(res => {
        localStorage.setItem('auth', JSON.stringify(res));
        this.currentUserSubject.next(res);
      })
    );
  }

  logout(): void {
    localStorage.removeItem('auth');
    this.currentUserSubject.next(null);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return this.currentUserSubject.value?.token ?? null;
  }

  getCurrentUser(): LoginResponse | null {
    return this.currentUserSubject.value;
  }

  getRole(): string {
    return this.currentUserSubject.value?.role ?? '';
  }

  hasRole(...roles: string[]): boolean {
    const role = this.getRole();
    return roles.some(r => r.toLowerCase() === role.toLowerCase());
  }

  isLoggedIn(): boolean {
    const user = this.currentUserSubject.value;
    if (!user) return false;
    return new Date(user.expiresAt) > new Date();
  }

  getMe(): Observable<User> {
    return this.http.get<User>('/api/auth/me');
  }

  private loadFromStorage(): LoginResponse | null {
    try {
      const raw = localStorage.getItem('auth');
      if (!raw) return null;
      const parsed: LoginResponse = JSON.parse(raw);
      if (new Date(parsed.expiresAt) <= new Date()) {
        localStorage.removeItem('auth');
        return null;
      }
      return parsed;
    } catch {
      return null;
    }
  }
}
