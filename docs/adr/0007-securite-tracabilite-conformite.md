# ADR 0007 — Sécurité, traçabilité et conformité

> **Statut :** Accepté  
> **Date :** 2026-04-23  
> **Décideur :** Architecte

---

## Contexte

Aegis Loop manipule des données OSINT potentiellement sensibles, des clés API, et des analyses de renseignement. Le système doit être lawful, éthique, traçable, audit-friendly et privacy-aware par conception.

## Décision

Adopter une stratégie **Security & Compliance by Design** avec les mesures suivantes :

### Sécurité locale
- **API localhost uniquement** — Bindé sur 127.0.0.1, pas d'exposition réseau
- **CORS restreint** — Origine Electron uniquement
- **Clés API chiffrées** — AES en V1 (simplifié), OS-native (DPAPI/Keychain) en V2
- **Validation des entrées** — Backend ET frontend
- **Protection XSS** — React échappe par défaut + CSP headers
- **Protection injection SQL** — EF Core paramétré, pas de SQL brut

### Traçabilité
- **Audit trail append-only** — Chaque action génère une entrée AuditEntry immutable
- **Provenance complète** — Chaque Observation trace sa chaîne source → collecte → normalisation → enrichissement
- **Feedback journalisé** — Chaque action analyste est tracée avec horodatage et détail
- **Export tracé** — Chaque export est journalisé dans l'audit trail avec horodatage (pas de type ReportExport en V1 — DTO Application)

### Conformité
- **Lawful by design** — Aucune collecte contraire au droit ou aux CGU
- **Privacy-aware** — Pas de stockage de données personnelles non nécessaires
- **Humain dans la boucle** — L'analyste valide, corrige, décide
- **Pas de scraping agressif** — Respect des rate limits et CGU
- **Avertissement à l'export** — Alerte si données sensibles détectées
- **Mode démo séparé** — Données de démo clairement marquées et séparées

## Alternatives considérées

| Alternative | Avantages | Inconvénients |
|---|---|---|
| **Chiffrement complet de la base** | Sécurité maximale | Performance dégradée, complexité de gestion des clés |
| **Authentification locale** | Contrôle d'accès | Sur-ingénierie pour mono-utilisateur desktop |
| **Audit par event sourcing** | Replay complet | Complexité élevée, sur-ingénierie pour MVP |
| **Anonymisation automatique** | Privacy maximale | Trop de faux positifs, perte d'information utile |

## Justification

1. **Contexte défense/sécurité** — La traçabilité et l'auditabilité sont des exigences non négociables
2. **Porteur solo** — Pas de complexité d'authentification multi-utilisateur
3. **Desktop local** — La menace réseau est limitée (localhost uniquement)
4. **Conformité RGPD** — Pas de données personnelles stockées sans nécessité
5. **Démonstrabilité** — L'audit trail est un atout pour les présentations
6. **Réputation** — L'approche éthique est un différenciateur

## Conséquences

- Surcoût de développement pour l'audit trail (~10%)
- Les clés API V1 sont chiffrées en AES local (pas de dépendance OS-native en V1)
- Pas de chiffrement de la base complète en V1 (compromis accepté)
- L'audit trail ne doit pas impacter la performance (> 10% de surcoût max)

## Risques résiduels

| Risque | Mitigation |
|---|---|
| Accès physique à la machine | Chiffrement OS (BitLocker/FileVault) recommandé |
| Données personnelles dans les sources OSINT | Filtrage à l'ingestion, avertissement à l'export |
| Export de données sensibles | Avertissement, options de masquage |
| Clés API en clair si DPAPI non disponible | Fallback vers chiffrement AES local |

## Références

- [02-architecture-technique.md](../specs/02-architecture-technique.md) section 14
- [00-expression-besoins.md](../specs/00-expression-besoins.md) section 7.3
- [07-glossaire-et-decisions.md](../specs/07-glossaire-et-decisions.md) section 7