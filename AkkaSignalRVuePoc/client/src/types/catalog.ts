export interface Organisation {
  id: string
  name: string
  createdAt: string
}

export interface Project {
  id: string
  organisationId: string
  name: string
  description: string | null
  createdAt: string
}
