import { createRouter, createWebHistory } from 'vue-router'
import ImportView from '../views/ImportView.vue'
import ProjectEditorView from '../views/ProjectEditorView.vue'
import ProjectsListView from '../views/ProjectsListView.vue'

export const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', name: 'import', component: ImportView },
    { path: '/projects', name: 'projects', component: ProjectsListView },
    { path: '/projects/new', name: 'project-new', component: ProjectEditorView },
    { path: '/projects/:id', name: 'project-edit', component: ProjectEditorView },
  ],
})
