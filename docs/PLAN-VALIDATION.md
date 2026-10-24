# Work rework — étapes et validations

Référence fonctionnelle : [CADRAGE.md](CADRAGE.md). Cible : RimWorld 1.7.4985 rev1269 installé localement. Date de création : 23 octobre 2026.

## Règle de progression

- Traiter un seul jalon de développement à la fois. Les suivants décrivent la suite, pas du travail à réaliser immédiatement.
- À chaque jalon, fournir les changements, les vérifications effectuées, la procédure en jeu et les limites connues.
- Distinguer vérification technique et validation utilisateur. Une compilation réussie ne coche jamais un test en jeu.
- Arrêter au point de contrôle de chaque jalon et attendre la validation explicite de l'utilisateur avant de développer le suivant.
- Corriger les problèmes du jalon courant avant de poursuivre. Ne pas cocher un résultat supposé ou non observé.
- Pas de version publiée, tag, déploiement dans l'installation de jeu ni modification d'une sauvegarde originale sans instruction correspondante.

## Étape 0 — modèle fonctionnel

- [x] Ordre commun à tous les colons ; priorités individuelles.
- [x] Catégories vanilla repliées au premier lancement, avec enfants accessibles en dépliant.
- [x] Priorité du parent et exceptions sur les enfants.
- [x] Deux niveaux maximum ; extraction d'un enfant en colonne autonome.
- [x] Rattachement à une autre catégorie d'origine interdit.
- [x] Conservation des valeurs à l'extraction et au retour vers l'origine.
- [x] Priorité numérique avant ordre principal, puis sous-ordre.
- [x] Schéma et explication validés par l'utilisateur.
- [x] Développement par jalons et compatibilité avec d'autres mods en phase 2 demandés.

## Phase 1 — fonctionnement vanilla

### Étape 1 — socle technique 1.7

Livrable : projet minimal compilable et chargeable, sans changement du comportement du travail.

- [x] Vérifier les signatures des API et la chaîne de compilation sur les assemblies locales 1.7 ; choisir le framework cible et la version de Harmony sur cette base.
- [x] Créer l'identité technique provisoire, About.xml, structure de sources et commande de build reproductible ; ne pas reprendre le packageId de Work Tab.
- [x] Compiler sans embarquer les assemblies du jeu.
- [x] Préparer un paquet local de test et les instructions d'installation manuelle.
- [x] Vérifier en jeu le chargement sans erreur attribuable au mod ; l'onglet vanilla reste utilisable. Premiers tests signalés OK par l'utilisateur ; aucun relevé détaillé des sous-tests fourni.
- [x] Validation utilisateur du jalon : « Premiers tests OK ».

### Étape 2 — catégories et sous-tâches dépliables

Livrable : onglet à présentation vanilla initiale, avec enfants visibles à la demande ; affectations encore natives.

- [x] Implémenter le catalogue à partir des fournisseurs de chaque catégorie ; un enfant par fournisseur, sans doublon. Exhaustivité effective à contrôler en jeu.
- [x] Implémenter l'affichage des parents dans l'ordre natif et de leurs enfants dans le sous-ordre natif.
- [x] Implémenter le dépliage/repliage comme état d'affichage temporaire, sans écriture des affectations ni patch de sélection des travaux.
- [x] Rendre les libellés et infobulles disponibles en français et en anglais ; cohérence des clés et arguments contrôlée.
- [ ] Vérifier colon incapable, catégorie masquée et taille de l'onglet avec plusieurs catégories ouvertes.
- [x] Validation utilisateur du jalon : « Tests OK ». Retour global, sans relevé individuel des sous-tests ci-dessus.
- [x] Implémenter l'amélioration demandée après validation : remplacer les boutons +/− par Ctrl + clic gauche sur le libellé, avec indication dans l'infobulle et conservation des autres clics natifs.
- [x] Validation en jeu de ce correctif d'interaction : « Raccourci validé ».
- [x] Corriger l'affichage des enfants pour suivre le toggle natif : coche/vide en mode oui/non, chiffres en priorités manuelles, infobulles adaptées ; enfants toujours en lecture seule.
- [x] Validation en jeu du basculement oui/non ↔ numérique : « Tests OK » sur 0.1.0.
- [x] Ajouter le versionnement demandé pour RimSort : première version 0.1.0, cohérence contrôlée entre modVersion, projet et DLL, numéro au chargement et historique des changements.
- [x] Vérifier l'affichage de la version installée dans RimSort après actualisation : retour global « Tests OK » sur 0.1.0, sans relevé détaillé des sous-tests.

### Étape 2b — option de libellés verticaux

Demandée par l'utilisateur après la validation de l'étape 2. Traiter après validation du correctif Ctrl + clic et avant les nouvelles priorités.

- [x] Implémenter une option horizontal/vertical, horizontale par défaut, dans les paramètres globaux Work Rework.
- [x] Implémenter la rotation à 90° des parents et enfants, largeur 32 px et hauteur mesurée ; rendu à vérifier en jeu.
- [x] Prévoir les infobulles complètes et la zone Ctrl + clic adaptée au libellé tourné.
- [x] Transmettre les autres clics au gestionnaire natif de tri/priorité ; signatures contrôlées. Alignement et défilement à vérifier en jeu.
- [x] Implémenter la persistance de l'option et le retour à l'horizontale ; conservation au redémarrage à vérifier en jeu.
- [x] Validation utilisateur du jalon 2b : « Test ok », avec demande de correction de l'alignement vertical.
- [x] Implémenter un alignement inférieur commun des libellés verticaux et déplacer leurs zones de clic/survol avec le texte.
- [x] Validation du correctif d'alignement 0.1.2 : « Test ok ».

### Étape 3 — priorités 1–9, héritage et sélection réelle

Livrable : chiffres qui pilotent réellement le travail, pas uniquement son affichage.

- [x] Décisions utilisateur : parent désactivé bloquant tous les enfants ; résumé minimum non nul + * ; modifier le parent applique la valeur à tous les enfants et réinitialise leurs exceptions.
- [x] Implémenter commandes gauche/droite, menu au clic milieu et retour à l'héritage par Ctrl + clic droit ; bordure bleue des valeurs propres.
- [x] Préserver le stockage natif des parents 0–4 et les enfants initialement hérités ; aucune conversion à l'import d'une ancienne partie. À confirmer sur copie en jeu.
- [x] Étendre les trois limites natives de validation/saisie à 9 et les infobulles ; contrôles des instructions ciblées réussis, constantes d'ancrage/enseignement laissées intactes.
- [x] Implémenter cache de sélection par priorité effective, seuil d'urgence et séparation des groupes de scanners par palier numérique ; 34 assertions de logique et contrats IL réussis.
- [x] Implémenter sauvegarde des exceptions par colon et copier/coller natif de la base et des exceptions ; sauvegarde/rechargement réels à tester.
- [ ] Vérifier en jeu les valeurs 5–9, le résumé 1*, l'action de groupe et la sélection réelle.
- [ ] Tester deux colons avec des valeurs différentes sur les mêmes travaux disponibles.
- [ ] Vérifier incapacités, ordres manuels et circuit d'urgence.
- [ ] Sauvegarder/recharger les chiffres et exceptions sans perte.
- [x] Validation utilisateur du jalon 3 : « Tests OK » (retour global, sans relevé individuel des scénarios).

### Étape 4 — ordre, extraction et réintégration

Livrable : écran d'organisation conforme au schéma validé, avec effet réel à priorité égale.

- [x] Implémenter la réorganisation des catégories avec leurs enfants et des enfants dans leur origine.
- [x] Implémenter l'extraction en colonne autonome, à déplacer ensuite dans la liste principale, sans duplication dans le modèle.
- [x] Implémenter la conservation de la priorité effective individuelle, zéro compris ; migration pure testée.
- [x] Réintégration uniquement vers l'origine, valeurs conservées en exceptions. Changement de niveau par boutons explicites ; aucun dépôt croisé entre listes.
- [x] Poignées avec ligne d'insertion native et commandes Monter/Descendre/Rendre autonome/Réintégrer.
- [x] Brouillon détaché, Appliquer et Annuler ; effet réel de l'annulation à vérifier en jeu.
- [ ] Tester la sélection à priorité égale ; une tâche à 6 ne dépasse pas une tâche à 2.
- [x] Implémenter le stockage par partie dans workOrganization ; préférences globales limitées à l'option verticale.
- [ ] Sauvegarder/recharger l'organisation en jeu, y compris depuis une partie 0.2.0.
- [x] Validation utilisateur du jalon 4 : « Tests OK », avec demande de correction visuelle (zéro vide et tâches autonomes comme parents vanilla).
- [x] Correctif visuel 0.3.1 validé par le retour utilisateur « Tests OK ».

### Étape 5 — surlignage du travail courant

Livrable : tâche active identifiable dans toutes les positions d'affichage.

- [x] Implémenter le cadre vert sur l'enfant actif si déplié, sinon sur son parent replié.
- [x] Limiter le repère d'une tâche extraite à sa colonne autonome.
- [x] Lire uniquement le fournisseur du job courant ; aucune catégorie supposée pour le repos ou les activités sans fournisseur.
- [x] Dessiner à l'extérieur de la case pour préserver chiffres, passions et bordures d'exception ; lisibilité à confirmer en jeu.
- [ ] Vérifier la mise à jour lors d'un changement de travail, sans calcul coûteux ajouté à chaque tick.
- [x] Validation utilisateur du jalon 5 : « tests OK » (retour global, sans relevé individuel des sous-tests).

### Étape 6 — régression et validation de la base vanilla

Livrable : première base fonctionnelle validée ; ce jalon ne constitue pas une publication.

- [ ] Rejouer les scénarios de la section 11 du cadrage sur une nouvelle partie et une copie de partie existante.
- [ ] Vérifier sauvegarde/rechargement de l'ensemble : ordre, extraction, chiffres et exceptions.
- [ ] Vérifier le comportement après plusieurs modifications successives de l'organisation.
- [ ] Vérifier urgences, incapacités et ordres manuels sans régression connue.
- [x] Périmètre de cette validation : colons humains adultes capables de travailler ; enfants, esclaves, prisonniers et méchanoïdes restent sans validation dédiée.
- [ ] Relever la réactivité de l'onglet sur une colonie représentative, en indiquant les conditions du test.
- [ ] Mettre à jour la liste des limites et la procédure de test.
- [x] Ajout demandé pendant ce jalon : bouton de retour à l'organisation vanilla dans le brouillon, réintégration des tâches autonomes et conservation des priorités.
- [ ] Valider Annuler / Appliquer sur cette réinitialisation en jeu.
- [x] Ajouter le maximum 1–9 configurable et le défaut réservé aux nouveaux colons, selon les réponses utilisateur ; baisse du maximum convertissant les chiffres supérieurs.
- [ ] Valider les champs, la conversion des valeurs, les nouveaux colons et la persistance des réglages en jeu.
- [x] Validation utilisateur de la phase 1 : « Tests OK » sur 0.5.0 ; retour global, sans relevé individuel des sous-tests.

## Phase 2 — compatibilité avec d'autres mods

### Étape 7 — Grouped Pawns Lists, première cible

Référence : https://steamcommunity.com/sharedfiles/filedetails/?id=2340773428. Sources annoncées : https://gitlab.com/krypt_lynx/PawnTableGrouped/.

La page annonce le regroupement des listes de colons et les dépendances Harmony et RWLayout. L'utilisateur indique que le mod fonctionne déjà en 1.7 malgré le tag absent. Ce signalement est retenu comme contexte ; notre compatibilité reste à tester.

Livrable : coexistence des groupes de lignes de colons et de notre organisation des colonnes de travail. Grouped Pawns Lists reste optionnel.

- [ ] Relever les versions réellement testées du jeu, de Grouped Pawns Lists, de RWLayout et de Harmony.
- [x] Copies locales identifiées pour les contrôles : Grouped Pawns Lists 2.3.2 / assembly 1.0.0.0, RWLayout Alpha 2 / assembly 1.0.0.0, dossiers 1.6 ; jeu 1.7.4985 rev1269 et Harmony assembly 3.0.0.0.
- [x] Examiner ses patches, résolveurs et cellules de groupe ; sept hooks optionnels préparés et seize contrats d'assembly vérifiés.
- [ ] Afficher et replier les groupes de colons avec nos parents dépliés et nos tâches autonomes.
- [ ] Vérifier que tri, clic et surlignage ciblent toujours le bon colon après regroupement ou repli.
- [ ] Définir puis tester les actions de groupe sur priorités héritées, exceptions et tâches autonomes ; ne pas supposer le support des valeurs 5–9.
- [ ] Sauvegarder/recharger sans perdre les réglages de l'un ou l'autre mod.
- [ ] Vérifier le fonctionnement avec et sans Grouped Pawns Lists, sans dépendance obligatoire introduite.
- [ ] Documenter les limites et les éventuels réglages d'ordre de chargement vérifiés.
- [x] Validation utilisateur globale de 0.6.0 : « Tests OK », avec demande de correction de la présentation et du texte technique.
- [ ] Valider le correctif 0.6.1 : cases de groupe, infobulles localisées et barre horizontale fine ; aucune modification de la hauteur des en-têtes demandée.
- [x] Retour détaillé 0.6.1 : infobulles, cases et interactions OK ; visibilité des deux barres à corriger.
- [x] Correctif 0.6.2 et jalon Grouped Pawns Lists acceptés par le retour utilisateur « Tests OK ». Retour global sur la checklist de visibilité des barres, sans relevé individuel supplémentaire.

### Étapes suivantes — autres mods

- [ ] Choisir ensemble la prochaine cible avant de développer une adaptation.
- [ ] Définir ses scénarios de compatibilité et son point de validation propres.

Complex Jobs reste une référence et une cible possible, sans être une dépendance du mod autonome. Aucune compatibilité externe n'est annoncée comme acquise avant son jalon.

## État actuel

- Modèle fonctionnel : validé.
- Plan de jalons : commencement autorisé par l'utilisateur.
- Étape 1 : premiers tests en jeu OK et jalon validé par l'utilisateur.
- Étape 2, correctifs Ctrl + clic et oui/non, et version 0.1.0 validés par les tests utilisateur.
- Étape 2b : 0.1.1 et correctif d'alignement 0.1.2 validés.
- Étape 3 : 0.2.0 validée par le retour utilisateur « Tests OK ».
- Étape 4 et correctif visuel 0.3.1 validés globalement par l'utilisateur.
- Étape 5 : 0.4.0 validée par le retour utilisateur « tests OK ».
- Étape 6 : base vanilla et ajouts 0.5.0 validés par l'utilisateur.
- Étape 7 : compatibilité Grouped Pawns Lists et correctifs jusqu'à 0.6.2 validés par l'utilisateur. Choisir ensemble la prochaine cible.
- Compilation et contrôles statiques : réussis, résultats dans [TESTING.md](TESTING.md).
- Tests en jeu : non effectués par l'agent.
- Dernier jalon validé : étape 7, Grouped Pawns Lists et correctif 0.6.2. Aucun nouveau mod cible choisi ; ne pas commencer une autre adaptation sans ce choix.

## Publication initiale 1.0.0

- [x] Périmètre initial accepté par les tests utilisateur jusqu'à 0.6.2.
- [x] Choix utilisateur : version et tag exacts 1.0.0, dépôt GitHub existant et licence MIT.
- [x] Préparer couverture, descriptions et note de publication FR/EN ; préserver ModIcon.png.
- [x] Préparer un paquet Workshop propre, distinct du checkout, avec contrôle des empreintes.
- [x] Approbation du visuel et des textes par l'utilisateur ; références GitHub retirées à sa demande.
- [x] Premier envoi Steam effectué manuellement par l'utilisateur en privé ; PublishedFileId 3826051737 conservé, page FR/EN et dépendance Harmony configurées.
- [ ] Passage en public uniquement sur nouvelle instruction de l'utilisateur.

Les contrôles de compilation et le statut de sauvegarde GitHub sont consignés dans TESTING.md. Aucune nouvelle compatibilité n'est en cours.
