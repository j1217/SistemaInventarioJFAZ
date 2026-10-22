import { computed, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { LoginResponse } from './models';

const TOKEN_KEY = 'inventory.session';
const API_URL = 'http://localhost:5080/api';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly session = signal<LoginResponse | null>(this.readSession());
  readonly isAuthenticated = computed(() => !!this.session()?.accessToken);
  readonly currentUser = computed(() => this.session());

  constructor(private readonly http: HttpClient) {}

  login(email: string, password: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${API_URL}/auth/login`, { email, password }).pipe(
      tap((session) => {
        // La sesión desaparece al cerrar el navegador o vencer el JWT.
        sessionStorage.setItem(TOKEN_KEY, JSON.stringify(session));
        this.session.set(session);
      }),
    );
  }

  token(): string | null { return this.session()?.accessToken ?? null; }

  logout(): void {
    sessionStorage.removeItem(TOKEN_KEY);
    this.session.set(null);
  }

  private readSession(): LoginResponse | null {
    try {
      const saved = sessionStorage.getItem(TOKEN_KEY);
      if (!saved) return null;
      const session = JSON.parse(saved) as LoginResponse;
      if (new Date(session.expiresAt).getTime() <= Date.now()) {
        sessionStorage.removeItem(TOKEN_KEY);
        return null;
      }
      return session;
    } catch { return null; }
  }
}
