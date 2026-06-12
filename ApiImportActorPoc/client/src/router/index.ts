import { createRouter, createWebHistory } from 'vue-router'
import ImportView from '../views/ImportView.vue'

export const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', name: 'import', component: ImportView },
  ],
})
