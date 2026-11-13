import type { DiagnosticResult, FieldErrors, TroubleRequest } from '../types'
const base = (import.meta.env.VITE_API_BASE_URL || (import.meta.env.DEV ? '' : 'http://localhost:5080')).replace(/\/$/, '')
export class ApiError extends Error { constructor(message: string, public errors: FieldErrors = {}) { super(message) } }
async function request(path: string, options?: RequestInit) {
  let response: Response
  try { response = await fetch(`${base}${path}`, { ...options, credentials: 'include' }) } catch { throw new ApiError('Could not connect to the service. Check your connection and try again.') }
  const body = await response.json().catch(() => ({}))
  if (!response.ok) throw new ApiError(body.message || (response.status === 401 ? 'Please sign in to continue.' : response.status === 400 ? 'Please check the highlighted fields.' : 'Something went wrong. Please try again.'), body.errors || {})
  return body
}
export async function getCategories(): Promise<string[]> { return (await request('/api/device-categories')).categories }
export async function submitTrouble(data: TroubleRequest): Promise<{sessionId: string}> { return request('/api/troubleshooting', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(data) }) }
export async function diagnosticQuestions(data: TroubleRequest): Promise<{ questions: string[]; ticket: string }> { return request('/api/diagnostics/questions', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(data) }) }
export async function diagnosticResult(ticket: string, answers: string[]): Promise<DiagnosticResult> { return request('/api/diagnostics/result', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ticket, answers}) }) }
export function googleSignInUrl() { return `${base}/api/auth/google?origin=${encodeURIComponent(window.location.origin)}` }
export async function authProviders(): Promise<{google: boolean}> { return request('/api/auth/providers') }
export async function logout() { return request('/api/auth/logout', {method: 'POST'}) }
export async function currentUser(): Promise<{email: string}> { return request('/api/auth/me') }
export type SavedSession = TroubleRequest & {id: string; createdAt: string; diagnosticResultJson: string | null; progressJson: string | null}
export async function history(): Promise<SavedSession[]> { return request('/api/troubleshooting') }
export async function getSession(id: string): Promise<SavedSession> { return request(`/api/troubleshooting/${encodeURIComponent(id)}`) }
export async function saveState(id: string, result: DiagnosticResult | null, progress: unknown) { return request(`/api/troubleshooting/${encodeURIComponent(id)}/state`, {method: 'PUT', headers: {'Content-Type': 'application/json'}, body: JSON.stringify({ diagnosticResultJson: result ? JSON.stringify(result) : null, progressJson: progress ? JSON.stringify(progress) : null })}) }
