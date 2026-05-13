# Aegis Loop — Rapport de synchronisation finale

> **Version :** 1.0  
> **Date :** 2026-04-23  
> **Source de vérité :** [10-mvp-solo-v1-officiel.md](10-mvp-solo-v1-officiel.md)

---

## 1. Résumé exécutif

La synchronisation finale du corpus Aegis Loop V1 a été effectuée le 2026-04-23. Les 7 documents specs (01 à 07), les 8 fichiers .progress.md, et l'ADR 0007 ont été mis à jour pour être strictement cohérents avec la V1 officielle définie dans `10-mvp-solo-v1-officiel.md`.

**Verdict : Corpus synchronisé prêt.**

Aucune contradiction majeure restante. Les 6 points encore ouverts sont des décisions d'implémentation (pas des contradictions documentaires) et sont explicitement nommés.

---

## 2. Documents mis à jour

| Document | Version | Aligné V1 | Modifications principales |
|---|---|---|---|
| `01-specs-fonctionnelles.md` | 2.0 | ✅ Oui | Backlog 17 items, 11 types, 3 composantes scoring, CU YouTube/STAC/Watchlists supprimés, seed data ajouté, "Démo prête" repris |
| `02-architecture-technique.md` | 2.0 | ✅ Oui | 6 projets C#, 11 types domaine, ~20 endpoints, cycle de vie Electron/.NET, connecteurs RSS+GDELT, types exclus V1 |
| `03-specs-ui-ux.md` | 2.0 | ✅ Oui | 5 vues, 10 composants, 9 raccourcis, vues fusionnées, scénarios GUI alignés |
| `04-manuel-utilisateur.md` | 2.0 | ✅ Oui | Parcours V1 uniquement (RSS/GDELT), sections YouTube/STAC/Watchlists/PDF supprimées, mode démo documenté |
| `05-plan-de-tests.md` | 2.0 | ✅ Oui | 5 catégories, ~80-100 tests, 10 tests critiques, smoke tests démo, tests exclus V1 |
| `06-dossier-presentation-produit.md` | 2.0 | ✅ Oui | Pitch aligné, "Ce que V1 ne fait PAS" explicite, 12 conditions démo prête |
| `07-glossaire-et-decisions.md` | 2.0 | ✅ Oui | Glossaire V1 nettoyé, termes exclus documentés, 14 arbitrages structurants |
| `0007-securite-tracabilite-conformite.md` | Corrigé | ✅ Oui | AES en V1 (pas DPAPI), ReportExport supprimé, cycle de vie ajouté |
| `00-expression-besoins.progress.md` | 2.0 | ✅ Oui | Progress file aligné |
| `01-specs-fonctionnelles.progress.md` | 2.0 | ✅ Oui | Progress file aligné |
| `02-architecture-technique.progress.md` | 2.0 | ✅ Oui | Progress file aligné |
| `03-specs-ui-ux.progress.md` | 2.0 | ✅ Oui | Progress file aligné |
| `04-manuel-utilisateur.progress.md` | 2.0 | ✅ Oui | Progress file aligné |
| `05-plan-de-tests.progress.md` | 2.0 | ✅ Oui | Progress file aligné |
| `06-dossier-presentation-produit.progress.md` | 2.0 | ✅ Oui | Progress file aligné |
| `07-glossaire-et-decisions.progress.md` | 2.0 | ✅ Oui | Progress file aligné |

---

## 3. Contradictions corrigées

| Document | Contradiction | Correction |
|---|---|---|
| `01-specs-fonctionnelles.md` | Backlog à 32+ items | Réduit à 17 items (12 Must + 5 Should) |
| `01-specs-fonctionnelles.md` | Types de domaine à 17 | Réduit à 11 types |
| `01-specs-fonctionnelles.md` | Scoring à 5 composantes | Réduit à 3 composantes |
| `01-specs-fonctionnelles.md` | Cas d'usage YouTube, STAC, Import, Watchlists | Supprimés V1, repoussés V2 |
| `01-specs-fonctionnelles.md` | Recherche avancée en V1 | Simplifiée en filtres + Ctrl+K |
| `02-architecture-technique.md` | 14 projets C# | Réduit à 6 projets |
| `02-architecture-technique.md` | 30+ endpoints API | Réduit à ~20 endpoints |
| `02-architecture-technique.md` | Types Claim, Evidence, ConnectorConfiguration séparés | Fusionnés (Claim→Observation, Evidence→Observation, Config→SourceConnector) |
| `02-architecture-technique.md` | Cycle de vie Electron/.NET absent | Ajouté explicitement |
| `03-specs-ui-ux.md` | 9 vues UI | Réduit à 5 vues |
| `03-specs-ui-ux.md` | 14+ composants UI | Réduit à 10 (7 Must + 3 Should) |
| `03-specs-ui-ux.md` | 18 raccourcis clavier | Réduit à 9 |
| `03-specs-ui-ux.md` | Timeline avancée | Remplacée par TimelineSimple |
| `04-manuel-utilisateur.md` | Sections YouTube, STAC, Import, Watchlists, PDF | Supprimées V1 |
| `05-plan-de-tests.md` | 11 catégories de tests | Réduit à 5 |
| `05-plan-de-tests.md` | 200+ tests | Réduit à ~80-100 |
| `06-dossier-presentation-produit.md` | Pitch annonçant YouTube, STAC, multi-source | Aligné sur RSS + GDELT uniquement |
| `07-glossaire-et-decisions.md` | Termes "fantômes" (Claim, Evidence, MediaAsset, Watchlist, SearchQuery, EntityLink, ReportExport, ConnectorConfiguration) | Documentés comme exclus V1 ou fusionnés |
| `0007-securite-tracabilite-conformite.md` | Chiffrement DPAPI/Keychain en V1 | Corrigé en AES en V1, OS-native en V2 |
| `0007-securite-tracabilite-conformite.md` | ReportExport comme type domaine | Corrigé en DTO Application + audit trail |

---

## 4. Éléments supprimés du corpus V1

| Élément | Document source | Raison |
|---|---|---|
| Connecteur YouTube | 01, 02, 03, 04, 06 | V2 |
| Connecteur STAC | 01, 02, 03, 04, 06 | V2 |
| Import manuel CSV/JSON/GeoJSON/KML | 01, 02, 04 | V2 |
| Watchlists | 01, 03, 04, 07 | V2 |
| Export PDF | 01, 03, 04, 06 | V2 |
| Recherche avancée (full-text) | 01, 03, 04 | V2 |
| Timeline avancée | 01, 03, 04 | V2 |
| Active learning | 01, 06, 07 | V3 |
| NLP/entity linking | 01, 06, 07 | V3 |
| Plugins connecteurs | 01, 07 | V3 |
| Types Claim, Evidence, MediaAsset, SearchQuery, EntityLink, Watchlist, ReportExport, ConnectorConfiguration | 02, 07 | Fusionnés ou V2 |
| Vue Contradictions séparée | 03 | Fusionnée dans EventCase |
| Vue Export séparée | 03 | Fusionnée dans EventCase |
| Vue Recherche avancée séparée | 03 | Fusionnée dans Observations |
| Vue Connecteurs & Jobs séparée | 03 | Fusionnée dans Dashboard/Paramètres |
| Composants SearchAdvanced, WatchlistManager, ExportWizard, TimelineAvancée | 03 | V2 |
| 9+ raccourcis supprimés | 03 | Réduit à 9 |
| Projets Connectors.Abstractions, Contracts, Fusion, Scoring, CaseManagement, Connectors.Rss, Connectors.Gdelt, Connectors.YouTube | 02 | Fusionnés |
| Endpoints Entity, Location, AuditEntry, Watchlist, SearchQuery, CaseFile, Search | 02 | Supprimés |
| Tests performance, sécurité dédiés, non-régression GUI | 05 | V2 |
| Chiffrement OS-native en V1 | 0007 | V2 |
| ReportExport type domaine | 0007, 02 | DTO Application |

---

## 5. Éléments déplacés en V2/V3

| Élément | Destination | Justification |
|---|---|---|
| Connecteur YouTube Data API | V2 | Quota API, CGU, complexité |
| Connecteur STAC/Copernicus | V2 | Valeur démo limitée sans images |
| Import manuel multi-format | V2 | Pas critique pour la démo |
| Watchlists | V2 | Complexe à implémenter |
| Export PDF | V2 | Complexité de génération |
| Timeline avancée | V2 | Composant lourd |
| Recherche avancée (full-text) | V2 | Filtres simples suffisent en V1 |
| Fraîcheur comme composante de scoring | V2 | C'est un tri, pas un score |
| Spécificité/Complétude comme composantes | V2 | Trop subjectif à calibrer |
| EntityLink (relations typées) | V2 | Enrichissement |
| MediaAsset | V2 | Avec connecteur YouTube |
| SearchQuery | V2 | Avec recherche avancée |
| Chiffrement OS-native (DPAPI/Keychain) | V2 | AES suffit en V1 |
| Géocodeur local (GeoNames) | V2 | Nominatim + cache suffit en V1 |
| Active learning | V3 | Prématuré |
| NLP/entity linking | V3 | Prématuré |
| Plugins connecteurs | V3 | Pas de besoin en V1 |
| Projets C# séparés (si équipe > 2) | V3 | Quand la complexité le justifiera |

---

## 6. Points encore ouverts

| Point | Statut | Action requise | Impact |
|---|---|---|---|
| Schéma JSON Schema formel pour l'export | À définir | Pendant implémentation | Mineur — le format est défini, le schéma formel est optionnel |
| Format exact du fichier user config | À définir | Pendant implémentation | Mineur — hiérarchie définie |
| Composant timeline exact (lib vs custom) | À décider | Pendant Phase 2 | Moyen — impacte le rendu |
| Stratégie de géocodage offline | À affiner | Nominatim + cache en V1, GeoNames locale en V2 | Mineur — approche V1 définie |
| Contenu exact des 90 observations seed | À rédiger | Pendant Phase 3 | Moyen — structure définie, contenu à rédiger |
| Politique de purge AuditEntry | À confirmer | > 90 jours par défaut, configurable | Mineur — défaut défini |

> **Note :** Ces points ouverts sont des décisions d'implémentation, pas des contradictions documentaires. Ils ne bloquent pas la synchronisation du corpus.

---

## 7. Vérification de cohérence finale

| Contrainte V1 | État dans corpus synchronisé | Conforme ? |
|---|---|---|
| 6 projets C# | 6 projets + desktop-electron | ✅ |
| 17 items backlog | 12 Must + 5 Should = 17 | ✅ |
| 11 types domaine | 11 types | ✅ |
| 3 composantes scoring | FiabilitéSource + Corroboration + FeedbackAnalyste | ✅ |
| 5 vues UI | Dashboard, Carte+Timeline, EventCase, Observations, Paramètres | ✅ |
| ~20 endpoints API | 21 endpoints | ✅ |
| 2 connecteurs V1 | RSS + GDELT | ✅ |
| 5 catégories tests | Unitaires, Intégration, Contrat, E2E, Robustesse | ✅ |
| ~80-100 tests | 80-100 estimés | ✅ |
| 9 raccourcis | 9 raccourcis | ✅ |
| 10 composants UI | 7 Must + 3 Should = 10 | ✅ |
| Seed data définies | 2 scénarios, 90 obs | ✅ |
| Cycle de vie Electron/.NET | Spécifié | ✅ |
| Invariants métier | 10 invariants | ✅ |
| Définition "Démo prête" | 12 conditions reprises partout | ✅ |
| Claim/Evidence/ConnectorConfiguration fusionnés | Documentés dans tous les specs | ✅ |
| Contradictions simplifiées | Détection + affichage + résolution manuelle | ✅ |

---

## 8. Verdict final

**Corpus synchronisé prêt.**

- Toutes les contradictions majeures ont été corrigées
- Les 7 specs sont alignées sur `10-mvp-solo-v1-officiel.md`
- Les 8 progress files sont à jour
- L'ADR 0007 est corrigé
- Les 6 points ouverts sont des décisions d'implémentation, pas des incohérences
- Aucune réintroduction de fonctionnalité exclue de V1
- Aucune réouverture d'arbitrage

Le corpus est prêt à servir de base officielle avant implémentation.