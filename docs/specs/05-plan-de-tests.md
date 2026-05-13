# Aegis Loop — Plan de tests

> **Version :** 2.0  
> **Statut :** Aligné V1 officielle  
> **Dernière mise à jour :** 2026-04-23  
> **Document amont :** [10-mvp-solo-v1-officiel.md](../review/10-mvp-solo-v1-officiel.md)

---

## 1. Stratégie de test V1

### 1.1 Objectif

Valider que le MVP V1 fonctionne correctement sur le chemin critique de la démo : ingestion RSS/GDELT → normalisation → fusion → scoring → affichage → feedback → export.

### 1.2 Catégories de tests — 5 catégories

| Catégorie | Outil | Priorité | Volume estimé |
|---|---|---|---|
| **Unitaires** | xUnit + FluentAssertions | Must | ~50 tests |
| **Intégration** | xUnit + WebApplicationFactory | Must | ~20 tests |
| **Contrat connecteurs** | xUnit | Must | ~5 tests |
| **E2E (smoke)** | Playwright | Must | ~5 tests |
| **Robustesse** | xUnit | Should | ~10 tests |

**Total estimé :** ~80-100 tests

> **Exclus V1 :** Tests de performance, tests de sécurité dédiés, tests de non-régression GUI (repoussés V2).

---

## 2. Tests unitaires (~50)

### 2.1 Domaine (15 tests)

| Test | Description |
|---|---|
| Observation_Create_Valid | Création d'une observation valide |
| Observation_Create_InvalidSourceHash | SourceHash vide → erreur |
| EventCase_MustHaveObservation | EventCase sans observation → erreur |
| ConfidenceScore_InRange | Score ∈ [0.0, 1.0] |
| ConfidenceScore_Clamp | Score > 1.0 clampé à 1.0 |
| AnalystFeedback_ImmutableAfterWindow | Feedback immutable après 5 min |
| AnalystFeedback_CancelableWithinWindow | Annulation possible dans les 5 min |
| Contradiction_DifferentObservations | Observation1Id ≠ Observation2Id |
| Location_ValidCoordinates | Lat ∈ [-90,90], Lon ∈ [-180,180] |
| Location_InvalidCoordinates | Coordonnées hors plage → erreur |
| SourceConnector_ActiveMustHaveConfig | Connecteur actif sans config → erreur |
| AuditEntry_AppendOnly | AuditEntry jamais modifié |
| Entity_NormalizedName | NormalizedName en minuscules sans accents |
| IngestionJob_CompletedHasDate | Job terminé a CompletedAt |
| RawItem_SourceHashRequired | RawItem sans SourceHash → erreur |

### 2.2 Scoring (10 tests)

| Test | Description |
|---|---|
| Score_Calculation_3Components | Score = 0.35×Fiabilité + 0.35×Corroboration + 0.30×Feedback |
| Score_FiabilitéSource_DefaultValues | Vérifier les scores par défaut (Institutionnel 0.85, Média 0.75, etc.) |
| Score_Corroboration_SingleSource | Corroboration = 0.0 pour 1 source |
| Score_Corroboration_TwoSources | Corroboration > 0 pour 2 sources |
| Score_FeedbackAnalyst_Default | Feedback = 0.0 par défaut |
| Score_FeedbackAnalyst_AfterConfirm | Feedback augmente après confirmation |
| Score_FeedbackAnalyst_AfterInvalidate | Feedback diminue après invalidation |
| Score_EventCase_WeightedAverage | Score EventCase = moyenne pondérée × FacteurCorroboration |
| Score_Reproducible | Mêmes entrées → même score |
| Score_WeightsConfigurable | Les poids sont configurables |

### 2.3 Normalisation (10 tests)

| Test | Description |
|---|---|
| Normalize_RSS_ValidItem | RSS → Observation correcte |
| Normalize_RSS_InvalidFeed | Flux malformé → erreur journalisée |
| Normalize_GDELT_ValidItem | GDELT → Observation correcte |
| Normalize_GDELT_InvalidData | Données invalides → erreur journalisée |
| Normalize_Idempotent | Re-traiter un RawItem → même résultat |
| Normalize_MetadataExtensible | Champs non mappés → Metadata |
| Normalize_ClaimTextExtracted | ClaimText extrait du contenu |
| Normalize_GeocodingResolved | Géocodage résolu si lieu détecté |
| Normalize_GeocodingFallback | Géocodage non résolu → observation sans coordonnées |
| Normalize_Deduplication | Doublon détecté par SourceHash |

### 2.4 Fusion/correlation (10 tests)

| Test | Description |
|---|---|
| Fusion_TwoObservations_SameEvent | 2 observations similaires → même événement |
| Fusion_DifferentSources | Sources différentes → même événement possible |
| Fusion_NoMatch_NewCluster | Observation isolée → nouveau cluster candidat |
| Fusion_ClusterPromotion | Cluster ≥ 2 obs de ≥ 2 sources → EventCase |
| Fusion_ClusterNotPromoted | Cluster < 2 sources → pas d'EventCase |
| Fusion_TemporalProximity | Proximité temporelle vérifiée |
| Fusion_GeographicProximity | Proximité géographique vérifiée |
| Fusion_TextualSimilarity | Similarité textuelle vérifiée |
| Fusion_CommonEntities | Entités communes vérifiées |
| Fusion_ContradictionDetection | Contradiction détectée si incompatibilité |

### 2.5 Pipeline ingestion (5 tests)

| Test | Description |
|---|---|
| Pipeline_FullFlow | Collecte → RawItem → Observation → EventCase → Score |
| Pipeline_NonBlocking | Pipeline ne bloque pas l'interface |
| Pipeline_ErrorRecovery | Erreur partielle → items préservés |
| Pipeline_IngestionJob_Created | Chaque collecte crée un IngestionJob |
| Pipeline_IngestionJob_Status | Statut mis à jour correctement |

---

## 3. Tests d'intégration (~20)

| Test | Description |
|---|---|
| API_CRUD_Events | GET/POST/PATCH /api/events |
| API_CRUD_Observations | GET /api/observations |
| API_Provenance | GET /api/observations/{id}/provenance |
| API_Feedback | POST /api/feedback → score recalculé |
| API_Connectors | GET/POST /api/connectors |
| API_Ingestion | POST /api/ingestion/run → jobs créés |
| API_ScoringBreakdown | GET /api/scoring/{id}/breakdown |
| API_Dashboard | GET /api/dashboard → KPIs |
| API_Export | GET /api/export/{id} → Markdown/JSON |
| API_Demo | POST /api/demo/load → données chargées |
| API_DemoReset | POST /api/demo/reset → données réinitialisées |
| API_SSE | GET /api/events/stream → notifications |
| API_HealthCheck | GET /health → 200 OK |
| API_Pagination | GET /api/events?page=1&pageSize=10 |
| API_Filtering | GET /api/events?source=rss&minScore=0.5 |
| EF_Core_Migrations | Migrations s'appliquent correctement |
| EF_Core_SeedData | Seed data insérées correctement |
| EF_Core_CRUD | Opérations CRUD via EF Core |
| API_ErrorHandling | Erreurs retournées au format standardisé |
| API_CORS | CORS restreint à l'origine Electron |

---

## 4. Tests de contrat connecteurs (~5)

| Test | Description |
|---|---|
| ISourceConnector_RSS_Implements | RSS implémente ISourceConnector |
| ISourceConnector_GDELT_Implements | GDELT implémente ISourceConnector |
| ISourceConnector_RateLimiting | Rate limiting respecté |
| ISourceConnector_Retry | Retry avec backoff exponentiel |
| ISourceConnector_HealthCheck | HealthCheck retourne un résultat |

---

## 5. Tests E2E / smoke tests (~5)

| Test | Description |
|---|---|
| Smoke_AppLaunch | Application lançable localement |
| Smoke_Demo5Min | Démo 5 min sans crash |
| Smoke_FullScenario1 | Scénario crise Soudan complet |
| Smoke_FullScenario2 | Scénario maritime Golfe d'Aden |
| Smoke_OfflineMode | Mode démo fonctionne sans réseau |

### 8 checks dans le smoke test démo

- Application lançable localement
- Dashboard affiche des événements avec scores
- EventCase montre la provenance complète
- L'analyste peut valider/invalider et voir l'impact
- La carte affiche les observations géolocalisées
- Le mode démo fonctionne sans réseau
- Les contradictions sont visibles dans l'interface
- L'export Markdown fonctionne

---

## 6. Tests de robustesse (~10)

| Test | Description |
|---|---|
| Robust_RSS_Unreachable | Source RSS injoignable → pas de crash |
| Robust_RSS_Malformed | Flux RSS malformé → erreur journalisée |
| Robust_GDELT_Timeout | Timeout GDELT → retry + pas de crash |
| Robust_GDELT_InvalidResponse | Réponse GDELT invalide → erreur journalisée |
| Robust_DuplicateRawItems | RawItems en doublon → déduplication |
| Robust_LargeVolume | 500+ observations → performance acceptable |
| Robust_ConcurrentIngestion | 2 connecteurs en parallèle → pas de conflit |
| Robust_BackendCrash | Crash backend → Electron affiche écran d'erreur |
| Robust_PortConflict | Port occupé → essai port suivant |
| Robust_DemoReset | Réinitialisation démo → données cohérentes |

---

## 7. 10 tests critiques

1. Normalisation RSS → Observation
2. Normalisation GDELT → Observation
3. Scoring avec 3 composantes → score ∈ [0, 1]
4. Fusion d'observations en événement
5. Détection de contradiction
6. Feedback analyste modifie le score
7. Pipeline ingestion bout en bout
8. API CRUD observations
9. Contrat ISourceConnector respecté
10. Smoke test démo 5 min

---

## 8. Jeux de données de test

| Niveau | Description | Usage |
|---|---|---|
| **Minimal** | 1 observation, 1 événement | Tests unitaires rapides |
| **Seed Démo** | 90 observations, 8 événements, 3 contradictions | Tests d'intégration et smoke |
| **Stress** | 500 observations, 50 événements | Tests de robustesse |
| **Edge** | Observations malformées, doublons, contradictions | Tests de robustesse |

---

## 9. Configuration CI

```yaml
# build.yml
Triggers: push sur main, pull_request
Matrice: windows-latest, ubuntu-latest, macos-latest
Steps:
  - dotnet test (unitaires + intégration + contrat + robustesse)
  - npm run test:e2e (smoke tests)
```

---

## Références

- MVP Solo V1 Officiel : [10-mvp-solo-v1-officiel.md](../review/10-mvp-solo-v1-officiel.md)