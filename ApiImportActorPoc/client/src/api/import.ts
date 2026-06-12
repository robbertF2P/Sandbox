const apiBase = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5001'

export async function startImport(payload: unknown): Promise<Response> {
  return fetch(`${apiBase}/api/import`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
}

export async function getImportModel(sessionId: string): Promise<Response> {
  return fetch(`${apiBase}/api/import/${sessionId}/model`)
}

export async function persistImport(sessionId: string): Promise<Response> {
  return fetch(`${apiBase}/api/import/${sessionId}/persist`, { method: 'POST' })
}
