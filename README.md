# Media Retention Guardian

## Présentation
Media Retention Guardian est un plugin Jellyfin qui automatise la suppression des fichiers multimédias dépassant une durée de conservation définie. Idéal pour les bibliothèques temporaires (replays TV, enregistrements, téléchargements ponctuels), il évite l'accumulation de contenus obsolètes, libère de l'espace disque et garantit que seuls les fichiers récents restent accessibles aux utilisateurs.

## Fonctionnalités principales
- Suppression automatique des fichiers plus anciens qu'un nombre de jours configurable par dossier.
- Gestion de plusieurs dossiers cibles avec des paramètres indépendants.
- Déclenchement conditionnel sur seuil d'espace disque libre pour protéger les supports contraints.
- Journalisation détaillée des suppressions dans le planificateur Jellyfin pour assurer la traçabilité.
- Interface de configuration intégrée à Jellyfin (panneau d'administration > Plugins > Media Retention Guardian).

## Prérequis
- Jellyfin 10.10.x ou version ultérieure (ABI ciblé : `10.10.0.0`).
- Accès administrateur au serveur Jellyfin.
- .NET SDK 8.0 uniquement si vous souhaitez compiler le plugin vous-même.

## Dépôt Git
Le code source est maintenu dans le dépôt GitHub suivant :
- https://github.com/Sebastienprt/server-cleaner

## Ajouter la source du plugin dans Jellyfin
### 1. Préparer les fichiers publiés
- Générez l'archive à partir du dossier `publish` :
  ```bash
  dotnet publish -c Release Jellyfin.Plugin.MediaRetentionGuardian/MediaRetentionGuardian.csproj
  cd Jellyfin.Plugin.MediaRetentionGuardian/bin/Release/net8.0/publish
  python3 -m zipfile -c ../../../../MediaRetentionGuardian_v0.1.0.zip ./*
  ```
- Calculez la somme : `sha256sum MediaRetentionGuardian_v0.1.0.zip` (exemple : `sha256:854d53e4cb62583147521e6df408d84bd8a19794407d529c11e4c021972be535`).
- Placez `MediaRetentionGuardian_v0.1.0.zip`, `thumb.png` et `manifest.json` sur un hébergement HTTP/HTTPS. Dans le manifest, adaptez `sourceUrl` et `thumb` vers les URL publiques, par exemple :
  ```json
  {
    "sourceUrl": "https://mon-domaine/plugins/MediaRetentionGuardian_v0.1.0.zip",
    "thumb": "https://mon-domaine/plugins/thumb.png"
  }
  ```

### 2. Déclarer le dépôt dans Jellyfin
1. Interface administrateur Jellyfin > `Tableau de bord` > `Plugins` > `Dépôts`.
2. Cliquez sur `Ajouter un dépôt` et indiquez :
   - **Nom** : `Media Retention Guardian` (ou tout libellé parlant).
   - **URL** : l'URL directe de votre `manifest.json`, par exemple `https://mon-domaine/plugins/manifest.json`.
3. Validez puis revenez à l'onglet `Catalogue`.
4. Rafraîchissez la liste, recherchez **Media Retention Guardian** et lancez l'installation.
5. Redémarrez Jellyfin si l'interface le demande pour charger le plugin.

### Installation manuelle (alternative)
1. Téléchargez l'archive `MediaRetentionGuardian_vX.Y.Z.zip` depuis la section "Assets" d'une release GitHub.
2. Décompressez le contenu dans `plugins/MediaRetentionGuardian` au sein du data directory Jellyfin (ex. `~/.local/share/jellyfin/plugins/MediaRetentionGuardian`).
3. Redémarrez Jellyfin.

## Configuration du plugin
1. Tableau de bord Jellyfin > `Plugins` > `Media Retention Guardian` > `Configuration`.
2. Activez la purge automatique puis ajoutez un ou plusieurs dossiers cibles :
   - **Chemin** : dossier surveillé.
   - **Durée de conservation (jours)** : ancienneté maximale des fichiers.
   - **Seuil d'espace libre (%)** *(facultatif)* : purge déclenchée seulement si l'espace libre est inférieur à ce pourcentage.
3. Sauvegardez. La tâche planifiée s'exécute chaque nuit à 03h00. Déclenchez-la manuellement via `Tableau de bord` > `Planificateur` > `Media Retention Guardian` si nécessaire.

## Workflow Git pour préparer une nouvelle version
1. **Cloner et créer une branche de travail**
   ```bash
   git clone https://github.com/Sebastienprt/server-cleaner.git
   cd server-cleaner
   git checkout -b feature/ma-modif
   ```
2. **Développer et valider**
   - Implémentez vos changements dans `Jellyfin.Plugin.MediaRetentionGuardian`.
   - Exécutez `dotnet build Jellyfin.Plugin.MediaRetentionGuardian/MediaRetentionGuardian.csproj` pour vérifier la compilation.
3. **Mettre à jour la version**
   - Incrémentez la version dans `build.yaml` (`version:`) et `Directory.Build.props` (`<Version>`, `<AssemblyVersion>`, `<FileVersion>`).
   - Actualisez le `changelog` de `build.yaml` si nécessaire.
4. **Commiter et fusionner**
   ```bash
   git commit -am "Prépare la version vX.Y.Z"
   git push origin feature/ma-modif
   ```
   - Ouvrez une Pull Request et validez qu'elle passe les workflows GitHub Actions (`🏗️ Build Plugin`).
5. **Publier une release**
   - Une fois la branche fusionnée sur `main`, créez un tag annoté :
     ```bash
     git checkout main
     git pull
     git tag -a vX.Y.Z -m "Media Retention Guardian vX.Y.Z"
     git push origin vX.Y.Z
     ```
   - Publiez la release GitHub en y ajoutant l'archive construite (`MediaRetentionGuardian_vX.Y.Z.zip`).
6. **Mettre à jour la source Jellyfin**
   - Le workflow `🚀 Publish Plugin` ou votre procédure de déploiement copie le manifest et l'archive vers l'hébergement.
   - Vérifiez que l'URL du manifest est à jour dans Jellyfin si le nom de fichier change.

## Dépannage
- Les logs sont accessibles via `Tableau de bord` > `Planificateur` > `Historique`.
- En cas d'erreur de permission, assurez-vous que l'utilisateur système Jellyfin a les droits en lecture/écriture sur les dossiers ciblés.
- Si le seuil de disque n'est jamais atteint, vérifiez que la valeur correspond bien au pourcentage d'espace libre (et non utilisé).

Contribution et retours bienvenus via issues ou Pull Requests sur le dépôt GitHub.
