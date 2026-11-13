import { createContext, useContext, useEffect, useState } from 'react'
import { Link, Navigate, useParams } from 'react-router-dom'
import { authProviders, currentUser, getSession, googleSignInUrl, history, logout, type SavedSession } from './services/api'
import type { DiagnosticResult } from './types'
import DiagnosticFlow from './DiagnosticFlow'

type Auth = { email: string | null; loading: boolean; refresh: () => Promise<void>; signOut: () => Promise<void> }
const Context = createContext<Auth | null>(null)
export function useAuth() { const value = useContext(Context); if (!value) throw new Error('Auth provider missing'); return value }
export function AuthProvider({children}: {children: React.ReactNode}) {
  const [email, setEmail] = useState<string | null>(null), [loading, setLoading] = useState(true)
  async function refresh() { try { setEmail((await currentUser()).email) } catch { setEmail(null) } finally { setLoading(false) } }
  async function signOut() { await logout(); setEmail(null) }
  useEffect(() => { void refresh() }, [])
  return <Context.Provider value={{email, loading, refresh, signOut}}>{children}</Context.Provider>
}
export function RequireAccount({children}: {children: React.ReactNode}) { const {email, loading} = useAuth(); if (loading) return <main className="container not-found"><p>Checking your account…</p></main>; return email ? children : <Navigate to="/account" replace/> }
export function Account() {
  const auth = useAuth()
  const [available, setAvailable] = useState<boolean | null>(null), [error, setError] = useState('')
  useEffect(() => { authProviders().then(x => setAvailable(x.google)).catch(() => setError('Cannot reach the sign-in service. Check that the API is running.')) }, [])
  if (auth.loading) return <main className="container not-found"><p>Checking your account…</p></main>
  if (auth.email) return <Navigate to="/history" replace/>
  return <main className="form-page"><div className="account-wrap form-card"><span className="kicker">YOUR FIXIT AI ACCOUNT</span><h1>Welcome to FixIt AI</h1><p>Sign in with Google to save and revisit your troubleshooting sessions. FixIt AI does not collect a password.</p>{available ? <a className="button primary submit" href={googleSignInUrl()}>Continue with Google</a> : <p className="form-error" role="status">{error || (available === null ? 'Checking sign-in availability…' : 'Google sign-in is not configured yet.')}</p>}</div></main>
}
export function History() {
  const [sessions, setSessions] = useState<SavedSession[] | null>(null), [error, setError] = useState('')
  useEffect(() => { history().then(setSessions).catch(e => setError(e instanceof Error ? e.message : 'Unable to load history.')) }, [])
  return <main className="form-page"><div className="container history"><span className="kicker">YOUR SESSIONS</span><h1>Troubleshooting history</h1><p>Pick up where you left off.</p>{error && <p className="form-error" role="alert">{error}</p>}{sessions === null && !error ? <p>Loading sessions…</p> : sessions?.length ? <div className="history-list">{sessions.map(s => <Link key={s.id} to={`/history/${s.id}`}><strong>{s.deviceName}</strong><span>{s.deviceCategory} · {new Date(s.createdAt).toLocaleDateString()}</span><p>{s.problemDescription}</p></Link>)}</div> : !error && <p>No sessions yet. <Link to="/troubleshoot">Start troubleshooting</Link>.</p>}<Link className="button primary" to="/troubleshoot">Start a new session</Link></div></main>
}
export function HistoryDetail() {
  const {id} = useParams(), [session, setSession] = useState<SavedSession | null>(null), [error, setError] = useState('')
  useEffect(() => { if (id) getSession(id).then(setSession).catch(e => setError(e instanceof Error ? e.message : 'Unable to load session.')) }, [id])
  let savedResult: DiagnosticResult | null = null, savedProgress: unknown = null
  try { savedResult = session?.diagnosticResultJson ? JSON.parse(session.diagnosticResultJson) : null; savedProgress = session?.progressJson ? JSON.parse(session.progressJson) : null } catch { return <main className="container not-found"><p>Saved session data could not be read.</p><Link to="/history">Back to history</Link></main> }
  return <main className="form-page"><div className="container form-layout"><div className="form-intro"><Link className="back" to="/history">← Back to history</Link><h1>{session?.deviceName || 'Session'}</h1><p>{session?.problemDescription}</p></div><div className="form-card">{error ? <p role="alert">{error}</p> : session ? <DiagnosticFlow device={{deviceCategory: session.deviceCategory, deviceName: session.deviceName, brand: session.brand || '', model: session.model || '', problemDescription: session.problemDescription}} session={session.id} initialResult={savedResult} initialProgress={savedProgress} onReset={() => window.location.assign('/troubleshoot')}/> : <p>Loading session…</p>}</div></div></main>
}
