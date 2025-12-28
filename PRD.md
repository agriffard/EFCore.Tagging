# PRD – EF Core TagWith Helper

## 1. Overview
Nom du produit : EF Core TagWith Helper  
Type : Librairie .NET (NuGet)  
Cible : EF Core / ASP.NET Core / SQL Server (compatible autres providers EF Core)  

Objectif : Fournir une API simple, structurée et standardisée pour appliquer automatiquement des `TagWith` EF Core aux requêtes afin d’améliorer le debugging, l’observabilité et l’analyse des performances SQL.

---

## 2. Problème
- Les requêtes SQL générées par EF Core sont difficiles à tracer
- Impossible d’identifier rapidement l’origine d’une requête en production
- `TagWith()` est peu utilisé, appliqué manuellement et de façon incohérente
- Les équipes manquent de visibilité dans les logs SQL et outils APM

---

## 3. Objectifs
- Standardiser l’usage de `TagWith()` dans toute l’application
- Ajouter automatiquement du contexte métier et technique aux requêtes
- Faciliter le diagnostic des lenteurs et bugs SQL
- S’intégrer sans modifier le code métier existant
- Compatible avec logging, APM et outils SQL Server

---

## 4. Concepts clés

### Tag de requête
Un tag est une chaîne descriptive ajoutée à une requête SQL EF Core.

Exemples :
- Nom du cas d’usage
- Endpoint HTTP
- Utilisateur ou rôle
- Feature ou module

### Modèle de tag
```csharp
public class QueryTag
{
    public string Name { get; set; }
    public Dictionary<string, string> Metadata { get; set; }
}
```

---

## 5. API publique

### Extension IQueryable
```csharp
query = query.TagWithContext("Products", new {
    Feature = "Catalog",
    User = userId,
    Action = "List"
});
```

### Tag automatique par scope
```csharp
using (tagScope.Begin("Orders", "GetById"))
{
    var order = await db.Orders.FirstAsync(o => o.Id == id);
}
```

### Middleware ASP.NET Core
- Ajoute automatiquement :
  - Nom du contrôleur / action
  - Méthode HTTP
  - CorrelationId

---

## 6. Intégration EF Core
- Utilise `IQueryable.TagWith()` et `TagWithCallSite()`
- Compatible LINQ, projections et Include
- Fonctionne avec requêtes compilées

---

## 7. Cas d’usage
- Analyse SQL Server Profiler
- Debugging production
- Corrélation requêtes API → SQL
- Optimisation des performances EF Core

---

## 8. Performance
- Zéro impact sur le plan d’exécution SQL
- Ajout minimal de texte dans la requête
- Désactivable par environnement (Prod/Dev)

---

## 9. Sécurité
- Filtrage des données sensibles dans les tags
- Configuration whitelist des champs autorisés
- Désactivation automatique en production si requis

---

## 10. Configuration
```json
{
  "EfTagging": {
    "Enabled": true,
    "IncludeUser": true,
    "IncludeEndpoint": true
  }
}
```

---

## 11. Livrables
- Package NuGet `EFCore.Tagging`
- README avec exemples et bonnes pratiques
- Tests unitaires
- Exemple ASP.NET Core avec logging SQL

---

## 12. Évolutions futures
- Intégration OpenTelemetry
- Tags dynamiques basés sur règles
- UI de visualisation des requêtes taguées
- Support multi-tenant et feature flags