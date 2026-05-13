# Analyse des connecteurs OSINT

> **Date :** 2026-04-23  
> **Axe d'audit :** C — Sources OSINT et connecteurs  
> **Gravité :** Importante

---

## Verdict : Stratégie connecteurs bien articulée, sources MVP bien choisies

La stratégie connecteurs est l'un des points les plus solides du corpus. Les sources MVP (RSS + GDELT) sont réalistes, lawful et démontrables. Les risques juridiques sont documentés. La distinction V1/futur/exclu est nette. Cependant, certains connecteurs Should méritent d'être repoussés.

Score : **7.5/10** — Bon.

---

## 1. Évaluation par connecteur

### Connecteurs V1 Must

| Connecteur | Accessibilité | Stabilité | Légalité | Complexité | Valeur démo | Verdict |
|---|---|---|---|---|---|---|
| **RSS/Atom** | ✅ Publique | ✅ Stable | ✅ Lawful | ✅ Faible | ✅ Forte | **V1 — Must** |
| **GDELT** | ✅ API publique | ✅ Stable | ✅ Lawful | ⚠️ Modérée | ✅ Forte | **V1 — Must** |

**RSS/Atom** — Choix excellent. Format universel, pas de quota, pas de clé API, légalement exploitable. Les flux de médias d'information et sites institutionnels sont légitimes et stables.

**GDELT** — Choix excellent. API publique documentée, données géolocalisées, volume massif, pas de clé API nécessaire pour l'usage basique. Le seul risque est le volume (filtrage nécessaire).

### Connecteurs V1 Should

| Connecteur | Accessibilité | Stabilité | Légalité | Complexité | Valeur démo | Verdict |
|---|---|---|---|---|---|---|
| **YouTube Data API** | ⚠️ Clé API | ⚠️ Quotas | ⚠️ CGU strictes | ⚠️ Modérée | ✅ Forte | **Repousser V2** |
| **STAC/Copernicus** | ✅ Public | ✅ Stable | ✅ Ouvert | ⚠️ Modérée | ⚠️ Moyenne | **Repousser V2** |
| **Import manuel** | ✅ Local | ✅ Stable | ✅ Aucun | ⚠️ Modérée | ⚠️ Faible | **Repousser V2** |

**YouTube Data API** — Pertinent mais :
- Nécessite une clé API Google Cloud
- Quotas limités (10 000 unités/jour)
- CGU strictes (pas de scraping, pas de stockage de données vidéo)
- La valeur démo est forte (métadonnées vidéo d'événements) MAIS le coût de setup + gestion des quotas + risque de blocage n'est pas justifié pour un MVP solo
- **Recommandation : V2.** Ajouter une section dans les specs : "YouTube est pré-architecté (interface ISourceConnector) mais l'implémentation est repoussée"

**STAC/Copernicus** — Intéressant pour le scénario géospatial mais :
- L'API STAC est diverse (chaque catalogue a sa propre implémentation)
- Les métadonnées Sentinel sont riches mais le mapping est complexe
- La valeur démo est limitée sans les images elles-mêmes
- **Recommandation : V2.** Le scénario géopolitique et maritime fonctionne très bien avec RSS + GDELT seul.

**Import manuel** — Utile pour les cas d'usage spécifiques mais :
- Pas nécessaire pour la démo (seed data suffit)
- Multi-format (CSV, JSON, GeoJSON, KML) = beaucoup de parsers
- **Recommandation : V2.** Accepter uniquement CSV en V2, pas 4 formats d'un coup.

### Connecteurs futurs (Could)

| Connecteur | Verdict | Raison |
|---|---|---|
| **AIS maritime** | ⚠️ V2 si source publique stable | MarineTraffic API est payante, AIS Hub instable |
| **Météo** | ✅ V2 | Open-Meteo API gratuite, valeur ajoutée réelle |
| **Twitter/X** | ❌ Non recommandé | API payante, CGU restrictives, instabilité |
| **Temps réel** | ⚠️ V3 | Trop complexe pour MVP |
| **Reddit** | ❌ Non recommandé | API restrictive, CGU, données non fiables |

---

## 2. Risques par connecteur

| Risque | Connecteur | Probabilité | Impact | Mitigation |
|---|---|---|---|---|
| Volume GDELT trop important | GDELT | Faible | Performance | Filtrage par pays/thème, pagination |
| Quota YouTube dépassé | YouTube | Moyenne | Collecte interrompue | Cache, rate limiting, alerte quota |
| Flux RSS modifie son format | RSS | Faible | Parsing en erreur | Parser tolérant, log d'erreur |
| API GDELT change | GDELT | Faible | Breaking change | Versionnage de l'API, tests de contrat |
| Source institutionnelle bloque | RSS | Faible | Données manquantes | Mode démo, autres flux |

---

## 3. Rate limiting concret — Manquant

Le corpus ne spécifie pas les limites exactes par connecteur. C'est un trou de spécification.

**Recommandation :** Ajouter les limites concrètes :

| Connecteur | Limite | Configuration |
|---|---|---|
| RSS/Atom | 1 requête par flux toutes les 15 min | `PollingIntervalMinutes` |
| GDELT API | 300 requêtes/min (limite API) | `MaxRequestsPerMinute` = 60 (marge) |
| YouTube | 10 000 unités API/jour | V2 |
| Nominatim (géocodage) | 1 requête/sec | `GeocodingDelayMs` = 1100 |

---

## 4. Stratégie de retry — Bien mais à concrétiser

Le corpus mentionne une stratégie de retry mais ne la détaille pas assez.

**Recommandation V1 :**
- Retry exponentiel : 1s, 2s, 4s, 8s, 16s (max 5 tentatives)
- Circuit breaker : après 3 échecs consécutifs, désactiver le connecteur pendant 5 min
- Log chaque tentative
- Notifier l'UI via SSE

---

## 5. Connecteurs recommandés pour V1

**V1 :** RSS/Atom + GDELT uniquement. C'est suffisant pour démontrer :
- Fusion multi-source (RSS = texte riche, GDELT = données structurées + géolocalisation)
- Scoring différencié (RSS fiable vs GDELT brut)
- Provenance distincte par source
- Contradictions entre sources

**V2 :** YouTube + STAC + Import CSV  
**V3 :** AIS maritime + Météo + plugins dynamiques

---

## 6. Résumé

| Action | Gravité | Urgence |
|---|---|---|
| Verrouiller RSS + GDELT uniquement en V1 | Importante | Avant implémentation |
| Repousser YouTube, STAC, Import en V2 | Importante | Avant implémentation |
| Ajouter les limites de rate limiting concrètes | Modérée | Avant implémentation |
| Détailler la stratégie de retry | Modérée | Avant implémentation |
| Exclure Twitter/X et Reddit | Mineure | Documentation |