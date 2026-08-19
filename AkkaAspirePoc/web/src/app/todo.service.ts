import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface TodoItem {
  id: string;
  title: string;
  isCompleted: boolean;
  createdAtUtc: string;
  completedAtUtc: string | null;
}

@Injectable({ providedIn: 'root' })
export class TodoService {
  private readonly apiBase = (window as unknown as { __API_URL__?: string }).__API_URL__
    ?? 'http://localhost:5080';

  constructor(private readonly http: HttpClient) {}

  getTodos(): Observable<TodoItem[]> {
    return this.http.get<TodoItem[]>(`${this.apiBase}/api/todos`);
  }

  createTodo(title: string): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(`${this.apiBase}/api/todos`, { title });
  }

  completeTodo(id: string): Observable<void> {
    return this.http.post<void>(`${this.apiBase}/api/todos/${id}/complete`, {});
  }
}
