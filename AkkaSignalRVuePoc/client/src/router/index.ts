import { createRouter, createWebHistory } from 'vue-router'
import LiveMessagesView from '../views/LiveMessagesView.vue'
import OrganisationsListView from '../views/OrganisationsListView.vue'
import ProjectsListView from '../views/ProjectsListView.vue'

export const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', name: 'live-messages', component: LiveMessagesView },
    { path: '/organisations', name: 'organisations', component: OrganisationsListView },
    { path: '/projects', name: 'projects', component: ProjectsListView },
  ],
})
