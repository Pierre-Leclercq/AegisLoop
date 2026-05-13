# ADR 0004 — Stratégie des connecteurs OSINT

> **Statut :** Accepté  
> **Date :** 2026-04-23  
> **Décideur :** Architecte

---

## Contexte

Aegis Loop doit collecter des données depuis plusieurs sources OSINT publiques. Les sources ont des formats, des protocoles et des limites très différents. Il faut une stratégie d'extensibilité qui permette d'ajouter des sources sans modifier le cœur du système.

## Décision

Architecture **plugin par interface** avec `ISourceConnector` comme contrat commun :

1. Chaque connecteur implémente l'interface `ISourceConnector`
2. Chaque connecteur est un projet C# séparé référençant `AegisLoop.Connectors.Abstractions`
3. Enregistrement par convention au démarrage (scan d'assemblies)
4. Chaque connecteur gère son propre rate limiting, retry et format
5. Le pipeline d'ingestion est indépendant des connecteurs

**Sources MVP (Must) :** RSS/Atom, GDELT  
**Sources V1 (Should) :** YouTube Data API, STAC/Copernicus, Import manuel  
**Sources futures (Could) :** AIS maritime, météo, Twitter/X (si API publique), temps réel

## Alternatives considérées

| Alternative | Avantages | Inconvénients |
|---|---|---|
| **Connecteurs hardcoded** | Simple, rapide | Pas d'extensibilité, modification du cœur |
| **Plugins DLL externes** | Extensible à l'exécution | Complexe, sécurité, versionning |
| **Scripts Python** | Flexible, rapide | Pas de typage, pas d'intégration native |
| **Configuration YAML seule** | Pas de code | Pas assez expressif pour les transformations |

## Justification

1. **Extensibilité** — Ajouter un connecteur = nouveau projet C#, pas de modification du cœur
2. **Testabilité** — Chaque connecteur est testable indépendamment par contrat
3. **Isolation** — Un connecteur en erreur ne bloque pas les autres
4. **Rate limiting** — Chaque connecteur gère ses propres limites API
5. **Futur** — Architecture prête pour des plugins DLL externes en V3

## Conséquences

- Un projet C# par connecteur (12 projets au total en V1)
- Le pipeline d'ingestion doit être robuste face aux erreurs de connecteur
- Les formats sources variables nécessitent un mapping par connecteur

## Risques juridiques par connecteur

| Connecteur | Risque | Mitigation |
|---|---|---|
| RSS/Atom | Faible — flux publics | Respect des CGU, rate limiting raisonnable |
| GDELT | Faible — API publique | Respect des limites de requêtes |
| YouTube Data API | Moyen — quotas, CGU strictes | Clé API obligatoire, respect des quotas, métadonnées uniquement |
| STAC/Copernicus | Faible — données ouvertes | Vérification des licences par catalogue |
| Import manuel | Aucun — données locales | Responsabilité de l'utilisateur |

## Références

- [01-specs-fonctionnelles.md](../specs/01-specs-fonctionnelles.md) section 3.1
- [02-architecture-technique.md](../specs/02-architecture-technique.md) section 12