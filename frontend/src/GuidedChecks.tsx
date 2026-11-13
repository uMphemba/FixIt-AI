import { useState } from 'react'
import { Check, ShieldAlert } from 'lucide-react'

type Outcome = 'solved' | 'stopped' | 'finished' | null

export default function GuidedChecks({ steps, difficulty, professionalAdvice }: {steps: string[]; difficulty: string; professionalAdvice: string}) {
  const [index, setIndex] = useState(0)
  const [completed, setCompleted] = useState<number[]>([])
  const [awaitingOutcome, setAwaitingOutcome] = useState(false)
  const [outcome, setOutcome] = useState<Outcome>(null)
  if (difficulty === 'professional') return <div className="guided-card"><h3>Professional help recommended</h3><p>Do not continue with checks if the situation may be dangerous. {professionalAdvice}</p></div>
  if (!steps.length) return <div className="guided-card"><h3>No checks available</h3><p>No safe basic checks were suggested. {professionalAdvice}</p></div>
  function next() { setAwaitingOutcome(false); if (index + 1 === steps.length) setOutcome('finished'); else setIndex(index + 1) }
  return <section className="guided-card" aria-label="Guided troubleshooting" aria-live="polite"><span className="kicker">GUIDED CHECKS</span>{outcome ? <><h3>{outcome === 'solved' ? 'The problem appears resolved' : outcome === 'stopped' ? 'Stop and seek help' : 'You’ve reached the end'}</h3><p>{outcome === 'solved' ? 'You reported that the problem is solved. If it returns or you notice anything unsafe, stop using the device and contact a qualified professional.' : outcome === 'stopped' ? `Do not continue if you are unsure or notice a hazard. ${professionalAdvice}` : `The checks did not resolve the problem. ${professionalAdvice}`}</p><p>{completed.length} of {steps.length} checks marked complete.</p>{outcome !== 'stopped' && <button type="button" className="text-button secondary-action" onClick={() => { setIndex(0); setCompleted([]); setOutcome(null); setAwaitingOutcome(false) }}>Restart these checks</button>}</> : <><h3>Check {index + 1} of {steps.length}</h3><p className="guided-instruction">{steps[index]}</p><p className="guided-warning"><ShieldAlert size={18}/> Stop if you see smoke, smell gas, see damaged wiring or batteries, feel heat, or are unsure. Do not open equipment or touch internal parts.</p>{awaitingOutcome ? <><p><strong>Did this check solve the problem?</strong></p><div className="guided-actions"><button className="button primary" type="button" onClick={() => setOutcome('solved')}>Yes, it’s solved</button><button className="button outline" type="button" onClick={next}>No, continue</button></div></> : <button className="button primary" type="button" onClick={() => {setCompleted(previous => [...previous, index]); setAwaitingOutcome(true)}}><Check size={17}/> Mark check complete</button>}<button type="button" className="text-button stop-action" onClick={() => setOutcome('stopped')}>Stop — this seems unsafe or I’m unsure</button></>}</section>
}
