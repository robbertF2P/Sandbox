import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { TodoItem, TodoService } from '../todo.service';

@Component({
  selector: 'app-todos',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './todos.component.html',
  styleUrl: './todos.component.css'
})
export class TodosComponent implements OnInit {
  readonly todos = signal<TodoItem[]>([]);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  newTitle = '';

  constructor(private readonly todoService: TodoService) {}

  ngOnInit(): void {
    this.refresh();
  }

  refresh(): void {
    this.loading.set(true);
    this.error.set(null);

    this.todoService.getTodos().subscribe({
      next: items => {
        this.todos.set(items);
        this.loading.set(false);
      },
      error: err => {
        this.error.set(err.message ?? 'Failed to load todos');
        this.loading.set(false);
      }
    });
  }

  addTodo(): void {
    const title = this.newTitle.trim();
    if (!title) {
      return;
    }

    this.todoService.createTodo(title).subscribe({
      next: () => {
        this.newTitle = '';
        this.refresh();
      },
      error: err => this.error.set(err.message ?? 'Failed to create todo')
    });
  }

  completeTodo(id: string): void {
    this.todoService.completeTodo(id).subscribe({
      next: () => this.refresh(),
      error: err => this.error.set(err.message ?? 'Failed to complete todo')
    });
  }
}
