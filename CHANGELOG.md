# Historique

## Correctif local validé — priorité des colons de départ

- Le réglage de priorité par défaut s'applique aussi aux affectations que RimWorld redistribue juste avant la génération de la carte ; il n'est plus remplacé par 3 lors du lancement d'une nouvelle partie.
- Correction limitée aux colons sélectionnés pour le départ pendant cette préparation. Le choix des tâches actives reste natif ; tâches inactives, parties existantes et réactivation conservées.
- Aide FR/EN précisée. Aucun changement de version ou de tag, aucun envoi Workshop pour ce candidat local.

## 1.0.1 — libellés de l'organisation et infobulles

- L'éditeur utilise maintenant le même libellé court de catégorie que l'onglet Travail. Les WorkTypeDefs de mods ou DLC sans champ label générique ne produisent plus de lignes sans nom.
- Nom d'origine également corrigé dans le détail des tâches autonomes et le bouton de réintégration. Repli sur le label générique, puis defName, si le libellé court est absent.
- Sous-tâches : libellé commun à Travail, en-têtes verticaux, organisation et infobulles ; si le label chargé est vide, repli sur le verbe traduit, le gérondif, puis l'identifiant de la tâche. En mode développeur, l'infobulle de l'éditeur précise le defName pour identifier les cas persistants.
- Infobulle des catégories parentes : la liste native des sous-tâches bénéficie du même libellé de secours si un enfant est sans nom ; ordre et marqueurs d'urgence conservés.
- Aucun changement de priorité, d'organisation sauvegardée ou de sélection des tâches. Correctifs validés en jeu par l'utilisateur sur les catégories, l'enfant Stabilizing, l'autonomie et l'infobulle Médecin. Version et tag 1.0.1 ; mise à jour Workshop à effectuer par le créateur, visibilité privée conservée.

## 1.0.0 — première version complète

- Périmètre initial atteint et validé par l'utilisateur jusqu'à 0.6.2 : priorités configurables, sous-tâches, réorganisation, tâches autonomes, repère du travail courant et compatibilité Grouped Pawns Lists.
- Version du mod et de l'assembly portée à 1.0.0 ; tag Git demandé : 1.0.0. Aucun changement de logique de jeu depuis 0.6.2.
- Préparation Workshop : couverture, descriptions et note de publication FR/EN, paquet propre de dix fichiers et manifeste SHA256. Aucun envoi Steam effectué.
- Sources et documentation préparées pour le dépôt GitHub existant, avec conservation de sa licence MIT ; statut distinct des visuels précisé dans LICENSING.md.

## 0.6.2 — barres de défilement seulement en cas de débordement

- Recalcul de la zone visible du tableau Travail groupé après son layout natif : aucune place réservée à une barre inutile.
- Décision conjointe des deux axes, à partir des colonnes et lignes réellement présentes ; évite qu'une barre verticale réservée d'avance crée une horizontale puis une verticale artificielles.
- Rectangles de dessin, noms figés, bornes et positions de défilement recalculés ensemble. Une barre disparue remet son axe à zéro et libère la capture éventuelle du curseur horizontal.
- Fond des cases, infobulles et interactions 0.6.1 validés par l'utilisateur et conservés. Rail fin horizontal conservé quand le débordement est réel.
- Dix hooks optionnels ; tests de viewport ajoutés et contrats des champs de layout contrôlés. Validation en jeu de la disparition/réapparition attendue.

## 0.6.1 — présentation des groupes et défilement

- Texte WorkPriority_Handling / RimWorld.PawnColumnWorker_WorkPriority identifié comme infobulle de diagnostic de Grouped Pawns Lists, pas comme clé non traduite. Ce doublon technique est masqué sur les cellules de travail ; les réglages de diagnostic et les autres tableaux restent inchangés.
- Infobulles de groupe françaises et anglaises raccourcies ; … reste le marqueur volontaire des priorités différentes entre colons, * celui des différences entre enfants.
- Fond, contour, survol et alignement des cases de groupe harmonisés. Les couleurs des priorités restent conservées, avec un fond neutre pour le groupe.
- Barre horizontale du tableau Travail groupé remplacée par un rail de 4 px et un curseur de 8 px, sans modifier la zone de clic ni les noms figés. Saisie du curseur à sa position, glissement, clic sur rail et bornes conservés ; autres tableaux inchangés.
- Neuf hooks optionnels ; 23 contrats d'assembly et deux sites IL contrôlés en plus de la régression existante. Tests 0.6.0 signalés OK par l'utilisateur, rendu de cette correction à valider.

## 0.6.0 — pont optionnel Grouped Pawns Lists, phase 2

- Adaptation à la copie installée de Grouped Pawns Lists 2.3.2 : cellules de groupe pour catégories, enfants et tâches autonomes, avec maximum configuré, modes simple/numérique et menus au clic milieu.
- Une commande de groupe sur une catégorie applique la valeur aux enfants encore rattachés et efface leurs exceptions. Une commande sur une tâche affecte seulement cette tâche pour les colons concernés ; incapacités et parents désactivés respectés.
- Colonnes locales liées aux résolveurs de groupe sans les enregistrer comme PawnColumnDefs. Valeurs mixtes affichées par … entre colons et * pour les exceptions internes d'une catégorie.
- Défilement horizontal confié au tableau groupé quand il est actif ; rafraîchissement de ses groupes après modification individuelle, dépliage ou réorganisation. Le cadre du travail courant reste sur les lignes individuelles.
- Pont activé seulement si Grouped Pawns Lists est actif, installé après le chargement des mods ; aucune référence de compilation obligatoire ou DLL tierce embarquée. Retour sans pont si son initialisation échoue, avec diagnostic unique.
- Contrôle des 16 membres de l'assembly optionnelle, couvrant les sept hooks ; pas de code du mod tiers exécuté pendant les tests statiques. Base vanilla 0.5.0 validée par « Tests OK » ; coexistence à valider en jeu.

## 0.5.0 — réinitialisation, réglages des priorités et régression vanilla

- Bouton Rétablir l'organisation vanilla dans l'écran d'organisation : ordre des catégories et enfants restauré et toutes les tâches autonomes réintégrées dans leur origine.
- Réinitialisation limitée au brouillon, confirmée par Appliquer ; Annuler / Échap / croix l'abandonnent. Les priorités individuelles sont conservées via la migration existante, sans retour forcé à 1–4 ou effacement des exceptions.
- Régression automatique étendue aux modifications successives pour trois profils de colons en mode numérique/simple et à la réinitialisation complète. Ces tests du modèle ne remplacent pas le chargement Scribe ou les tests en jeu.
- Maximum réglable entre 1 et 9, initialement 9 : menus et cycles adaptés à chaud. Appliquer un maximum abaissé ramène les bases et exceptions supérieures au nouveau plafond dans la partie courante. Remonter le maximum ne restaure pas les anciens chiffres. Le maximum global est aussi appliqué au chargement des parties.
- Champ de priorité par défaut, initialement 3, borné par le maximum : sert seulement à la première initialisation du travail des nouveaux colons. Les tâches laissées inactives par le jeu restent à zéro, les colons existants et la réactivation ne reçoivent pas cette nouvelle valeur.
- Champs modifiables dans les paramètres Work Rework, avec bouton explicite Appliquer les réglages de priorité. Pas de modification des préférences de l'utilisateur par l'agent.
- Version intermédiaire 0.4.1 contrôlée pendant le développement, puis intégrée à cette livraison ; elle n'a pas été installée.
- Étape 5 (0.4.0) validée par le retour utilisateur « tests OK ». Régression vanilla de l'étape 6 et nouveau bouton à valider avant la phase 2.

## 0.4.0 — repère du travail en cours, étape 5

- Cadre vert autour de la cellule correspondant au fournisseur du job courant : enfant si déplié, parent si replié, colonne autonome si extraite.
- Aucun repère sur l'ancien parent d'une tâche autonome ou pour une activité sans fournisseur identifiable.
- Infobulle localisée ; cadre extérieur préservant couleurs de priorité, fonds vanilla, passions et bordures bleues d'exception.
- Lecture au dessin des cellules, sans traitement par tick ni donnée sauvegardée. Aucun changement des priorités ou de leur sélection.
- Correctif 0.3.1 validé par le retour utilisateur « Tests OK ». Étape 5 à vérifier en jeu avant la régression de l'étape 6.

## 0.3.1 — cases à zéro et apparence des tâches autonomes

- Priorité zéro affichée sans trait dans les sous-tâches et tâches autonomes ; une incapacité reste distincte.
- Tâches autonomes : fond vanilla selon les compétences, passions et avertissements natifs, chiffres au format des parents et suppression de la bordure bleue d'exception.
- En-têtes autonomes blancs, police et dimensions de parent vanilla ; suppression du sous-libellé autonome dans l'onglet. L'origine reste indiquée dans l'infobulle. En vertical, alignement inférieur conservé.
- Aucun changement des priorités stockées, de l'organisation ou de la sélection des tâches. Étape 4 signalée « Tests OK » par l'utilisateur ; correction visuelle à valider avant l'étape 5.

## 0.3.0 — organisation et tâches autonomes, étape 4

- Organisation commune aux colons, sauvegardée par partie : ordre principal et sous-ordre des enfants, sans modification des Defs natives.
- Écran accessible depuis Travail et les paramètres : deux listes, poignées de glisser-déposer, Monter / Descendre, brouillon Appliquer / Annuler.
- Extraction par Rendre autonome puis placement dans la liste principale ; retour uniquement vers la catégorie d'origine, par bouton explicite. Le glisser-déposer reste interne à chaque liste.
- Priorités effectives individuelles conservées aux changements de niveau, valeurs numériques conservées en mode simple. Réintégration d'une tâche active dans une catégorie éteinte : base réactivée et autres enfants figés à zéro.
- Résumé et commandes de groupe limités aux enfants encore rattachés. Les tâches extraites restent indépendantes d'une désactivation de leur ancienne catégorie ; une incapacité réelle conserve les règles natives.
- À priorité égale, cache de travail suivant l'ordre choisi et limites de scanner par tâche ; le passage d'urgence natif demeure distinct. Aucun changement immédiat forcé du travail en cours lors d'un déplacement.
- Chargement des anciennes parties avec organisation vanilla par défaut ; nouvelle clé workOrganization, clés d'exceptions conservées. Copier/coller et initialisation des nouveaux colons prennent en charge les tâches extraites.
- Étape 3 signalée « Tests OK » par l'utilisateur. Étape 4 : contrôles techniques effectués, validation en jeu encore attendue.

## 0.2.0 — priorités 1–9 et exceptions, étape 3

- Priorités 1–9 dans les cellules et les commandes natives de colonne.
- Sous-tâches éditables avec héritage, exceptions par colon et bordure bleue pour les valeurs explicites ; Ctrl + clic droit rétablit l'héritage, clic milieu ouvre un choix direct.
- Parent résumé par le minimum non nul des enfants, avec * si leurs valeurs diffèrent. Modifier le parent applique la valeur à tous ses enfants et réinitialise les exceptions, conformément au choix utilisateur.
- Un parent désactivé bloque tous ses enfants ; les incapacités natives restent respectées.
- Cache de sélection ordonné par priorité effective des sous-tâches, avec passage d'urgence adapté et frontières des scanners préservant les priorités numériques.
- Exceptions sauvegardées dans un composant de partie et incluses au copier/coller natif avec la base héritée ; import d'une ancienne partie sans rééchelonnage des valeurs 0–4.
- Contrôles de logique, de résumé, d'urgence et des instructions natives ciblées ajoutés au build. Tests en jeu et de sauvegarde/rechargement à valider par l'utilisateur.

## 0.1.2 — alignement inférieur des libellés verticaux

- Noms verticaux des catégories et sous-tâches alignés sur un bord inférieur commun, avec espace réservé à l'indicateur de tri.
- Zone de clic et survol déplacée avec le texte ; les noms courts restent au même niveau de départ que les noms longs.

## 0.1.1 — libellés verticaux, étape 2b

- Réglage global persistant pour tourner les noms des catégories et sous-tâches de 90°, désactivé par défaut.
- Colonnes de tâches de 32 pixels en mode vertical et hauteur d'en-tête adaptée aux noms visibles ; infobulles complètes pour les noms longs.
- Zone Ctrl + clic adaptée au texte tourné ; tri natif et actions de priorité de colonne transmis au gestionnaire du jeu.
- Retour à l'horizontale sans modification des affectations et sans réinitialiser les groupes dépliés.
- ZIP de test nommé avec la version pour distinguer les livraisons.

## 0.1.0 — version de test locale

Première version numérotée, à la demande de l'utilisateur pour identifier l'installation dans RimSort. Les précédents tests du socle et de l'étape 2 étaient non numérotés.

- Catégories vanilla dépliables, enfants en lecture seule et défilement horizontal.
- Ctrl + clic gauche sur le libellé pour déplier/replier ; raccourci déjà validé par l'utilisateur.
- Correction : les enfants suivent le toggle oui/non ou priorités manuelles, y compris leurs infobulles.
- Icône ModIcon.png fournie par l'utilisateur et traductions françaises/anglaises.
- Version cohérente entre About.xml, compilation, DLL et journal de chargement.

Les priorités 1–9, les exceptions, la réorganisation et l'option de libellés verticaux ne sont pas encore implémentées. Aucun tag ni publication effectué.
