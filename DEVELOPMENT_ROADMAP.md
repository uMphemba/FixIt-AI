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
3. **Guided repair:** Basic checks are presented individually with completion, solved/continue outcomes, and a safety stop. Signed-in users can save progress. Guided checks use the existing AI assessment, which cannot be verified end-to-end until gateway access is enabled. Component-level repair is deliberately excluded.
4. **Accounts and history:** Google OAuth and owner-scoped session history, results, and progress are implemented locally. The EF migration has been applied to Neon. End-to-end sign-in, session recovery, and access isolation still need verification. Existing anonymous sessions remain unowned. Follow-up answers before an assessment are not persisted.
5. **Reliability and refinement:** Improved account/network error feedback, progress-save retry, safety warning placement, and Google callback failure handling. A focused ownership test and anonymous API check pass; account page and mobile navigation were inspected. Live AI assessment, two-user access isolation against PostgreSQL, full session recovery, and broader safety/accessibility review remain outstanding.

## Dependencies and decisions

- Google OAuth client configuration is required; the accounts migration has been applied to Neon. End-to-end sign-in remains unverified.
- AI Gateway requires credits and model access. A successful live model response has not been verified.
- Private history depends on Google OAuth configuration and migration, and requires end-to-end verification.
- Work proceeds one milestone at a time; review the existing implementation and verify each milestone before starting the next.
