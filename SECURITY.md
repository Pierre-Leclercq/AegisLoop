# Security Policy

## Supported Versions

Only the latest `main` branch commit of this prototype is provided. No formal release or LTS is offered.

## Reporting a Vulnerability

Please do not disclose security issues publicly.

Use GitHub Private Vulnerability Reporting if enabled, or contact the maintainer through the GitHub profile.

Do not include real OSINT data, credentials, personal data or sensitive operational material in any report.

## Scope

This prototype is local-only and not intended for production deployment. Security considerations include:

- No authentication or authorization is implemented (local workbench).
- The API binds to `localhost` only (default port 5100).
- CORS is restricted to Vite dev server origins.
- SQLite database contains no real PII or sensitive operational data.
- No secrets, tokens, or cloud credentials are stored in the repository.

**This is not a hardened production system.**