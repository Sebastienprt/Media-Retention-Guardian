# Media Retention Guardian

## Présentation
Media Retention Guardian est un plugin Jellyfin qui automatise la suppression des fichiers multimédias dépassant une durée de conservation définie. Pensé pour les bibliothèques temporaires (replays TV, enregistrements, téléchargements ponctuels), il évite l'accumulation de contenus obsolètes, libère de l'espace disque et garantit que seuls les fichiers récents restent accessibles aux utilisateurs.

## Installation via dépôt Jellyfin (recommandée)
1. Connectez-vous avec un compte administrateur puis ouvrez `Tableau de bord` > `Plugins` > `Dépôts`.
2. Cliquez sur `Ajouter un dépôt` et saisissez :
   - **Nom** : `Media Retention Guardian` (ou tout autre libellé parlant).
   - **URL** : `https://github.com/Sebastienprt/Media-Retention-Guardian/releases/latest/download/manifest.json`.
3. Validez, revenez à l'onglet `Catalogue`, puis utilisez la recherche pour trouver **Media Retention Guardian**.
4. Lancez l'installation et redémarrez Jellyfin si l'interface vous y invite.

Grâce à ce dépôt, Jellyfin vérifiera automatiquement les nouvelles versions publiées sur GitHub et proposera la mise à jour du plugin.

## Installation locale via dotnet (alternative)
Cette méthode s'adresse aux personnes qui souhaitent construire le plugin depuis les sources en utilisant le SDK .NET.

1. Prérequis : .NET SDK 8.0 et Git.
2. Cloner le dépôt et préparer le package :
   ```bash
   git clone https://github.com/Sebastienprt/Media-Retention-Guardian.git
   cd Media-Retention-Guardian
   ./scripts/package.sh 0.1.0   # remplacez par la version souhaitée
   ```
   La commande produit un fichier `MediaRetentionGuardian_vX.Y.Z.zip` à la racine du dépôt.
3. Sur le serveur Jellyfin, décompressez l'archive dans le dossier `plugins/MediaRetentionGuardian` du data directory (exemple : `~/.local/share/jellyfin/plugins/MediaRetentionGuardian`).
4. Redémarrez Jellyfin pour charger la nouvelle version.

## Fonctionnalités principales
- Suppression automatique des fichiers plus anciens qu'un nombre de jours configurable par dossier.
- Gestion de plusieurs dossiers cibles avec des paramètres indépendants.
- Déclenchement conditionnel sur seuil d'espace disque libre pour protéger les supports contraints.
- Journalisation détaillée des suppressions dans le planificateur Jellyfin pour assurer la traçabilité.
- Interface de configuration intégrée à Jellyfin (`Panneau d'administration` > `Plugins` > `Media Retention Guardian`).

## Configuration du plugin
1. `Tableau de bord Jellyfin` > `Plugins` > `Media Retention Guardian` > `Configuration`.
2. Activez la purge automatique puis ajoutez un ou plusieurs dossiers cibles :
   - **Chemin** : dossier surveillé.
   - **Durée de conservation (jours)** : ancienneté maximale des fichiers.
   - **Seuil d'espace libre (%)** *(facultatif)* : purge déclenchée uniquement si l'espace libre devient inférieur au pourcentage indiqué.
3. Sauvegardez. La tâche planifiée s'exécute chaque nuit à 03h00 ; vous pouvez la déclencher manuellement via `Tableau de bord` > `Planificateur` > `Media Retention Guardian`.

## Dépannage
- Consultez les journaux via `Tableau de bord` > `Planificateur` > `Historique` pour vérifier les suppressions.
- En cas d'erreur de permission, assurez-vous que l'utilisateur système Jellyfin possède les droits en lecture/écriture sur les dossiers ciblés.
- Si la purge ne se déclenche pas, vérifiez que le seuil d'espace libre correspond bien à un pourcentage d'espace libre et non d'espace utilisé.

## Contribution
Les contributions et retours sont les bienvenus via issues ou Pull Requests sur GitHub.
