# Préparation Workshop — Work Rework 1.0.1

Le correctif **1.0.1**, validé localement par l'utilisateur, répare les noms de catégories et sous-tâches dans l'éditeur et les infobulles. Sa version et son tag sont **1.0.1**. Le créateur a mis à jour l'article existant ; téléchargement 1.0.1 vérifié : version, identifiant et onze fichiers SHA256 identiques à la livraison. Visibilité privée toujours demandée ; aucun réglage Steam modifié par l'agent.

## État de la page

Premier envoi effectué manuellement par le créateur : [Work Rework](https://steamcommunity.com/sharedfiles/filedetails/?id=3826051737), identifiant **3826051737**. Visibilité Masquée (privée) vérifiée dans la session du propriétaire. Descriptions et note de première publication anglaises et françaises validées enregistrées dans leurs langues respectives, sans référence au dépôt ; Harmony déclaré comme seul article requis. Grouped Pawns Lists reste optionnel. Téléchargement vérifié : les onze fichiers correspondent à l'installation envoyée. Ne pas passer en public sans nouvelle instruction.

`About/PublishedFileId.txt` est conservé dans les sources et les paquets : toujours mettre à jour cet article, ne pas créer un second envoi.

## Éléments prêts à relire

- Couverture : `../About/Preview.png`, PNG carré 640 × 640, 816 832 octets. L'original généré est conservé dans `assets/preview-master.png`. L'icône personnelle `../About/ModIcon.png` n'a pas été remplacée.
- Titre, résumés, dépendances et identifiants : `listing.json`.
- Texte anglais prêt à copier : `description.en.bbcode.txt`.
- Texte français prêt à copier : `description.fr.bbcode.txt`.
- Notes de mise à jour 1.0.1 : `changenote.en.txt` et `changenote.fr.txt`. Notes initiales archivées sous `changenotes/1.0.0.en.txt` et `changenotes/1.0.0.fr.txt`.
- Code sous la licence MIT déjà présente dans le dépôt ; visuels séparés dans `../LICENSING.md`.

Le visuel est une illustration promotionnelle, pas une capture réelle. Pour une galerie supplémentaire, prendre des captures de la version finale : onglet avec parent déplié et tâche autonome, éditeur d'organisation, paramètres maximum/défaut, puis cellules de groupe de Grouped Pawns Lists. L'ancienne capture avec diagnostic technique ne représente pas le rendu final et n'est pas incluse.

## Créer le contenu propre

Depuis la racine du projet :

```powershell
./prepare-workshop.ps1
```

Le script crée `artifacts/workshop/WorkRework-1.0.1-<horodatage UTC>/WorkRework` et un `manifest.json` adjacent. Le sous-dossier **WorkRework** est le contenu destiné au jeu ; le manifeste reste à côté. Il contient les onze fichiers autorisés : métadonnées, PublishedFileId, icône, aperçu, LoadFolders, DLL propre, README, deux documents de licence et traductions FR/EN.

Ne pas sélectionner le checkout de développement comme dossier d'envoi. Le code local du jeu 1.7 confirme que l'uploader prend le dossier racine du mod : il faut utiliser le contenu propre, pas les sources, `.git`, outils ou décompilations.

## Checklist avant le premier envoi — à valider avec le créateur

- [x] Couverture, titre et présentation FR/EN acceptés par l'utilisateur ; liens vers le dépôt retirés à sa demande.
- [x] Version 1.0.0 visible dans RimSort, démarrage sans erreur et fonctionnement en partie validés par les trois réponses utilisateur.
- [x] Build final, contrats, aperçu et manifeste de fichiers conformes (voir docs/TESTING.md).
- [x] Harmony déclaré comme seul élément requis ; Grouped Pawns Lists reste optionnel. Ses utilisateurs installent également RWLayout.
- [x] Visibilité Masquée (privée) vérifiée sur la page créée. Aucun passage en public autorisé.
- [x] Envoi effectué par le créateur et identifiant conservé. Textes FR/EN enregistrés sur la page.

## Lors de l'envoi autorisé

1. Installer le paquet propre dans Mods via le script de déploiement, jeu fermé. Vérifier que le dossier installé ne contient pas de fichiers de développement supplémentaires ; le script préserve volontairement les fichiers non concernés.
2. Ouvrir la liste des mods dans RimWorld, sélectionner Work Rework et utiliser le bouton d'envoi Steam Workshop lorsqu'il est proposé. Le code local confirme une boîte `Dialog_ConfirmModUpload` avant le transfert ; la validation de cette boîte est l'action finale d'envoi.
3. Laisser le compte Steam du créateur effectuer toute acceptation d'accord demandée. Vérifier ensuite la visibilité et la page obtenue ; aucun comportement implicite de confidentialité n'est supposé ici.
4. Le premier envoi utilise le titre et la description d'About.xml. Appliquer ensuite le texte BBCode choisi sur la page, puis la version française dans les champs de langue si proposés.
5. Déclarer Harmony comme élément requis, sans rendre Grouped Pawns Lists obligatoire. Le jeu ajoute les tags `Mod` et `1.7` selon les métadonnées locales.
6. Conserver `About/PublishedFileId.txt` créé par le jeu ; reporter son identifiant dans `listing.json` et le suivi de publication, puis le sauvegarder dans Git. Le même identifiant sert aux mises à jour.
7. Vérifier abonnement, téléchargement, aperçu et dépendance depuis la page obtenue. Éviter d'activer simultanément la copie locale et la copie Workshop du même package.

Dans le code local inspecté, l'uploader ne remplace la description que lors de la création, mais met à jour titre, aperçu, tags et contenu lors des mises à jour. La description longue doit donc être maintenue sur la page au besoin.

## Références vérifiées

- Installation locale 1.7.4985 rev1269 : `ModMetaData.PreviewImagePath` attend `About/Preview.png`, `GetWorkshopUploadDirectory` retourne le dossier du mod, et `Workshop.SetWorkshopItemDataFrom` fixe les données d'envoi.
- [Steamworks — SetItemPreview et limite de l'aperçu](https://partner.steamgames.com/doc/api/ISteamUGC#SetItemPreview) ; la documentation des résultats d'envoi précise la limite inférieure à 1 Mo.
- [Steamworks — processus d'envoi d'un item](https://partner.steamgames.com/doc/features/workshop/implementation#UploadingAWorkshopItem).
- Licence existante conservée dans `../LICENSE`.

## Mise à jour 1.0.1 à envoyer par le créateur

1. Utiliser la copie locale 1.0.1 dans Mods/WorkRework et l'action de mise à jour de l'article existant 3826051737. Ne pas créer de nouvel article.
2. Conserver la visibilité Masquée (privée).
3. Appliquer la note de version 1.0.1 de changenote.en.txt / changenote.fr.txt dans les langues respectives. Les premières notes 1.0.0 sont archivées sous changenotes/.
4. Vérifier après envoi la version téléchargée, les éléments requis (Harmony seul) et la visibilité privée. Signaler la fin de l'envoi pour mettre à jour le suivi local.

La préparation, le tag Git et le déploiement local n'envoient aucune mise à jour Workshop.
