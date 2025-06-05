# Application Gestion des Devis

## Architecture d'application
### Séparation des préoccupations:
- Des classes modèles (Client, Facture, Devis) chacune représentant une table.
- Des classes *Repositories* pour gérer les requêtes SQL à la base de données.
- Des classes *WinForms* utilisant *MetroFramework*.
### Struture de la base de données
- *Service-Base Database*
  ![DB](https://github.com/user-attachments/assets/dd8ebcc4-ed37-4549-ab88-57e32cc99862)
### Struture du projet
- Dossier `Client` *Class Library* (.NET Framework) contenant un modèle client et son *Repository*, référencié par `GestionClient` et `GestionClientFactures`
- Dossier `GestionClient` sert du projet principale, contient la base de données et le rapport *CrystalReport.rpt* et *ProductData* DataSet, ainsi que des modèles (`Devis` et `Facture`) et leur `Reposiories`, et contient la solution ***.sln***
- Dossier `GestionClient` sert d'interface *WinForms* pour l'ajout d'un client, il référencie la base de données contenu dans `GestionClientFactures` 
- Dossier `Imprimante` contient le projet *WinForms* pour l'affichage de l'interface de l'imprimante

## Exemples
### Validation des champs
https://github.com/user-attachments/assets/8b8be780-9b7d-405e-88fd-d80d62883396
### Excel
- Des Clients + Statistiques
  
https://github.com/user-attachments/assets/ccc2e359-35bc-41e4-8f21-4e61c893e09a
- Des Devis
  
https://github.com/user-attachments/assets/7ece1f86-6834-44aa-b5be-831dd196fe70
### Filtrage
- Des clients par un champ sélectionné
  
https://github.com/user-attachments/assets/211b27d0-0fa5-46d1-920d-93bbffe86ff5
- Des produits par leur numéro de devis
  
https://github.com/user-attachments/assets/bf48512e-c22d-4370-ba4d-7c4f03519389
### Insertion et Mise à jour Client

https://github.com/user-attachments/assets/676f41dc-28f8-466f-9ee1-56ae0a3f6031

https://github.com/user-attachments/assets/d942af1c-7242-418f-aaca-0e5a29719852
### Insertion et mise à jour Produit
https://github.com/user-attachments/assets/d36317b7-af9f-4e33-9b6e-07484759e164
### Enregistrement d'une commande
https://github.com/user-attachments/assets/01cf51b4-fe33-423e-a25d-f1b30fd3d9e3
### Suppression d'un produit
https://github.com/user-attachments/assets/e0e3b0d0-9f28-460a-abdf-6950a93f4272
### Rapport PDF + Imprimante

https://github.com/user-attachments/assets/5a66ec35-bdc3-47d2-be90-b65fd6cce7cc

https://github.com/user-attachments/assets/5b4875ca-e277-4736-b445-0cb90d922ff1
# Setup
- Lancer la solution  ***.sln***  contenu dans `GestionClientFactures`
- Installer CrystalReport à partie de site officiel ainsi que son RunTime (assurer la compatibilité)
- Changer la chaîne de connextion de la base de données
## Dépendances
- MetroFramework 1.2.0.3 pour `GestionClientFactures` et `GestionClient`
- ClosedXML 0.105.0 `GestionClientFactures` et `GestionClient`
- SAP CrystalReport







