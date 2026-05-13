# Aegis Loop — Dossier de présentation produit

> **Version :** 2.0  
> **Statut :** Aligné V1 officielle  
> **Dernière mise à jour :** 2026-04-23  
> **Document amont :** [10-mvp-solo-v1-officiel.md](../review/10-mvp-solo-v1-officiel.md)

---

## 1. Pitch

**Aegis Loop** est un workbench analyste desktop qui fusionne, qualifie, trace et rend explicables des observations hétérogènes issues de sources OSINT publiques.

En **12 semaines**, un analyste solo peut disposer d'un outil qui :
- Collecte automatiquement des données depuis **RSS** et **GDELT** (aucune clé API requise)
- Fusionne et corrèle les observations en **événements structurés**
- Attribue un **score de confiance explicable** à 3 composantes (FiabilitéSource, Corroboration, FeedbackAnalyste)
- Détecte les **contradictions** entre sources
- Permet à l'analyste de **valider, corriger, annoter** et d'enregistrer son feedback
- Offre une **provenance complète** et traçable pour chaque information
- Exporte les dossiers en **Markdown et JSON**

Le tout dans une application desktop **lawful, traçable, éthique et défensif par construction**.

---

## 2. Problème

Les analystes OSINT travaillent avec des sources publiques multiples (flux RSS, bases de données événementielles) mais manquent d'outils pour :
- **Fusionner** les informations de sources hétérogènes
- **Qualifier** la fiabilité de l'information de manière explicable
- **Tracer** la provenance de chaque donnée
- **Détecter** les contradictions entre sources
- **Exporter** les résultats de manière structurée

Les outils existants sont soit trop complexes, soit trop limités, soit nécessitent une infrastructure serveur.

---

## 3. Solution V1

Aegis Loop V1 est un **workbench desktop solo** qui résout ces problèmes avec :

### 3.1 Fonctionnalités clés V1

| Fonctionnalité | Description | Statut |
|---|---|---|
| **Connecteurs RSS + GDELT** | Collecte automatique, polling, rate limiting | Must |
| **Pipeline d'ingestion** | Collecte → RawItem → Normalisation → Observation → Événement | Must |
| **Scoring explicable** | 3 composantes : FiabilitéSource, Corroboration, FeedbackAnalyste | Must |
| **Dashboard** | KPIs, événements prioritaires, contradictions | Must |
| **EventCase** | Détail événement, observations, provenance, score, contradictions | Must |
| **Feedback analyste** | Confirmer/invalider/corriger, recalcul du score | Must |
| **Mode démo** | 2 scénarios embarqués, sans réseau | Must |
| **Carte + Timeline** | Observations géolocalisées, timeline simple | Must |
| **Export Markdown/JSON** | Dossier événement exportable | Should |
| **Audit trail** | Journal append-only des actions | Should |
| **Tags/notes** | Annotation basique | Should |
| **Contradictions** | Détection + affichage simplifié | Should |

### 3.2 Ce que V1 ne fait PAS

Pour rester réaliste et livrable en 12 semaines, V1 ne comprend PAS :

- ❌ Connecteurs YouTube, STAC, Import manuel (→ V2)
- ❌ Watchlists (→ V2)
- ❌ Recherche avancée (→ V2)
- ❌ Export PDF (→ V2)
- ❌ Timeline avancée (→ V2)
- ❌ Active learning, NLP avancé (→ V3)
- ❌ Multi-utilisateur, serveur distant (→ V3)

---

## 4. Démo prête — 12 conditions

Le MVP V1 est prêt à être démontré quand les conditions suivantes sont **toutes** remplies :

1. ✅ Application desktop lançable localement (clone → build → run < 30 min)
2. ✅ Configuration minimale fonctionnelle (pas de clé API requise)
3. ✅ 2 connecteurs V1 fonctionnels (RSS + GDELT)
4. ✅ Ingestion batch/polling opérationnelle
5. ✅ Normalisation des observations (RSS → Observation, GDELT → Observation)
6. ✅ Création ou consolidation d'événements (clustering automatique)
7. ✅ Scoring explicable (3 composantes, décomposable, cliquable)
8. ✅ Consultation de provenance (chaîne complète visible)
9. ✅ Au moins 1 scénario analyste complet (crise géopolitique)
10. ✅ Seed/replay data stable (2 scénarios, 90 observations, 8 événements, 3 contradictions)
11. ✅ Export simple (Markdown + JSON)
12. ✅ Smoke tests verts (5 min de démo sans crash)

---

## 5. Architecture produit

```
┌─────────────────────────────────────────────────────────────┐
│                      Aegis Loop Desktop                       │
│  ┌─────────────────────────────────────────────────────────┐ │
│  │                 Electron + React + TS                    │ │
│  │  Dashboard │ Carte+Timeline │ EventCase │ Observations │ │
│  └────────────────────┬────────────────────────────────────┘ │
│                       │ REST localhost:5100                  │
│  ┌────────────────────┴────────────────────────────────────┐ │
│  │                  .NET 10 Backend                         │ │
│  │  Domain │ Application │ Infrastructure │ Connectors    │ │
│  │  Api    │ Worker       │ SQLite          │ RSS + GDELT  │ │
│  └─────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

**6 projets C#** au lieu de 14. Architecture simplifiée pour un porteur solo.

---

## 6. Stack technique

| Couche | Technologie |
|---|---|
| Shell desktop | Electron 41.3.0+ |
| Frontend | React 19 + TypeScript + Tailwind CSS + shadcn/ui |
| Backend | C# .NET 10 |
| API | ASP.NET Core Minimal APIs |
| Persistance | SQLite via EF Core 10 |
| Carte | MapLibre GL JS |
| Tests | xUnit + FluentAssertions + Playwright |

---

## 7. Planning V1 — 12 semaines

| Phase | Semaines | Contenu |
|---|---|---|
| **Phase 1** | 1-4 | Domain model (11 types) + Connecteur RSS + Pipeline + API minimale (5 endpoints) + Dashboard basique |
| **Phase 2** | 5-8 | Connecteur GDELT + Scoring 3 composantes + Feedback analyste + Carte/Timeline simple + Provenance visible |
| **Phase 3** | 9-12 | Mode démo + Seed data + Contradictions + Export Markdown + Audit trail + Polish UI + Smoke tests |

---

## 8. Scénarios de démo

### Scénario 1 — Crise au Soudan

**Titre :** « Suivi d'une crise géopolitique via RSS + GDELT »

L'analyste active les connecteurs RSS et GDELT, collecte des observations, voit les événements se former automatiquement avec scores de confiance, identifie une contradiction sur le bilan humain, valide la source la plus fiable, exporte le dossier.

**Valeur démontrée :** Fusion multi-source, scoring explicable, provenance, contradictions, feedback analyste, export.

### Scénario 2 — Incident maritime Golfe d'Aden

**Titre :** « Incident maritime dans le Golfe d'Aden »

L'analyste configure GDELT (filtre maritime) et RSS (sources maritimes), voit les observations géolocalisées sur la carte, recoupe les sources, détecte une contradiction sur la nature de l'attaque (missile vs drone), la résout.

**Valeur démontrée :** Corrélation géospatiale, carte interactive, contradictions géographiques.

---

## 9. Différenciation

| Critère | Aegis Loop V1 | Outils existants |
|---|---|---|
| **Scoring explicable** | 3 composantes décomposables | Score opaque ou absent |
| **Provenance complète** | Chaîne de bout en bout | Provenance partielle |
| **Détection de contradictions** | Automatique + résolution analyste | Manuelle |
| **Desktop solo** | Aucune infrastructure requise | Nécessite un serveur |
| **Mode démo** | 2 scénarios sans réseau | Pas de démo autonome |
| **Open source** | Code ouvert, traçable | Propriétaire ou limité |

---

## 10. Conditions de succès V1

- [ ] Application lançable en < 30 min (clone → build → run)
- [ ] Démo fonctionnelle sans réseau en < 5 min
- [ ] Score de confiance explicable et cliquable
- [ ] Provenance visible en 1 clic
- [ ] Contradictions détectées automatiquement
- [ ] Feedback analyste modifie le score en temps réel
- [ ] Export Markdown/JSON fonctionnel
- [ ] Smoke tests verts (5 min sans crash)

---

## Références

- MVP Solo V1 Officiel : [10-mvp-solo-v1-officiel.md](../review/10-mvp-solo-v1-officiel.md)
- Plan de recentrage appliqué : [09-plan-de-recentrage-applique.md](../review/09-plan-de-recentrage-applique.md)