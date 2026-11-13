import type { FieldErrors, TroubleRequest } from '../types'
const base = (import.meta.env.VITE_API_BASE_URL || 'http://localhost:5080').replace(/\/$/, '')
export class ApiError extends Error { constructor(message: string, public errors: FieldErrors = {}) { super(message) } }
async function request(path: string, options?: RequestInit) {
  let response: Response
  try { response = await fetch(`${base}${path}`, options) } catch { throw new ApiError('Could not connect to the service. Check your connection and try again.') }
  const body = await response.json().catch(() => ({}))
  if (!response.ok) throw new ApiError(body.message || (response.status === 400 ? 'Please check the highlighted fields.' : 'Something went wrong. Please try again.'), body.errors || {})
  return body
}
export async function getCategories(): Promise<string[]> { return (await request('/api/device-categories')).categories }
export async function submitTrouble(data: TroubleRequest): Promise<{sessionId: string}> { return request('/api/troubleshooting', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(data) }) }
