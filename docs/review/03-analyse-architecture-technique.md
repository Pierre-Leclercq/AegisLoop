# Analyse de l'architecture technique

> **Date :** 2026-04-23  
> **Axe d'audit :** E — Architecture technique  
> **Gravité :** Critique (sur-découpage) à Modérée (choix technologiques)

---

## Verdict : Architecture saine mais sur-découpée pour V1

Les principes SOLID sont bien compris. La séparation Domain/Application/Infrastructure est correcte. Les choix technologiques (.NET 10, React, Electron, SQLite) sont réalistes. Mais le découpage en **14 projets C#** est le principal signal historique de sur-ingénierie — il crée une bureaucratie d'architecture disproportionnée pour un porteur solo.

Score : **6.5/10** — Correct mais à simplifier.

---

## 1. Sur-découpage modulaire — Problème critique

### Problème
14 projets C# pour un MVP solo signifie :
- 14 fichiers .csproj à maintenir
- 14× les références inter-projets
- 14 namespaces à gérer
- Des tests séparés par projet (6 projets de test)
- Un overhead de compilation et de configuration significatif

### Recommandation : réduire à 6-7 projets V1

| Projet V1 | Contient | Justification |
|---|---|---|
| **AegisLoop.Domain** | Modèle de domaine, invariants, interfaces | Séparation indispensable |
| **AegisLoop.Application** | Use cases, services, pipeline | Séparation indispensable |
| **AegisLoop.Infrastructure** | Persistance EF Core, config, logging | Séparation indispensable |
| **AegisLoop.Connectors** | Tous les connecteurs V1 (RSS + GDELT) | Un seul projet suffit pour 2 connecteurs |
| **AegisLoop.Api** | REST Minimal APIs | Couche API |
| **AegisLoop.Worker** | Service d'ingestion, host | Processus hôte |
| *(Optionnel)* **AegisLoop.Contracts** | DTOs publics | Si l'API est consommée par Electron |

**Total : 6-7 projets** au lieu de 14. Les modules Fusion, Scoring, CaseManagement sont intégrés dans Application en V1. Les connecteurs restent ensemble tant qu'ils ne sont que 2.

### Quand restaurer le découpage ?
- Quand un connecteur devient complexe (YouTube, STAC) → projet séparé
- Quand Fusion dépasse 500 lignes de logique → module séparé
- Quand l'équipe grandit au-delà de 2 développeurs

---

## 2. Communication Electron ↔ .NET — Point à clarifier

### Ce qui est spécifié
- REST sur localhost (port configurable)
- Electron lance le processus .NET au démarrage
- SSE pour les notifications

### Ce qui manque (gravité : modérée)
- **Cycle de vie** : Comment Electron démarre-t-il le backend ? Process.spawn ? Service intégré ?
- **Gestion des crashes** : Que se passe-t-il si le backend crash ? L'UI doit-elle afficher un écran d'erreur ?
- **Port conflict** : Que se passe-t-il si le port 5100 est déjà utilisé ?
- **Arrêt propre** : Comment le backend est-il notifié de l'arrêt de l'application ?
- **Health check** : L'UI doit-elle attendre que le backend soit prêt ?

**Recommandation :** Ajouter un document "Cycle de vie Electron ↔ .NET" avec :
1. Démarrage : Electron spawn le processus .NET, attend le health check
2. Arrêt : Electron envoie SIGTERM, attend la grâce
3. Crash : Electron affiche un écran d'erreur avec bouton "Redémarrer"
4. Port conflict : Détection + fallback sur port suivant

---

## 3. Persistance — Choix correct

SQLite via EF Core est le bon choix pour V1 :
- Zero-config ✅
- Mono-utilisateur ✅
- EF Core compatible ✅
- Portable ✅

Le seul risque : les performances pour les requêtes géographiques. Mais en V1, le volume sera gérable (< 100k observations).

---

## 4. API — Trop d'endpoints pour V1

### Problème
30+ endpoints API pour un MVP, c'est excessif. Beaucoup sont des CRUD standard qui pourraient être simplifiés ou regroupés.

### Recommandation : réduire à 15-20 endpoints V1

| Endpoints V1 essentiels | Count |
|---|---|
| Observations (GET list, GET by id) | 2 |
| EventCases (GET list, GET by id, POST create, PATCH update) | 4 |
| Connectors (GET list, GET status, POST configure) | 3 |
| Jobs (GET list, POST trigger, GET status) | 3 |
| Scoring (GET score breakdown) | 1 |
| Feedback (POST validate, POST invalidate, POST correct) | 3 |
| Search (GET search) | 1 |
| Export (GET export markdown, GET export json) | 2 |
| Dashboard (GET KPIs, GET contradictions) | 2 |
| Health (GET health) | 1 |
| **Total** | **~22** |

Les endpoints CRUD pour Entity, Location, AuditEntry, Watchlist, ReportExport, MediaAsset, Claim, Evidence sont repoussés en V2 ou accessibles via les endpoints principaux.

---

## 5. Stratégie de plugin/connecteurs — Correcte mais premature

L'interface `ISourceConnector` est bien conçue. Mais pour V1 avec seulement 2 connecteurs, un système de plugin complet est sur-ingénierie.

**Recommandation V1 :**
- Interface `ISourceConnector` dans Domain ✅ (garder l'abstraction)
- Tous les connecteurs dans un seul projet `AegisLoop.Connectors`
- Enregistrement par convention au démarrage ✅ (garder ce mécanisme)
- Pas de système de chargement dynamique en V1

**V2 :** Séparer les connecteurs en projets individuels quand ils deviennent complexes.

---

## 6. Stratégie de configuration — Manquante

Le corpus ne spécifie pas clairement comment la configuration est gérée :
- Où sont stockées les clés API ?
- Quel est le format de configuration ?
- Comment l'analyste configure-t-il les connecteurs ?
- Quelle est la hiérarchie de configuration (default → user → env) ?

**Recommandation :** Ajouter une section sur la stratégie de configuration :
- `appsettings.json` pour les valeurs par défaut
- Fichier utilisateur `%APPDATA%/AegisLoop/config.json` pour les overrides
- Variables d'environnement pour les secrets (clés API)
- UI de configuration dans Paramètres

---

## 7. Sécurité — Correcte pour V1

L'ADR 0007 est bien articulé :
- API localhost uniquement ✅
- CORS restreint ✅
- Clés API chiffrées ✅
- Audit trail ✅

Le seul point faible : le chiffrement des clés API est spécifié par OS (DPAPI/Keychain/Secret Service) mais l'implémentation cross-platform sera complexe.

**Recommandation :** En V1, utiliser un fichier chiffré simple avec AES. La gestion par OS est un Should, pas un Must.

---

## 8. CI/CD — Réaliste

GitHub Actions pour build, test, release est adapté à un porteur solo. Pas de sur-ingénierie ici.

---

## 9. Résumé des recommandations architecture

| Action | Gravité | Urgence | Impact |
|---|---|---|---|
| Réduire à 6-7 projets C# | Critique | Avant implémentation | Architecture/Planning |
| Réduire les endpoints API à 15-20 | Importante | Pendant implémentation | Architecture |
| Ajouter le cycle de vie Electron ↔ .NET | Modérée | Avant implémentation | Architecture |
| Ajouter la stratégie de configuration | Modérée | Avant implémentation | Architecture |
| Simplifier le chiffrement des clés API en V1 | Mineure | Pendant implémentation | Sécurité |
| Garder l'interface ISourceConnector mais un seul projet connecteurs | Importante | Avant implémentation | Architecture |