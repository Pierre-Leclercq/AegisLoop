# ADR 0002 — Architecture modulaire du backend en C# / .NET (Recentrée V1)

> **Statut :** Accepté — Modifié après audit (14 → 6 projets)  
> **Date :** 2026-04-23  
> **Décideur :** Architecte

---

## Contexte

Le backend d'Aegis Loop doit gérer l'ingestion, la normalisation, la fusion, le scoring, le case management et l'API. L'architecture initiale prévoyait 14 projets C# pour respecter les principes SOLID. L'audit critique a identifié ce sur-découpage comme le principal signal de sur-ingénierie pour un porteur solo.

## Décision initiale (V1.0)

Architecture modulaire en couches avec 14 projets C# : Domain, Application, Contracts, Infrastructure, Connectors.Abstractions, Connectors.Rss, Connectors.Gdelt, Connectors.YouTube, Connectors.Stac, Fusion, Scoring, CaseManagement, Api, Worker.

## Décision recentrée (V2.0 — Post-audit)

Architecture **modulaire compacte** en **6 projets C#** pour le MVP solo :

| Projet | Responsabilité | Justification V1 |
|---|---|---|
| **AegisLoop.Domain** | Modèle de domaine pur (11 types, invariants, interfaces ISourceConnector, IScoringService, etc.) | Séparation indispensable |
| **AegisLoop.Application** | Use cases, pipeline ingestion, fusion, scoring, feedback (fusionne Application + Fusion + Scoring + CaseManagement) | Fonctionnalités métier rassemblées |
| **AegisLoop.Infrastructure** | Persistance EF Core + SQLite, configuration, logging, géocodage | Séparation indispensable |
| **AegisLoop.Connectors** | RSS + GDELT (un seul projet pour 2 connecteurs V1) | 2 connecteurs = 1 projet suffit |
| **AegisLoop.Api** | REST Minimal APIs localhost (~20 endpoints) | Couche API |
| **AegisLoop.Worker** | Service d'ingestion, host, planification | Processus hôte |
| **desktop-electron** | Shell Electron + React + TypeScript | Frontend (pas un projet C#) |

**Ce qui a été fusionné/supprimé :**
- Connectors.Abstractions → interface ISourceConnector dans Domain
- Contracts → DTOs dans Application
- Fusion, Scoring, CaseManagement → namespaces dans Application
- Connectors.Rss, Connectors.Gdelt → un seul projet Connectors
- Connectors.YouTube, Connectors.Stac → V2

## Alternatives considérées

| Alternative | Avantages | Inconvénients |
|---|---|---|
| Monolithe | Simple | Pas de séparation, difficile à tester et étendre |
| 14 projets (initial) | Séparation maximale | Bureaucratie d'architecture pour porteur solo |
| **6 projets (décision V1)** | Bon équilibre séparation/complexité | À découpler quand l'équipe grandit |
| Microservices | Scalabilité | Trop complexe pour porteur solo |
| Python | Écosystème data/ML | Typage dynamique, performance moindre |

## Justification du recentrage

1. **14 projets = sur-ingénierie** — Chaque projet = .csproj, références, namespace, tests séparés
2. **Porteur solo** — La maintenance de 14 projets est disproportionnée à la valeur délivrée
3. **2 connecteurs V1** — Un seul projet Connectors suffit
4. **Fusion/Scoring/CaseManagement** — Sont des services métier, pas des couches indépendantes en V1
5. **Quand restaurer le découpage ?** — Quand un connecteur devient complexe (YouTube V2) → projet séparé. Quand Fusion dépasse 500 lignes → namespace → module séparé. Quand l'équipe dépasse 2 développeurs.

## Conséquences

- Courbe d'apprentissage .NET pour développeurs non familiers — inchangé
- Communication IPC Electron ↔ C# à gérer — inchangé
- Moins de projets à maintenir — positif
- Startup time du backend .NET — inchangé
- Les modules Fusion/Scoring/CaseManagement sont des namespaces dans Application, pas des projets séparés — à découpler si nécessaire en V2+

## Références

- [10-mvp-solo-v1-officiel.md](../review/10-mvp-solo-v1-officiel.md) section Architecture V1
- [09-plan-de-recentrage-applique.md](../review/09-plan-de-recentrage-applique.md) arbitrage A1
- [02-architecture-technique.md](../specs/02-architecture-technique.md)