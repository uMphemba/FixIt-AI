# FixIt AI development roadmap

## Current baseline

The workspace began empty and is not a Git repository. Milestone 1 adds a React/TypeScript/Vite frontend and an ASP.NET Core 10 API using EF Core and PostgreSQL. The initial migration is present; a configured PostgreSQL instance is still required to verify persistence.

## Proposed architecture

- A responsive web interface collects device details and symptoms and presents guided troubleshooting.
- A backend validates requests, enforces safety rules, and communicates with an AI provider. Provider credentials and internal prompts stay on the server.
- Troubleshooting sessions use an explicit data model for device details, answers, diagnostic results, steps, and progress. Choose storage appropriate to the selected stack and the eventual privacy requirements.
- Treat AI output as uncertain, validate its structure, and apply safety checks independently of provider responses.

## Milestones

0. **Inspection and planning:** Record the empty baseline and this roadmap. Complete before implementation.
1. **Core foundation:** Implemented layout, device and symptom form, validation, API endpoints, PostgreSQL session model and migration. Builds, health, categories, validation, and database-unavailable handling verified. Database persistence and successful end-to-end submission remain to be verified against a configured PostgreSQL instance.
2. **AI troubleshooting:** Select and integrate a real AI provider through the backend using environment-based credentials. Collect relevant follow-up answers and validate structured results, safety guidance, and provider errors. Verify with configured provider credentials; do not represent an unverified integration as working.
3. **Guided repair:** Present steps individually, record completion and outcomes, adapt the flow to answers, and enforce clear safety stop conditions. Verify the main paths and hazardous scenarios.
4. **Accounts and history:** Choose a justified authentication and persistence approach, save session progress and results, and enforce ownership of private records. Verify access isolation and session recovery.
5. **Reliability and refinement:** Verify key journeys, accessibility, mobile layouts, validation, privacy, and safety. Remove unfinished UI and document the actual startup and configuration steps.

## Dependencies and decisions

- Authentication method and AI provider have not yet been selected. PostgreSQL must be configured locally before requests can be saved.
- A real AI integration requires provider credentials and network access; neither is assumed to exist.
- Persistent private history depends on storage and an appropriate identity mechanism.
- Work proceeds one milestone at a time; review the existing implementation and verify each milestone before starting the next.
