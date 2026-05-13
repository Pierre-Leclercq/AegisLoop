# ADR 0006 — Stratégie de tests

> **Statut :** Accepté  
> **Date :** 2026-04-23  
> **Décideur :** Architecte

---

## Contexte

Aegis Loop doit être testable, fiable et démontrable. La stratégie de tests doit couvrir le domaine pur, les intégrations, les API, les connecteurs et les scénarios E2E, tout en restant réalisable par un porteur solo.

## Décision

Pyramide de tests avec répartition **60% unitaires / 20% intégration / 20% E2E** :

- **Tests unitaires** — xUnit + FluentAssertions, ciblant Domain, Application, Fusion, Scoring
- **Tests d'intégration** — xUnit + WebApplicationFactory, ciblant API, Infrastructure, Pipeline
- **Tests de contrat** — xUnit, vérifiant que chaque connecteur respecte `ISourceConnector`
- **Tests E2E** — Playwright pour les scénarios GUI complets
- **Tests de performance** — Benchmarks pour les seuils (dashboard < 2s, recherche < 500ms)
- **Tests de sécurité** — API localhost uniquement, validation, XSS, audit

Conventions de nommage : `{ClassTestée}Tests.{Méthode}_{Scénario}_{RésultatAttendu}`

Données de test :
- Factories pour les tests unitaires
- SQLite en mémoire pour les tests d'intégration
- Datasets seed pour les tests E2E
- Mock HTTP (WireMock) pour les connecteurs

CI : GitHub Actions avec matrice Windows/Linux/macOS.

## Alternatives considérées

| Alternative | Avantages | Inconvénients |
|---|---|---|
| **NUnit** | Attributs classiques | Moins fluide que xUnit pour les tests paramétrés |
| **MSTest** | Natif Visual Studio | Moins flexible, pas de parallélisme natif |
| **Cypress** | E2E mature | Pas de support Electron natif |
| **Selenium** | E2E classique | Lourd, pas de support Electron |
| **Pas de tests E2E** | Plus rapide | Risque de régressions GUI |

## Justification

1. **xUnit** — Standard .NET, parallélisme natif, fixtures
2. **FluentAssertions** — Lisibilité des assertions, maintenance facilitée
3. **Playwright** — Support Electron, multi-navigateur, moderne
4. **Pyramide** — Les tests unitaires sont rapides et ciblent le domaine pur
5. **Porteur solo** — La stratégie est réaliste : automatisable, maintenable, CI-friendly

## Baseline de validation locale/CI

La baseline officielle de tests backend pour AegisLoop V1 est :

```bash
dotnet test AegisLoop.sln --configuration Release --settings .runsettings
```

Raison : sur l'environnement Windows local de référence, la commande Debug implicite `dotnet test AegisLoop.sln` est reproductiblement bloquée par Windows Code Integrity / Smart App Control lors du chargement d'assemblies de tests générées localement, avec `System.IO.FileLoadException` et le code `0x800711C7` (`Une stratégie de contrôle d’application a bloqué ce fichier`). Les journaux Windows Code Integrity indiquent que `testhost.exe` tente de charger l'assembly Debug concernée et que celle-ci ne respecte pas les exigences de signature Enterprise ou viole la policy `{0283ac0f-fff1-49ae-ada1-8a933130cad6}`.

Cette décision ne désactive aucun test et ne change pas le périmètre fonctionnel testé : les tests doivent passer en Release. Debug peut être utilisé pour du développement interactif si la politique locale l'autorise, mais il n'est pas une baseline de validation dans cet environnement.

## Conséquences

- Effort de test significatif (mais réaliste pour porteur solo)
- CI GitHub Actions nécessaire pour la confiance
- Les tests E2E sont les plus lents mais les plus précieux pour la démo
- Les scripts et la CI doivent appeler explicitement la configuration `Release` pour éviter toute ambiguïté avec la configuration Debug locale.

## Références

- [05-plan-de-tests.md](../specs/05-plan-de-tests.md)
- [02-architecture-technique.md](../specs/02-architecture-technique.md) section 16.4