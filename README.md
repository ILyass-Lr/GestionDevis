# Application Gestion des Devis

## Architecture d'application
### Séparation des préoccupation:
- Des classes modèles (Client, Facture, Devis) chacune représentant une table.
- Des classes *Repositories* pour gérer les requêtes SQL à la base de données.
- Des class *WinForms* utilisant *MetroFramework*.
### Struture de la base de données
- *Service-Base Database*

### Struture du projet
- Dossier `Client` *Class Library* (.NET Framework) contenant un modèle client et son *Repository*, référencié par `GestionClient` et `GestionClientFactures`
- Dossier `GestionClient` sert du projet principale, contient la base de données et le rapport *CrystalReport.rpt* et *ProductData* DataSet, ainsi que des modèles (`Devis` et `Facture`) et leur `Reposiories`, et contient la solution ***.sln***
- Dossier `GestionClient` sert d'interface *WinForms* pour l'ajout d'un client, il référencie la base de données contenu dans `GestionClientFactures` 
- Dossier `Imprimante` contient le projet *WinForms* pour l'affichage de l'interface de l'imprimante

## Exemples
### Validation des champs

### Excel
- Des Clients + Statistiques

- Des Devis

### Filtrage
- Des clients par un champ sélectionné

- Des produits par leur numéro de devis

### Insertio et Mise à jour Client

### Insertion et mise à jour Produit

### Enregistrement d'une commande

### Suppression d'un produit

### Rapport PDF + Imprimante

# Setup
- Lancer la solution  ***.sln***  contenu dans `GestionClientFactures`
- Installer CrystalReport à partie de site officiel ainsi que son RunTime (assurer la compatibilité)
- Changer la chaîne de connextion de la base de données
## Dépendances
- MetroFramework 1.2.0.3 pour `GestionClientFactures` et `GestionClient`
- ClosedXML 0.105.0 `GestionClientFactures` et `GestionClient`
- SAP CrystalReport







