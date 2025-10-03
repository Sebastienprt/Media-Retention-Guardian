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

## Workflow Git pour préparer une version installable via une source Jellyfin
1. **Cloner et créer une branche de travail**
   ```bash
   git clone https://github.com/<votre-organisation>/media-retention-guardian.git
   cd media-retention-guardian
   git checkout -b feature/ma-modif
   ```
2. **Développer et valider**
   - Implémentez vos changements dans `Jellyfin.Plugin.MediaRetentionGuardian`.
   - Exécutez `dotnet build Jellyfin.Plugin.MediaRetentionGuardian/MediaRetentionGuardian.csproj` pour valider la compilation.
3. **Mettre à jour la version**
   - Incrémentez la version dans `build.yaml` (`version:`) et `Directory.Build.props` (`<Version>`, `<AssemblyVersion>`, `<FileVersion>`).
   - Ajustez le changelog dans `build.yaml` si besoin.
4. **Commiter et fusionner**
   ```bash
   git commit -am "Prépare la version vX.Y.Z"
   git push origin feature/ma-modif
   ```
   - Ouvrez une Pull Request et validez qu'elle passe les workflows GitHub Actions (`🏗️ Build Plugin`).
5. **Créer un tag et une release**
   - Une fois fusionné sur `master`, créez un tag annoté :
     ```bash
     git checkout master
     git pull
     git tag -a vX.Y.Z -m "Media Retention Guardian vX.Y.Z"
     git push origin vX.Y.Z
     ```
   - Publiez une release GitHub en attachant l'archive générée par le workflow (fichier `MediaRetentionGuardian_vX.Y.Z.zip`).
6. **Déployer le manifest**
   - Le workflow `🚀 Publish Plugin` se charge de copier le manifest et les artefacts vers l'hôte défini par vos secrets (`DEPLOY_HOST`).
   - L'URL finale du manifest prend généralement la forme `https://<votre-hôte>/media-retention-guardian/manifest.json`. Notez-la : elle sera nécessaire pour l'installation côté Jellyfin.

## Installation via une source personnalisée dans Jellyfin
1. Connectez-vous à l'interface administrateur de Jellyfin.
2. Ouvrez `Tableau de bord` > `Plugins` > `Dépôts`.
3. Cliquez sur `Ajouter un dépôt` et renseignez :
   - **Nom** : `Media Retention Guardian`
   - **URL** : l'URL du manifest publiée par votre workflow, ex. `https://<votre-hôte>/media-retention-guardian/manifest.json`
4. Validez, puis allez dans l'onglet `Catalogue` des plugins.
5. Recherchez "Media Retention Guardian" et installez-le depuis la nouvelle source.
6. Redémarrez le serveur Jellyfin si l'interface vous le demande.

### Installation manuelle (alternative)
Si vous ne disposez pas encore d'une source publiée :
1. Téléchargez l'archive `MediaRetentionGuardian_vX.Y.Z.zip` depuis la section "Assets" d'une release GitHub.
2. Décompressez le contenu dans le dossier `plugins/MediaRetentionGuardian` de votre data directory Jellyfin (par exemple `~/.local/share/jellyfin/plugins/MediaRetentionGuardian`).
3. Redémarrez Jellyfin pour que le plugin soit chargé.

## Configuration du plugin
1. Dans Jellyfin, ouvrez `Tableau de bord` > `Plugins` > `Media Retention Guardian` > `Configuration`.
2. Activez la purge automatique puis ajoutez un ou plusieurs dossiers cibles :
   - **Chemin** : dossier surveillé.
   - **Durée de conservation (jours)** : ancienneté maximale des fichiers.
   - **Seuil d'espace libre (%)** *(facultatif)* : purge déclenchée seulement si l'espace libre est inférieur à ce pourcentage.
3. Sauvegardez. La tâche planifiée s'exécute chaque nuit à 03h00. Vous pouvez déclencher une exécution manuelle via `Tableau de bord` > `Planificateur` > `Media Retention Guardian`.

## Dépannage
- Les logs de la tâche sont disponibles dans `Tableau de bord` > `Planificateur` > `Historique`. Ils détaillent le nombre de fichiers supprimés par cible.
- En cas d'erreur de permission, vérifiez que l'utilisateur système de Jellyfin possède les droits en lecture/écriture sur les dossiers surveillés.
- Si le seuil de disque est activé mais jamais déclenché, confirmez que la valeur correspond au pourcentage d'espace libre restants (et non utilisé).

Pour toute contribution ou problème, ouvrez une issue ou une Pull Request dans le dépôt Git.
