# Aegis Loop — Expression des besoins

> **Version :** 2.0 — Recentrée après audit  
> **Statut :** Référence — Spec-first, recentrée  
> **Dernière mise à jour :** 2026-04-23  
> **Lien vers progression :** [00-expression-besoins.progress.md](00-expression-besoins.progress.md)  
> **Document de référence V1 :** [../review/10-mvp-solo-v1-officiel.md](../review/10-mvp-solo-v1-officiel.md)

---

## 1. Vision produit

### 1.1 Énoncé de vision

Aegis Loop est une **couche logicielle de fusion, qualification, traçabilité, explicabilité et apprentissage continu** pour signaux hétérogènes issus de sources OSINT publiques et d'autres sources injectables.

Le système transforme des alertes et observations disparates en **événements corrélés, priorisés, explicables et exploitables** par un analyste humain.

Aegis Loop n'est PAS :
- un constructeur de drones ou de capteurs,
- une plateforme ISR complète et mature,
- un produit magique qui prétend tout faire,
- un moteur de scraping agressif,
- un système offensif.

Aegis Loop EST :
- un **workbench analyste desktop**,
- une couche de **fusion et de qualification** au-dessus de sources hétérogènes,
- un système où **l'humain reste dans la boucle**,
- un outil **lawful, éthique, traçable et audit-friendly**.

### 1.2 Metaphore centrale

> Si les signaux OSINT sont des fils de laine de couleurs et qualités différentes, Aegis Loop est le **métier à tisser** qui les assemble en un tissu cohérent, dont on peut tracer chaque fil jusqu'à sa provenance, et dont les défauts sont signalés au tisserand.

### 1.3 Nomenclature

Le nom principal du produit est **Aegis Loop**.

- *Aegis* évoque la protection, la vigilance, le bouclier — la couche de qualification qui protège l'analyste du bruit.
- *Loop* évoque la boucle de feedback, l'apprentissage continu, le cycle observation → corrélation → validation → apprentissage.

L'alias **Signal Loom** est mentionné comme alternative sémantique (le métier à tisser de signaux), mais Aegis Loop est retenu comme nom principal pour sa consonance défense/sécurité et sa mémorabilité.

---

## 2. Problème métier

### 2.1 Le problème

Les analystes en sécurité, défense, renseignement et surveillance d'événements font face à un problème récurrent :

1. **Volume et hétérogénéité** — Les signaux pertinents arrivent de nombreuses sources publiques (RSS, GDELT, APIs ouvertes, métadonnées géospatiales, médias), dans des formats et structures incompatibles.
2. **Absence de corrélation** — Chaque source est consultée isolément. Les recoupements sont faits manuellement, au prix d'un effort considérable.
3. **Confiance non qualifiée** — Aucune indication systématique sur la fiabilité d'une source, d'une observation, d'un recoupement.
4. **Provenance perdue** — Une fois l'information intégrée, on perd souvent la trace de son origine, de sa date de collecte, de ses transformations.
5. **Contradictions invisibles** — Des observations contradictoires coexistent sans être détectées ou signalées.
6. **Feedback non capitalisé** — L'analyste valide, corrige, enrichit, mais ce travail n'alimente pas le système.
7. **Priorisation opaque** — Quand des centaines d'alertes se累积nt, l'analyste n'a pas de mécanisme transparent de tri.

### 2.2 Conséquences

- Temps analyste perdu à des tâches manuelles de recoupement.
- Risque de manquer des corrélations critiques.
- Décisions basées sur des informations dont la confiance est inconnue.
- Difficulté à constituer des dossiers exploitables et présentables.
- Impossibilité de démontrer la chaîne de preuve derrière une alerte.

### 2.3 Pourquoi les solutions existantes ne suffisent pas

| Catégorie de solution | Limite pour l'analyste solo / petite équipe |
|---|---|
| Plateformes ISR complètes (Palantir, etc.) | Coût prohibitif, infrastructure lourde, sur-spécialisées |
| Outils OSINT fragmentés | Pas de fusion, pas de corrélation, pas de scoring unifié |
| Tableaux de bord de monitoring | Passifs, sans feedback, sans fusion |
| Notebooks Jupyter | Trop bas niveau, pas de workbench intégré, pas de scoring persistant |
| SIEM / log management | Conçus pour l'IT, pas pour le renseignement multi-source |

---

## 3. Contexte du challenge

### 3.1 Positionnement stratégique

Le projet vise un **edge réaliste pour un porteur de projet unique**. Il s'inscrit dans un contexte de challenge défense / sécurité / renseignement / analyse multi-source.

L'edge retenu est la **couche de fusion + confiance + feedback analyste**. Ce positionnement est délibérément :

- **Complémentaire** aux capteurs et collecteurs existants,
- **Non concurrentiel** avec les plateformes ISR matures,
- **Lawful et éthique** par conception,
- **Démontrable rapidement** avec des sources publiques stables,
- **Extensible** à terme par des connecteurs supplémentaires.

### 3.2 Pourquoi ce projet est réaliste pour une seule personne

1. **Périmètre maîtrisé** — Le MVP se limite à 2-3 scénarios, quelques connecteurs stables, et un scoring heuristique explicable.
2. **Stack éprouvée** — Electron + React + C#/.NET sont des technologies matures avec une documentation abondante.
3. **Pas d'infra distribuée** — Tout tourne en local, pas de serveur à maintenir, pas de DevOps complexe.
4. **Sources publiques et légales** — RSS, GDELT, YouTube Data API, STAC sont accessibles sans contrat spécifique.
5. **Architecture modulaire** — Chaque connecteur, chaque module de scoring est indépendant et peut être développé incrémentalement.
6. **Mode démo intégré** — Des datasets seed permettent des démonstrations sans dépendance externe.
7. **Spec-first** — La phase de spécification garantit que l'implémentation est ciblée et non dispersée.

### 3.3 Pourquoi la couche fusion + confiance + feedback est l'edge

C'est précisément là que se trouve le **vide de marché** pour un outil analyste solo :

- Les outils de collecte existent, mais ils ne fusionnent pas.
- Les outils de visualisation existent, mais ils ne qualifient pas.
- Les outils de corrélation existent en entreprise, mais pas en version analyste desktop.
- Le feedback analyste n'est capturé nulle part de façon systématique.

En se positionnant sur cette couche, Aegis Loop offre une valeur unique sans affronter les géants du secteur.

---

## 4. Objectifs

### 4.1 Objectifs principaux

| # | Objectif | Critère de succès |
|---|---|---|
| O1 | Fusionner des observations hétérogènes en un modèle unifié | Au moins 2 types de sources corrélées dans le MVP (RSS + GDELT) |
| O2 | Qualifier la confiance de chaque fait avec un score explicable | Tout fait affiché a un score et une provenance traçable |
| O3 | Détecter et signaler les contradictions entre sources | Les contradictions sont listées et accessibles en un clic |
| O4 | Capitaliser le feedback analyste pour améliorer la priorisation | Le feedback modifie le scoring des observations futures |
| O5 | Constituer des dossiers / case files exportables | Un dossier peut être créé, enrichi, exporté en Markdown/JSON (PDF en V2) |
| O6 | Fournir une interface analyste desktop riche et impressionnante | La démo impressionne un évaluateur technique en moins de 10 minutes |
| O7 | Être déployable facilement depuis GitHub | Clone → build → run en moins de 30 minutes |

### 4.2 Non-objectifs (explicitement exclus du MVP)

| # | Non-objectif | Raison |
|---|---|---|
| N1 | Temps réel basse latence (< 1 min) | Trop complexe pour un porteur solo, polling batch suffisant |
| N2 | Multi-utilisateur / collaboration | Infrastructure serveur hors périmètre MVP |
| N3 | Scraping agressif ou contournement de CGU | Non lawful, non éthique |
| N4 | IA/ML opaque ou complexe | Heuristiques explicables suffisent en V1 |
| N5 | Connecteurs à des plateformes fermées instables | Fragilité et risque juridique |
| N6 | Module offensif | Hors périmètre et hors éthique |
| N7 | Déploiement cloud natif | MVP = desktop local |
| N8 | Support mobile | MVP = desktop |
| N9 | Connecteurs YouTube, STAC, Import manuel en V1 | Repoussés en V2 — RSS + GDELT suffisent pour la démo |
| N10 | Watchlists, recherche avancée, timeline avancée | Repoussés en V2 — complexité disproportionnée pour un MVP solo |
| N11 | Export PDF | Repoussé en V2 — Markdown + JSON suffisent |

---

## 5. Utilisateurs cibles

### 5.1 Persona principal : Analyste solo

- **Profil :** Analyste renseignement / sécurité / défense, travaillant seul ou en petite équipe
- **Contexte :** Besoin de recouper des informations publiques pour produire des analyses
- **Frustrations :** Trop de sources, pas de fusion, confiance inconnue, contradictions invisibles
- **Attentes :** Un outil qui lui donne des faits qualifiés, pas des raw data

### 5.2 Persona secondaire : Évaluateur technique

- **Profil :** Évaluateur de challenge / incubateur / partenaire industriel
- **Contexte :** Évalue la crédibilité, la modularité, la qualité du démonstrateur
- **Attentes :** Démo impressionnante, architecture solide, potentiel d'extension

### 5.3 Persona tertiaire : Développeur extérieur (futur)

- **Profil :** Développeur voulant ajouter un connecteur ou un module de scoring
- **Contexte :** Architecture modulaire permettant l'extension
- **Attentes :** Documentation claire, interfaces stables, onboarding rapide

---

## 6. Scénarios d'usage principaux

### 6.1 Scénario 1 — Surveillance événementielle multi-source (priorité MUST)

**Titre :** « Suivi d'une crise émergente via sources publiques »

**Description :** Un analyste configure des connecteurs RSS (médias internationaux), GDELT (événements globaux) et des imports manuels pour suivre une situation en évolution (ex : crise humanitaire, conflit, catastrophe naturelle). Le système collecte les observations, les normalise, détecte les entités et événements émergents, les corrèle, calcule des scores de confiance, et présente à l'analyste une vue unifiée avec provenance et contradictions mises en évidence.

**Flux :**
1. L'analyste active les connecteurs RSS et GDELT
2. Le système collecte les observations et les normalise
3. Le système détecte des entités (lieux, organisations, personnes) et des événements
4. Le système corrèle les observations et calcule les scores de confiance
5. Le système présente les événements prioritaires sur le dashboard
6. L'analyste explore un événement, consulte la provenance, identifie une contradiction
7. L'analyste valide ou corrige l'information, ajoute une note
8. Le système capitalise le feedback et ajuste la priorisation
9. L'analyste constitue un case file et l'exporte

**Valeur démontrée :** Fusion multi-source, scoring, provenance, contradictions, feedback analyste, case file.

### 6.2 Scénario 2 — Suivi d'un événement maritime (priorité MUST)

**Titre :** « Incident maritime dans le Golfe d'Aden »

**Description :** Un analyste configure des connecteurs RSS (sources maritimes) et GDELT (filtre maritime) pour suivre un incident en mer. Le système affiche les observations géolocalisées sur la carte, détecte des clusters spatio-temporels, et permet à l'analyste de qualifier la confiance de chaque source.

**Flux :**
1. L'analyste configure GDELT (filtre maritime) et RSS (sources maritimes)
2. Le système collecte et normalise les observations géolocalisées
3. La carte affiche les observations et clusters
4. L'analyste recoupe les sources, vérifie la provenance géographique
5. Une contradiction sur la localisation est détectée et résolue
6. L'analyste constitue un case file géospatial et l'exporte

**Valeur démontrée :** Corrélation géospatiale, carte interactive, contradictions géographiques, provenance.

> **Note :** Le scénario géospatial original (STAC/Copernicus + Import GeoJSON) est repoussé en V2 quand ces connecteurs seront implémentés.

### 6.3 Scénario 3 — Construction de dossier d'analyse avec feedback itératif (priorité SHOULD)

**Titre :** « Constitution et enrichissement d'un dossier d'événements »

**Description :** L'analyste utilise Aegis Loop sur plusieurs jours pour suivre un dossier, ajouter des observations, valider ou invalider des corrélations, enrichir avec des notes, et exporter un rapport synthétique pour présentation.

**Flux :**
1. L'analyste crée un case file pour un dossier spécifique
2. Il ajoute des observations manuellement ou via connecteurs
3. Il valide ou corrige les corrélations proposées par le système
4. Il enrichit avec des notes et des tags
5. Il suit l'évolution via le dashboard et la timeline
6. Il exporte le dossier en Markdown/PDF pour présentation

**Valeur démontrée :** Case management, feedback itératif, enrichissement, export.

---

## 7. Risques

### 7.1 Risques techniques

| # | Risque | Impact | Probabilité | Mitigation |
|---|---|---|---|---|
| RT1 | Instabilité des sources OSINT publiques | Collecte interrompue | Moyenne | Mode démo avec datasets seed, retries configurables |
| RT2 | Volume de données GDELT trop important | Performance dégradée | Faible | Filtrage par région/thème, pagination, limitation configurable |
| RT3 | Complexité de la fusion multi-format | Retard de développement | Moyenne | Modèle de domaine unifié, normalisation incrémentale |
| RT4 | Quotas API YouTube / limites | Collecte limitée | Moyenne | Cache local, rate limiting configuré, mode démo |
| RT5 | Electron + C# : complexité IPC | Bugs de communication | Faible | REST local sur localhost, contrat clair, tests d'intégration |

### 7.2 Risques produit

| # | Risque | Impact | Probabilité | Mitigation |
|---|---|---|---|---|
| RP1 | Périmètre trop ambitieux pour un porteur solo | Échec du MVP | Moyenne | Spécification stricte du MVP, Must/Should/Could/Won't |
| RP2 | Démonstration pas assez impressionnante | Rejet par évaluateur | Faible | Mode démo soigné, seed data riche, UX soignée |
| RP3 | Positionnement flou entre outil et plateforme | Confusion utilisateur | Faible | Communication claire : workbench analyste, pas plateforme ISR |

### 7.3 Risques juridiques / éthiques

| # | Risque | Impact | Probabilité | Mitigation |
|---|---|---|---|---|
| RLE1 | Données personnelles dans les sources OSINT | Non-conformité RGPD | Moyenne | Filtrage à l'ingestion, anonymisation, pas de stockage de données personnelles non nécessaires |
| RLE2 | Utilisation abusive de l'outil | Réputation | Faible | Clause d'usage éthique, pas de fonctionnalités offensives |
| RLE3 | CGU des APIs violées par un connecteur | Blocage, responsabilité | Faible | Audit des CGU, respect strict, avertissement dans la config |
| RLE4 | Export de rapports contenant des données sensibles | Fuite | Moyen | Avertissement à l'export, possibilité de masquer les sources |

---

## 8. Hypothèses structurantes

| # | Hypothèse | Justification | Risque si invalidée |
|---|---|---|---|
| H1 | Un analyste solo est le persona principal | Contexte challenge, porteur unique | L'interface serait trop simple pour une équipe |
| H2 | Les sources RSS/GDELT sont suffisantes pour le MVP V1 | Sources publiques, stables, légales, sans clé API | La démo serait moins convaincante, nécessiterait d'autres connecteurs |
| H3 | Un scoring heuristique explicable suffit en V1 | Pas besoin de ML opaque pour un démonstrateur | Si le scoring est jugé trop simple, un NLP léger pourra être ajouté |
| H4 | Electron + C# est une stack viable pour un desktop analyste | Technologies matures, communauté large | Complexité IPC, mais mitigée par REST local |
| H5 | SQLite suffit pour la persistance MVP | Volume local, pas de concurrence | Si les volumes explosent, migration PostgreSQL possible |
| H6 | Le mode batch/polling est acceptable | Pas de besoin temps réel critique | Un connecteur temps réel pourra être ajouté en V2 |
| H7 | Le mode démo local avec datasets seed est acceptable pour les présentations | Pas de dépendance à des sources live | La démo doit être riche et réaliste |
| H8 | L'architecture modulaire permet l'extension future | Principes SOLID, séparation des préoccupations | Si le modèle de domaine est mal conçu, la modularité sera illusoire |

---

## 9. Priorités MVP

### 9.1 Must (V1 — Obligatoire, 12 items)

- Modèle de domaine (11 types, invariants explicites)
- Connecteurs RSS/Atom et GDELT (uniquement)
- Pipeline d'ingestion et normalisation
- Détection d'entités et fusion/correlation d'événements
- Scoring de confiance heuristique (3 composantes)
- Provenance de chaque fait
- Interface analyste desktop — Dashboard
- Interface analyste desktop — Vue EventCase
- Interface analyste desktop — Carte + Timeline simple
- Feedback analyste (validation / correction)
- Mode démo avec seed data (2 scénarios, 90 observations)
- Export Markdown/JSON

### 9.2 Should (V1 — Important, 5 items)

- Audit trail / journalisation
- Tags et notes simples
- Détection et affichage des contradictions
- Configuration des connecteurs
- Export Markdown/JSON (si Must décalé)

### 9.3 Repoussés en V2 (ex-V1 Should)

- Connecteur YouTube Data API
- Connecteur STAC/Copernicus
- Import manuel de fichiers
- Watchlists
- Export PDF
- Recherche avancée / filtres
- Timeline avancée
- Active learning

### 9.3 Could (V2 — Souhaitable)

- NLP léger (entity linking, classification)
- Connecteurs additionnels (maritimes, météo)
- Timeline avancée avec zoom
- Rapports personnalisables
- Mode serveur optionnel
- Plugins connecteurs par tiers

### 9.4 Won't (Exclu)

- Temps réel poussé
- Multi-utilisateur
- IA opaque non explicable
- Scraping agressif
- Fonctionnalités offensives
- Cloud natif
- Mobile

---

## 10. Feuille de route macro

```
V0 — Specs (actuelle)
  ├── Corpus documentaire complet
  ├── Architecture définie
  ├── Backlog initial
  └── Prêt pour implémentation

V1 — MVP recentré (~12 semaines)
  ├── Phase 1 (S1-4) : Domain (11 types) + RSS + Pipeline + API min + Dashboard
  ├── Phase 2 (S5-8) : GDELT + Scoring 3 comp. + Feedback + Carte/Timeline + Provenance
  ├── Phase 3 (S9-12) : Mode démo + Seed data + Contradictions + Export + Audit + Polish
  ├── 6 projets C#, 5 vues UI, ~20 endpoints, ~100 tests
  └── Critère "démo prête" défini (12 conditions)

V2 — Enrichissement (~2-3 mois après V1)
  ├── Connecteurs YouTube + STAC + Import manuel
  ├── Watchlists, recherche avancée
  ├── Timeline avancée, Export PDF
  ├── Scoring Fraîcheur + Spécificité
  ├── NLP léger (entity linking)
  └── Active learning

V3 — Extensibilité (2-3 mois)
  ├── Plugins connecteurs
  ├── Mode serveur optionnel
  ├── Rapports personnalisables
  ├── Connecteurs additionnels
  ├── API publique
  └── Documentation développeur
```

---

## 11. Contraintes du porteur solo

| Contrainte | Implication |
|---|---|
| Temps limité | Priorisation stricte, MVP focalisé |
| Pas d'équipe | Stack connue et bien documentée |
| Budget nul | Outils gratuits, sources publiques, pas d'infra |
| Démonstration courte | Mode démo riche et instantané |
| Maintenance future | Architecture modulaire, dette technique minimale |
| Visibilité | Code sur GitHub, documentation soignée |

---

## 12. Valeur ajoutée

### 12.1 Pour l'analyste

- **Gain de temps** — Corrélation automatique au lieu de recoupement manuel.
- **Qualification** — Confiance et provenance sur chaque fait.
- **Transparence** — Possibilité de répondre à "pourquoi je vois cette alerte ?".
- **Capitalisation** — Le feedback n'est pas perdu.
- **Export** — Dossiers présentables et traçables.

### 12.2 Pour l'évaluateur / partenaire

- **Modularité** — Architecture prête à être étendue.
- **Démonstrabilité** — Mode démo intégré, pas de dépendance live.
- **Éthique** — Lawful, privacy-aware, humain dans la boucle.
- **Qualité** — Spec-first, testable, auditable.

### 12.3 Pour le développeur futur

- **Extensibilité** — Nouveaux connecteurs via interface standard.
- **Documentation** — Specs complètes, ADR, glossaire.
- **Tests** — Stratégie de test définie dès le départ.

---

## 13. Références croisées

- Spécifications fonctionnelles : [01-specs-fonctionnelles.md](01-specs-fonctionnelles.md)
- Architecture technique : [02-architecture-technique.md](02-architecture-technique.md)
- Spécifications UI/UX : [03-specs-ui-ux.md](03-specs-ui-ux.md)
- Manuel utilisateur : [04-manuel-utilisateur.md](04-manuel-utilisateur.md)
- Plan de tests : [05-plan-de-tests.md](05-plan-de-tests.md)
- Dossier présentation : [06-dossier-presentation-produit.md](06-dossier-presentation-produit.md)
- Glossaire et décisions : [07-glossaire-et-decisions.md](07-glossaire-et-decisions.md)
- ADR : [../adr/](../adr/)