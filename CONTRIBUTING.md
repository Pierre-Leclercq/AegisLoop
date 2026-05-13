# Contributing

AegisLoop is currently a solo MVP V1 project. Issues, suggestions and focused pull requests are welcome.

## Local validation

```powershell
.\scripts\test.bat
```

For frontend-only changes:

```powershell
cd src\desktop-electron
npm install
npm run lint
npm run build
npm test
```

## Data policy

Do not submit real sensitive OSINT data, credentials, personal data or operational material in issues, screenshots or pull requests.

## Before opening a pull request

Please include:

- scope of the change;
- affected backend/frontend area;
- validation commands executed;
- screenshots for UI changes.