import { createRouter, createWebHistory } from 'vue-router'
import BookHoursView from '../views/BookHoursView.vue'
import ImportView from '../views/ImportView.vue'
import ProjectEditorView from '../views/ProjectEditorView.vue'
import ProjectProgressView from '../views/ProjectProgressView.vue'
import ProjectsListView from '../views/ProjectsListView.vue'

export const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', name: 'import', component: ImportView },
    { path: '/projects', name: 'projects', component: ProjectsListView },
    { path: '/projects/new', name: 'project-new', component: ProjectEditorView },
    { path: '/projects/:id', name: 'project-edit', component: ProjectEditorView },
    { path: '/projects/:id/progress', name: 'project-progress', component: ProjectProgressView },
    { path: '/book-hours', name: 'book-hours', component: BookHoursView },
    { path: '/projects/:id/book-hours', name: 'project-book-hours', component: BookHoursView },
  ],
})
