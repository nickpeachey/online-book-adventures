# Online Book Adventures — Monorepo

A production-ready Choose Your Own Adventure platform managed as a single repository.

## Structure
- `backend/` — .NET 10 ASP.NET Core API and tests
- `frontend/` — Next.js 16 + React 19 frontend
- `infra/` — Helm charts and Kubernetes manifests
- `docs/` — Project documentation
- `package.json` — Root monorepo scripts for both application stacks

## Prerequisites
- Node.js 22+
- npm 10+
- .NET SDK 10.0.100+
- Docker Desktop (optional, for local infrastructure or full stack containers)

## Monorepo Commands
Run these from the repository root.

```bash
npm run bootstrap
npm run restore
npm run dev:backend
npm run dev:frontend
npm run dev:services
npm run dev:stack
npm run build
npm run test
npm run lint
npm run type-check
```

## Command Reference
- `npm run bootstrap` installs frontend dependencies.
- `npm run restore` restores .NET packages and frontend dependencies.
- `npm run dev:backend` starts the ASP.NET Core API.
- `npm run dev:frontend` starts the Next.js app.
- `npm run dev:services` starts supporting local services with Docker Compose.
- `npm run dev:stack` builds and starts the full containerized stack.
- `npm run build` builds backend and frontend from the root.
- `npm run test` runs backend and frontend tests from the root.
- `npm run lint` runs frontend linting.
- `npm run type-check` runs the frontend TypeScript type check.

## Local Development
For local development without fully containerizing the app:

```bash
npm run dev:services
npm run dev:backend
npm run dev:frontend
```

The frontend is available at `http://localhost:3000` and the backend at `http://localhost:8080`.

## Full Stack With Docker

```bash
npm run dev:stack
```

This uses the root `docker-compose.yml` to start the API, frontend, and supporting infrastructure together.
