# FixIt AI development roadmap

## Current baseline

The workspace began empty. Milestone 1 adds a React/TypeScript/Vite frontend and an ASP.NET Core 10 API using EF Core and PostgreSQL. The initial migration has been applied to the configured Neon branch, and a test submission has been saved through the application.

## Proposed architecture

- A responsive web interface collects device details and symptoms and presents guided troubleshooting.
- A backend validates requests, enforces safety rules, and communicates with an AI provider. Provider credentials and internal prompts stay on the server.
- Troubleshooting sessions use an explicit data model for device details, answers, diagnostic results, steps, and progress. Choose storage appropriate to the selected stack and the eventual privacy requirements.
- Treat AI output as uncertain, validate its structure, and apply safety checks independently of provider responses.

## Milestones

0. **Inspection and planning:** Record the empty baseline and this roadmap. Complete before implementation.
1. **Core foundation:** Implemented layout, device and symptom form, validation, API endpoints, PostgreSQL session model and migration. Builds, health, categories, validation, database-unavailable handling, and a Neon-backed submission were verified.
2. **AI troubleshooting:** OpenAI Responses API integration, Neon AI Gateway support, protected short-lived question tickets, follow-up forms, structured results, basic safety stops, and provider error handling are implemented. The gateway returns 403 because the Neon account has no AI Gateway credits; a successful model response and end-to-end assessment remain unverified. Safety screening needs evaluation against a broad set of real-world hazards before deployment.
3. **Guided repair:** Present steps individually, record completion and outcomes, adapt the flow to answers, and enforce clear safety stop conditions. Verify the main paths and hazardous scenarios.
4. **Accounts and history:** Choose a justified authentication and persistence approach, save session progress and results, and enforce ownership of private records. Verify access isolation and session recovery.
5. **Reliability and refinement:** Verify key journeys, accessibility, mobile layouts, validation, privacy, and safety. Remove unfinished UI and document the actual startup and configuration steps.

## Dependencies and decisions

- Authentication has not yet been selected. Neon PostgreSQL is configured for local development.
- AI Gateway requires credits and model access. A successful live model response has not been verified.
- Persistent private history depends on storage and an appropriate identity mechanism.
- Work proceeds one milestone at a time; review the existing implementation and verify each milestone before starting the next.
