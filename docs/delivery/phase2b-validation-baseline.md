# Phase 2B — Stabilisation tests/build et baseline de validation

> Date : 2026-04-28  
> Statut : baseline clarifiée — Phase 2 validable en Release

## Diagnostic Debug

Après nettoyage complet (`dotnet clean AegisLoop.sln` puis suppression des dossiers `bin/` et `obj/`), restauration et build Debug réussissent, mais la commande Debug implicite échoue :

```bash
dotnet test AegisLoop.sln
```

Assembly identifié :

```text
tests/AegisLoop.Domain.Tests/bin/Debug/net10.0/AegisLoop.Domain.Tests.dll
```

Commande minimale reproductible :

```bash
dotnet test tests/AegisLoop.Domain.Tests/AegisLoop.Domain.Tests.csproj --no-restore
```

Message exact observé :

```text
System.IO.FileLoadException: Could not load file or assembly 'c:\All My Files\WorkingCode\Git\MyCode\Codex\AegisLoop\tests\AegisLoop.Domain.Tests\bin\Debug\net10.0\AegisLoop.Domain.Tests.dll'. Une stratégie de contrôle d’application a bloqué ce fichier. (0x800711C7)
```

Les journaux Windows Code Integrity confirment un blocage externe :

```text
Code Integrity determined that a process (...\tests\AegisLoop.Domain.Tests\bin\Debug\net10.0\testhost.exe) attempted to load ...\AegisLoop.Domain.Tests.dll that did not meet the Enterprise signing level requirements or violated code integrity policy (Policy ID:{0283ac0f-fff1-49ae-ada1-8a933130cad6}).
```

## Tentatives raisonnables réalisées

- Clean solution + suppression des sorties `bin/` / `obj/`.
- Restore complet.
- Build Debug solution : OK.
- Tests solution Debug : échec isolé sur `AegisLoop.Domain.Tests.dll`.
- Tests projet par projet en Debug : seul `AegisLoop.Domain.Tests` échoue ; les autres projets passent.
- Vérification des chemins de sortie : chemin standard `bin/Debug/net10.0`.
- Vérification des dépendances de test : packages xUnit / runner / Microsoft.NET.Test.Sdk alignés avec les autres projets.
- Vérification ADS/Mark-of-the-Web via `dir /r` : aucun flux alternatif listé.
- Comparaison comportementale Debug/Release : `AegisLoop.Domain.Tests` passe en Release.
- Essais Debug sans PDB (`/p:DebugType=none`) et avec optimisation (`/p:Optimize=true`) : échec inchangé `0x800711C7`.

Aucun test n'a été supprimé ou désactivé.

## Décision

La configuration Debug implicite est déclarée non supportée comme baseline de validation dans cet environnement Windows local lorsque Windows Code Integrity / Smart App Control applique cette policy. La baseline officielle locale/CI devient :

```bash
dotnet test AegisLoop.sln --configuration Release --settings .runsettings
```

La configuration Release reste exigée pour valider la Phase 2 et la CI. Debug peut rester utilisable pour du développement interactif sur un environnement où la politique Windows ne bloque pas les assemblies générées localement, mais il ne doit pas être présenté comme vert ici.

## Fichiers alignés

- `scripts/test.bat`
- `.github/workflows/ci.yml`
- `README.md`
- `docs/adr/0006-strategie-tests.md`
- `docs/specs/02-architecture-technique.md`
- `docs/delivery/phase2b-validation-baseline.md`