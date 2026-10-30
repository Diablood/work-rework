# Vérifications — Work Rework

## Étape 1 — socle technique, 23 octobre 2026

Statut : contrôles techniques réussis. L'utilisateur a ensuite signalé « Premiers tests OK », ce qui valide le jalon 1 pour poursuivre. Les sous-tests manuels ci-dessous ne sont pas détaillés dans son retour ; aucune observation en jeu n'est attribuée à l'agent.

### Environnement vérifié

- Jeu local : RimWorld 1.7.4985 rev1269, selon Version.txt.
- Assembly du jeu : Assembly-CSharp, version 1.7.9790.38077, cible .NETCoreApp v10.0.
- SDK : .NET 10.0.204.
- Harmony Workshop : About.xml annonce 3.0.1.0 ; l'assembly du dossier 1.7 porte l'identité 0Harmony 3.0.0.0 et cible .NETCoreApp v10.0. Ces deux numéros distincts sont relevés, pas assimilés.
- Références externes locales uniquement ; aucune dépendance NuGet téléchargée.

### Contrôles effectués

- [x] Signatures lues avec MetadataLoadContext, sans exécuter le code du jeu : Verse.Mod(ModContentPack), Verse.Log.Message(string), HarmonyLib.Harmony(string).
- [x] Signatures utiles à la suite relevées : Pawn_WorkSettings.SetPriority(WorkTypeDef, int), GetPriority(WorkTypeDef), CacheWorkGiversInOrder() et ReorderableWidget.NewGroup/Reorderable. Leur fonctionnement interne n'est pas encore analysé.
- [x] Build Release forcé via `./build.ps1` : zéro avertissement, zéro erreur.
- [x] Lecture XML et cohérence du packageId diablood.workrework, de la cible 1.7, de la dépendance Harmony et de son ordre de chargement.
- [x] Dossier d'assemblies du mod limité à WorkRework.dll ; aucune DLL du jeu ou de Harmony copiée.
- [x] Métadonnées du résultat : WorkRework.WorkReworkMod hérite de Verse.Mod et expose le constructeur public attendu avec Verse.ModContentPack.
- [x] Références du résultat : System.Runtime 10.0.0.0, Assembly-CSharp 1.7.9790.38077, 0Harmony 3.0.0.0 ; cible .NETCoreApp v10.0.
- [x] Source du socle sans patch, sérialisation ni traitement par tick. L'instance Harmony est créée mais PatchAll n'est pas appelé.
- [x] ZIP vérifié par liste exacte de ses quatre entrées et comparaison SHA256 de chaque entrée avec son fichier source.

Paquet : artifacts/packages/WorkRework-stage1.zip, 4 117 octets au moment du contrôle.

SHA256 : `72CF6555EAD5CD982ECCC99C424912B3F3646838BEAE6F3B8DD747A3B89E17DE`.

### Checklist manuelle à effectuer

Procédure d'installation et de test détaillée dans le README à la racine, également inclus dans le paquet. À la demande explicite de l'utilisateur, le paquet a ensuite été recréé et extrait dans D:/SteamLibrary/steamapps/common/RimWorld/Mods/WorkRework. Le dossier n'existait pas ; les quatre fichiers installés ont été vérifiés par SHA256 contre les sources. L'activation et les tests en jeu restent à effectuer.

- [x] Work Rework apparaît dans la liste des mods avec Harmony comme dépendance.
- [x] Après activation et redémarrage, le journal contient `[Work Rework] Technical baseline loaded. No gameplay patches applied.` sans nouvelle erreur attribuable au mod.
- [x] Dans une nouvelle petite partie de test, l'onglet Travail reste utilisable ; les priorités natives sont modifiables et un colon effectue son travail normalement.
- [x] Un second lancement charge à nouveau le socle sans erreur attribuable au mod.
- [x] L'utilisateur valide l'étape 1 avant le développement de l'étape 2.

Aucune sauvegarde originale modifiée par l'agent. Aucun test en jeu effectué par l'agent. Aucune compatibilité avec un autre mod annoncée comme validée. Les contrôles de sauvegarde/rechargement des futures données s'appliqueront dès leur introduction à l'étape 3.

## Étape 2 — catégories dépliables et icône utilisateur

Statut : implémentation, contrôles techniques et installation terminés. L'utilisateur a signalé « Tests OK » ; le jalon 2 est validé, sans relevé individuel des sous-tests ci-dessous. Le correctif de raccourci demandé ensuite possède sa propre checklist en fin de document.

### Contrôles effectués

- [x] Méthodes locales inspectées avec ILSpy : PawnTable.Columns, PawnTableOnGUI et calculs de caches/dimensions, MainTabWindow_Work, MainTabWindow_PawnTable, PawnColumnWorker_WorkPriority, PawnColumnWorker, PawnColumnDef, PawnColumnDefGenerator, WidgetsWork et ModMetaData.
- [x] Le jeu charge automatiquement About/ModIcon.png lorsque modIconPath n'est pas défini. Le fichier utilisateur est conservé tel quel et ajouté au paquet.
- [x] Build Release forcé : zéro avertissement et zéro erreur.
- [x] Sept patches de présentation : existence et unicité des cibles, paramètres d'origine, champs privés injectés et types vérifiés sur les assemblies locales par tools/check-api.ps1.
- [x] Vérification du worker enfant et du champ privé table de la fenêtre native, sans exécution du code du jeu.
- [x] XML valides, PNG reconnu et parité des cinq clés de traduction françaises/anglaises et de leurs arguments.
- [x] Packaging limité aux sept fichiers attendus, sans DLL du jeu/Harmony/UnityStubs ; contenu de chaque entrée vérifié par SHA256.
- [x] Déploiement jeu arrêté, installation précédente sauvegardée dans artifacts/backups/WorkRework-20261023-213239-1327012.zip ; sept fichiers installés conformes au ZIP par SHA256.

La lecture du code montre un état d'affichage par tableau et une liste locale d'enfants. Aucun appel à SetPriority, aucune modification des Defs natives et aucun patch de sélection du travail n'est introduit. L'application des boutons est différée au début du dessin suivant pour ne pas changer le nombre de colonnes en plein parcours.

### Checklist en jeu pour l'utilisateur

- [ ] Relancer le jeu : icône présente, message de chargement Stage 2 et aucune erreur liée à Work Rework.
- [ ] Ouvrir Travail : catégories vanilla initialement repliées, boutons + visibles sous les en-têtes.
- [ ] Déplier Construction, puis Cuisine et Cultures : sous-tâches dans l'ordre natif, en-têtes lisibles et cellules alignées.
- [ ] Faire défiler horizontalement jusqu'aux dernières colonnes ; tester aussi une résolution/échelle d'interface habituelle et une longue liste de colons pour le défilement vertical.
- [ ] Modifier une priorité native du parent : ses enfants suivent ; cliquer sur un enfant ne change rien.
- [ ] Vérifier un colon incapable : enfant grisé et infobulle explicative, sans réglage indépendant possible.
- [ ] Replier les catégories : les priorités et le comportement de travail restent inchangés.
- [ ] Vérifier les infobulles françaises et, si possible, anglaises. Les labels des tâches proviennent des traductions du jeu/des Defs.
- [ ] Valider l'étape 2 ou fournir les erreurs/écarts avant l'étape 3.

Limites prévues : enfants en lecture seule ; priorité affichée d'après le parent ; aucune sauvegarde de l'état déplié/replié. La résolution/recréation du tableau peut réinitialiser cet état. Pas de gel de la colonne des noms pendant le défilement horizontal. Grouped Pawns Lists et les autres mods d'interface restent hors validation à ce stade.

## Correctif de l'étape 2 — Ctrl + clic gauche sur le libellé

Demande : remplacer les boutons +/− pour réserver les autres actions de colonne à leur fonctionnement natif et à la future compatibilité Grouped Pawns Lists.

- [x] Suppression des boutons et du patch ajoutant 20 pixels à la hauteur native des en-têtes.
- [x] Interception du seul Ctrl + clic gauche sur la zone du libellé obtenue par GetLabelRect natif. L'événement est consommé avant la logique de tri/colonne. La modification du nombre de colonnes reste différée au dessin suivant.
- [x] Ajout du raccourci à l'infobulle native française/anglaise, sans supprimer ses informations existantes.
- [x] Compilation réussie, zéro avertissement et zéro erreur ; vérification des sept patches et de la signature GetLabelRect(Rect) via les métadonnées locales.
- [x] Paquet réactualisé et installé via deploy.ps1 ; sept fichiers vérifiés par SHA256. Sauvegarde préalable : artifacts/backups/WorkRework-20261023-214354-1409670.zip.
- [ ] En jeu, Ctrl + clic gauche ouvre puis ferme les enfants, sans changement du tri ni des priorités.
- [ ] Le clic gauche simple garde son tri natif ; Maj + clic garde son action native de priorité de colonne.
- [ ] Le raccourci fonctionne aussi après défilement horizontal et n'est pas déclenché dans les cellules de colons.
- [ ] Aucun bouton +/− ne subsiste ; infobulle mise à jour ; aucune nouvelle erreur.
- [ ] Validation utilisateur du correctif avant l'étape 3.

Retour utilisateur ultérieur : « Raccourci validé ». Le correctif Ctrl + clic est accepté ; les sous-tests ne sont pas détaillés individuellement.

La compatibilité Grouped Pawns Lists n'est toujours pas testée : cette séparation des gestes prépare l'intégration future sans la déclarer acquise.

## Correctif oui/non et première version numérotée 0.1.0

Signalement utilisateur : en mode oui/non, les enfants restaient affichés en chiffres.

- Le worker enfant lit le réglage natif useWorkPriorities à chaque dessin : mode simple avec texture de coche native ou case vide, mode manuel avec chiffre de priorité. Une incapacité reste grisée dans les deux modes.
- Les infobulles en mode simple indiquent activé/désactivé ; elles n'exposent plus la priorité numérique, y compris pour une catégorie interdite au colon.
- Les enfants restent en lecture seule : aucun widget interactif ni écriture de priorité ajouté.
- Version initiale 0.1.0 demandée pour identifier le mod dans RimSort : champ modVersion dans About.xml, version de compilation dans Directory.Build.props, AssemblyVersion/FileVersion correspondantes et message de chargement versionné.
- Le build contrôle l'égalité de la version numérique des métadonnées et de la DLL. La source officielle de RimSort lit bien modVersion dans About.xml : https://github.com/RimSort/RimSort/blob/main/app/models/metadata/metadata_factory.py, fonction _parse_optional.
- Contrôles effectués : compilation Release sans avertissement/erreur, cohérence modVersion/AssemblyVersion/FileVersion 0.1.0/0.1.0.0, XML et huit clés françaises/anglaises et leurs arguments, sept signatures de patches et GetLabelRect, empreintes du ZIP, puis des sept fichiers déployés. Sauvegarde précédente : artifacts/backups/WorkRework-20261023-214854-4348287.zip. Pas de test en jeu ou d'observation de RimSort par l'agent.

Checklist à valider en jeu et dans RimSort :

- [ ] Actualiser les métadonnées dans RimSort : Work Rework indique 0.1.0.
- [ ] Relancer le jeu : message de chargement portant Work Rework 0.1.0 et absence de nouvelle erreur liée au mod.
- [ ] Déplier une catégorie et désactiver les priorités manuelles : parents et enfants affichent des coches/cases vides ; aucune priorité numérique dans l'infobulle simple d'un enfant.
- [ ] Activer/désactiver un parent : ses enfants reflètent immédiatement son état sans être cliquables.
- [ ] Réactiver les priorités manuelles : chiffres affichés pour les parents et enfants selon les valeurs natives actuelles, sans nouveau réglage indépendant.
- [ ] Vérifier un colon incapable dans les deux modes ; Ctrl + clic continue à déplier/replier.
- [ ] Validation utilisateur de ce correctif avant le jalon 2b des libellés verticaux.

Retour utilisateur ultérieur : « Tests OK ». La livraison 0.1.0 (correctif oui/non et versionnement) est acceptée pour poursuivre, sans détail individuel des sous-tests.

## Étape 2b — libellés verticaux, version 0.1.1

Statut : contrôles techniques et installation terminés ; validation visuelle et de persistance en jeu en attente.

- [x] Signatures locales examinées pour Mod.GetSettings, ModSettings.ExposeData, Mod.WriteSettings et Listing_Standard.CheckboxLabeled ; utilisation du mécanisme natif ModSettings/Scribe_Values, clé verticalLabels et défaut false.
- [x] Rotation conforme au mécanisme UI.RotateAroundPivot utilisé par le jeu ; matrice, couleur, police, ancrage et WordWrap restaurés dans finally.
- [x] Largeur de 32 px pour parents/enfants, hauteur commune calculée à partir des libellés visibles et bornée à 110–260 px. Libellés trop longs tronqués visuellement ; infobulles complètes.
- [x] Ctrl + clic testé statiquement sur une zone tournée ; autres clics transmis au HeaderClicked natif, dont la signature est vérifiée. Aucune nouvelle logique de priorité/tri introduite.
- [x] Compilation Release : zéro erreur, zéro avertissement ; modVersion 0.1.1 et DLL 0.1.1.0 cohérents.
- [x] Onze patches, GetLabelRect, HeaderClicked, types des workers et classe ModSettings vérifiés sans exécuter le jeu.
- [x] XML et dix clés de traductions françaises/anglaises avec arguments cohérents ; paquet à sept fichiers vérifié par empreintes.
- [x] Paquet versionné artifacts/packages/WorkRework-0.1.1.zip déployé ; sept fichiers installés conformes au paquet, sauvegarde antérieure artifacts/backups/WorkRework-20261023-220016-1999492.zip.

Checklist utilisateur :

- [ ] Actualiser RimSort puis relancer le jeu : version 0.1.1, aucune erreur liée au mod, affichage horizontal par défaut si aucun réglage antérieur.
- [ ] Dans les paramètres de Work Rework, cocher « Afficher les noms des tâches à la verticale » : catégories et enfants tournés, colonnes plus étroites, noms lisibles et cellules alignées.
- [ ] Ctrl + clic sur un nom vertical déplie/replie ; cliquer sur un nom court ne déclenche pas celui de la colonne voisine. Tester aussi après défilement horizontal.
- [ ] Clic simple conserve le tri ; Maj + clic conserve la modification native des priorités de colonne. Les enfants restent non modifiables.
- [ ] Tester les modes oui/non et numérique avec plusieurs catégories dépliées ; les cellules suivent toujours le parent.
- [ ] Vérifier un nom long : pas de chevauchement et libellé complet en infobulle. Tester à l'échelle/résolution d'interface habituelle.
- [ ] Décocher : retour horizontal sans perte des affectations ni des catégories dépliées.
- [ ] Recocher puis quitter et relancer : mode vertical conservé ; décocher reste possible.
- [ ] Validation utilisateur du jalon 2b avant l'étape 3.

Aucun test en jeu ni observation du rendu effectué par l'agent. Le réglage est global au mod ; les sauvegardes des parties ne reçoivent pas de nouvelles données à ce jalon. Compatibilité Grouped Pawns Lists toujours reportée à la phase 2.

Retour utilisateur sur 0.1.1 : « Test ok », avec demande d'aligner les noms verticaux sur le bas. Retour global sans détail individuel des sous-tests.

## Correctif 0.1.2 — alignement inférieur des noms verticaux

La zone de texte mesurée conserve un bord inférieur commun, 18 px au-dessus de la fin de l'en-tête pour réserver l'indicateur de tri. Le texte est ancré à gauche avant rotation à −90°, ce qui place son début en bas après rotation. Le même rectangle sert au rendu, au survol et au Ctrl + clic des catégories ; les enfants utilisent le même calcul de placement.

- [x] Compilation 0.1.2 sans avertissement/erreur ; versions, XML, traductions, onze signatures de patches et paquet vérifiés.
- [x] Installation effectuée, sept empreintes conformes au ZIP ; sauvegarde antérieure artifacts/backups/WorkRework-20261023-221214-4831068.zip. Rendu en jeu non observé par l'agent.

- [ ] En jeu, comparer des noms courts et longs : départ au même niveau inférieur, noms remontant vers le haut.
- [ ] Vérifier les catégories et leurs enfants, y compris les noms très longs tronqués avec infobulle complète.
- [ ] Ctrl + clic et tri fonctionnent sur les libellés déplacés, y compris après défilement horizontal.
- [ ] Revenir à l'horizontale puis à la verticale sans décalage des cellules ni changement d'affectation.
- [ ] Validation utilisateur avant l'étape 3.

Retour utilisateur sur 0.1.2 : « Test ok ». Alignement inférieur accepté ; étape 3 autorisée selon la progression convenue.

## Étape 3 — version 0.2.0, priorités et résumé du parent

Décisions utilisateur du 24 octobre 2026 :

- Un parent désactivé bloque tous ses enfants.
- Résumé comme Work Tab : minimum non nul des enfants, * si leurs valeurs diffèrent (zéro compris dans la différence).
- Modifier le parent applique la nouvelle valeur à tous ses enfants et réinitialise leurs exceptions. Cette décision remplace la proposition initiale de préserver ces exceptions.

### Vérifications techniques

- [x] Lecture des méthodes natives Pawn_WorkSettings (initialisation, Get/SetPriority, ExposeData et cache), JobGiver_Work (urgence, scans, incapacités, ordres forcés), PawnColumnWorker_CopyPasteWorkPriorities et Pawn_JobTracker.Notify_WorkTypeDisabled.
- [x] Parents laissés dans leur DefMap natif ; extension de la validation/saisie 4 → 9 sans modification de la sérialisation native.
- [x] Contrats IL sur l'assembly installé : les constantes liées aux bornes sont seules ciblées ; les constantes 4 d'ancrage du texte et d'enseignement de concepts dans DrawWorkBoxFor sont ignorées. Deux accès scanner priorityInType vérifiés avant leur remplacement par une clé de palier.
- [x] Cache natif conservé sans exception. Avec exceptions, cache reconstruit par priorité effective, ordre natif de catégorie puis de fournisseur ; seuil urgent calculé sur les travaux non urgents actifs. Contrôles natifs de capacité, ShouldSkip et ordres forcés non réécrits.
- [x] Résumé du parent mis en cache par colon/catégorie/mode/base et invalidé à l'édition des exceptions. Référence d'héritage conservée séparément du minimum affiché.
- [x] Commandes de groupe limitées aux actions de widgets/en-têtes et menus. Les désactivations natives liées aux incapacités ou au chargement ne sont pas traitées comme des réinitialisations utilisateur.
- [x] Composant de partie WorkPriorityComponent : dictionnaire par référence de colon, exceptions par defName de fournisseur, valeurs entières 0–9. Ancienne partie sans clé = dictionnaire vide ; noms de Defs absents conservés dans les données. Aucune donnée de partie ajoutée à ModSettings.
- [x] Copier/coller étendu aux exceptions et à la valeur de base du groupe, pour ne pas coller le minimum résumé à la place de l'héritage.
- [x] Compilation sans avertissement/erreur et vingt contrats de patches vérifiés, dont les paramètres et champs injectés ; constructor GameComponent(Game) contrôlé.
- [x] 34 assertions de règles : héritage, blocage, simple/manuel, cycles 0–9, minimum/astérisque, ordre stable et passage d'urgence. Contrats IL des trois limites et des deux accès scanner réussis. Ces tests n'exécutent pas le jeu et ne simulent pas une sauvegarde.
- [x] Paquet 0.2.0 final reconstruit et contrôlé, puis installé via deploy.ps1. Les sept fichiers installés correspondent au ZIP par SHA256 ; la 0.1.2 précédente est sauvegardée dans artifacts/backups/WorkRework-20261023-224034-5269728.zip. Aucun test en jeu effectué par l'agent.

### Checklist en jeu — utiliser une copie de partie

- [ ] RimSort indique 0.2.0 ; chargement sans nouvelle erreur liée au mod. Tester sans Work Tab ni autre gestionnaire de priorités (Grouped Pawns Lists reste pour la phase 2).
- [ ] Ouvrir une copie de partie 0.1.2 ou sans le mod : valeurs natives 0–4 identiques, enfants initialement hérités ; option verticale déjà configurée conservée.
- [ ] Colon A : Construction à 4, Réparer à 1 et Déconstruire à 0. Autres enfants hérités à 4, bordures bleues sur les exceptions, parent 1*. Replier puis déplier ne change rien.
- [ ] Choisir 3 sur le parent par clic milieu : tous les enfants à 3, aucune bordure d'exception et aucun *. Modifier de nouveau un enfant : * réapparaît.
- [ ] Tester 5, 6 et 9 dans les parents/enfants, clics gauche/droit et Maj + clic d'en-tête, sans message de priorité invalide ni clé de traduction manquante.
- [ ] Ctrl + clic droit sur un enfant rétablit la dernière valeur de groupe ; le menu indique cette valeur. Un parent à 0 bloque tous les enfants et ne permet pas d'activer une exception tant qu'il est désactivé.
- [ ] Colon B : autres priorités et exceptions ; sauvegarder sous un nouveau nom, revenir au menu puis recharger. Valeurs de base, valeurs propres et astérisques identiques. Répéter après un redémarrage complet du jeu.
- [ ] Copier/coller de A vers un colon capable : mêmes enfants, y compris hérités, exceptions et valeurs 5–9. Aucun changement sur les autres colons.
- [ ] Basculer oui/non puis manuel sans éditer entre-temps : cases cohérentes, valeurs numériques originales retrouvées. Modifier le parent en mode simple agit sur le groupe entier.
- [ ] Travaux disponibles simultanément : Réparer 1, Cuisine 2, autres constructions 9. À la prochaine sélection normale, le colon essaie Réparer avant Cuisine, puis construction 9. Modifier une priorité pendant un travail ne force pas un changement immédiat, sauf désactivation native du parent.
- [ ] Vérifier urgence et ordre forcé, colon incapable, catégories horizontales/verticales, défilement et raccourci Ctrl + clic sans régression.
- [ ] Validation utilisateur avant développement de l'étape 4.

Limites : tests réels de sélection et de sauvegarde/rechargement non effectués par l'agent ; aucune compatibilité externe ou prise en charge des méchanoïdes annoncée comme validée. L'ordre configurable et les tâches autonomes restent pour l'étape 4. Les versions antérieures du mod ne connaissent pas les exceptions 0.2.0 ; ne pas considérer un retour arrière de sauvegarde comme validé.

## Étape 4 — version 0.3.0, organisation et tâches autonomes

Retour utilisateur sur la 0.2.0 : « Tests OK ». Étape 3 validée globalement, sans relevé individuel ; les cases de sous-tests restent comme trace de ce qui n'a pas été décrit.

### Contrôles techniques effectués

- [x] API locales inspectées : ReorderableWidget (insertion et callbacks), Window, GameComponent.LoadedGame, Pawn_WorkSettings.EnableAndInitialize / WorkIsActive / SetPriority, Pawn_JobTracker.Notify_WorkTypeDisabled et JobQueue.RemoveAll.
- [x] Build Release forcé sans avertissement ni erreur ; version 0.3.0 cohérente About / projet / DLL ; XML, PNG utilisateur et parité FR/EN avec arguments contrôlés.
- [x] 23 signatures de patches validées sur le jeu local, paramètres et champs injectés compris. Les contrats IL des trois bornes et deux champs de scanners passent toujours.
- [x] 72 assertions de logique : héritage, résumé, urgences, ordre numérique puis position, extraction unique, refus d'une autre origine, réintégration, déplacements, copie du brouillon, normalisation et restauration du modèle, migration des priorités individuelles, zéro et base réactivée sans activation des frères. Les tests exécutent les modèles réels utilisés par le composant ; ils ne simulent pas Verse/Scribe ou les événements GUI.
- [x] Modèle à deux niveaux sans mutation des Defs. Nouvelle clé Scribe workOrganization contenant roots et childOrder ; absence de clé = ordre vanilla. Les doublons/Defs absentes sont ignorés pour l'organisation, les nouveaux fournisseurs sont ajoutés à leur origine ; les exceptions de Defs absentes restent stockées.
- [x] Révision d'organisation et invalidation des caches après Appliquer ; mode vertical et défilement adaptés aux colonnes autonomes. Un brouillon cloné évite l'écriture avant Appliquer.
- [x] Copier/coller et initialisation des tâches autonomes pris en charge ; une incapacité garde le chemin natif de désactivation, une priorité de groupe zéro préserve les tâches extraites.
- [x] ZIP limité aux sept entrées autorisées et empreintes de chaque entrée vérifiées par le build.

### Checklist en jeu — copie de partie 0.2.0, profil vanilla + Harmony

- [ ] RimSort indique 0.3.0 ; chargement sans nouvelle erreur. Valeurs héritées/explicites 0.2.0 et option verticale conservées, ordre vanilla initial.
- [ ] Ouvrir Organiser les tâches depuis Travail puis depuis les paramètres. Cliquer sur un nom de groupe sélectionne ses enfants. La poignée ≡ permet de déplacer groupes et enfants ; une ligne d'insertion indique le dépôt. Tester aussi Monter / Descendre et les listes défilantes.
- [ ] Déplacer une catégorie avec ses enfants et un enfant dans son origine. Annuler, Échap ou croix : aucun changement de l'onglet ni des priorités. Refaire et Appliquer : ordre cohérent dans l'onglet, parents repliés ou dépliés.
- [ ] Colon A : Construction 4 et Réparer hérité ; colon B : Construction 2 et Réparer explicite 1. Extraire Réparer puis le déplacer en tête : une seule colonne autonome, valeurs 4 et 1 ; il disparaît des enfants de Construction.
- [ ] Modifier ou désactiver Construction : Réparer autonome conserve sa valeur et demeure utilisable. Le résumé et le * de Construction ne tiennent plus compte de Réparer. Extraire une tâche bloquée donne zéro ; une incapacité réelle continue à interdire les travaux correspondants.
- [ ] Réintégrer Réparer : seul Construction proposé ; retour en fin de ses enfants et valeurs propres 4 / 1 conservées. S'il était à zéro, les autres enfants restent à zéro malgré la réactivation de la base pour Réparer. Aucun rattachement à Cuisine possible ; un dépôt entre listes n'effectue aucune opération.
- [ ] Deux travaux ordinaires disponibles à même priorité : inverser l'ordre principal puis le sous-ordre et observer la prochaine sélection normale. Une tâche à 6 déplacée devant une tâche à 2 ne la dépasse pas. Ne pas prendre un travail déjà en cours pour une nouvelle sélection.
- [ ] Sauvegarder sous un nouveau nom puis recharger, et redémarrer : ordre, tâches extraites, exceptions et priorités identiques. Charger une autre partie sans configuration retrouve son propre ordre vanilla.
- [ ] Vérifier copier/coller entre deux colons capables, arrivée d'un nouveau colon et autonomie après modification de l'ancienne catégorie. Basculer oui/non ↔ manuel sans édition conserve les chiffres 5–9.
- [ ] Vérifier horizontal/vertical, dépliage Ctrl + clic et défilement avec plusieurs tâches autonomes ; urgence, ordre forcé et colon incapable sans régression.
- [ ] Validation utilisateur du jalon avant l'étape 5.

Limites : rendu et glisser-déposer, sélection réelle, nouveaux colons et sauvegarde/rechargement restent à observer en jeu. Le changement de niveau est un bouton explicite ; le glisser-déposer reste dans une liste. Le circuit d'urgence garde sa séparation native même à priorité égale ; l'ordre configurable porte sur les tentatives dans chaque passage. Pas de prise en charge externe annoncée avant la phase 2.

Installation locale 0.3.0 effectuée par deploy.ps1, jeu fermé : D:/SteamLibrary/steamapps/common/RimWorld/Mods/WorkRework. Les sept fichiers installés sont conformes au ZIP par SHA256. Installation 0.2.0 sauvegardée dans artifacts/backups/WorkRework-20261023-225929-0757459.zip. Aucun test en jeu effectué par l'agent ; attente de la validation utilisateur de l'étape 4.

## Correctif 0.3.1 — zéro vide et rendu vanilla des tâches autonomes

Retour utilisateur sur 0.3.0 : « Tests OK ». Étape 4 validée globalement ; demandes visuelles traitées avant l'étape 5.

- Les cellules enfants et autonomes à zéro ne dessinent plus de trait ; les incapacités restent identifiées séparément.
- Une tâche autonome appelle le fond natif WidgetsWork.DrawWorkBoxBackground, sans détourner GetPriority ni modifier les données. Fond selon compétences, passions et avertissements du WorkType d'origine ; signature privée contrôlée par check-api.
- Chiffres en police Medium, cadre natif 25 px et extension du rectangle de texte comme le parent vanilla. Pas de bordure bleue pour les tâches autonomes ; les enfants explicites la conservent.
- En-tête horizontal Small, blanc, lignes de liaison et dimensions natives 32/39/80 px, hauteur 50 px ; mode vertical blanc, 32 px et alignement inférieur conservés. Pas de sous-libellé autonome dans le tableau ; origine toujours en infobulle.
- Aucun changement du stockage, des priorités ou du planificateur. Aucun test en jeu effectué par l'agent.

Checklist :

- [ ] En mode numérique, un enfant et une tâche autonome à zéro ont une case vide ; gauche/droite permettent encore de les réactiver.
- [ ] Extraire une tâche : même fond de compétence, passions et taille de chiffre que son parent d'origine ; aucune bordure bleue. Tester les modes numérique et oui/non.
- [ ] Libellé autonome blanc et présentation de parent en horizontal et vertical ; alignement inférieur vertical conservé. Réintégrer restaure l'apparence d'enfant et sa bordure d'exception.
- [ ] Un colon incapable reste interdit ; valeurs, organisation et sauvegarde/rechargement inchangés.
- [ ] Validation du correctif avant étape 5.

Contrôles 0.3.1 terminés : compilation Release sans avertissement/erreur, versions/XML/PNG/traductions et 23 signatures de patches conformes ; signature du fond privé natif vérifiée (Rect, Pawn, Verse.WorkTypeDef), 72 assertions et contrats IL réussis. Une première exécution du contrôle additionnel utilisait le mauvais namespace attendu pour WorkTypeDef ; cette attente a été corrigée d'après les métadonnées locales, puis le build complet a réussi. Paquet sept entrées contrôlé par SHA256 et installé jeu fermé dans Mods/WorkRework ; sept fichiers installés conformes. Sauvegarde 0.3.0 : artifacts/backups/WorkRework-20261023-231054-3264562.zip. Rendu en jeu à valider par l'utilisateur.

## Étape 5 — version 0.4.0, repère du travail courant

Le correctif 0.3.1 est validé par le retour utilisateur « Tests OK ». Ce retour global ne constitue pas un relevé individuel des sous-tests.

Implémentation : ActiveWorkHighlight lit Pawn.CurJob.workGiverDef au dessin des cellules. L'enfant actif est encadré quand déplié ; sinon son parent encore rattaché. Une tâche autonome n'encadre jamais son ancienne catégorie. Aucun repère sans fournisseur de travail ni sur un colon mort ; un trajet inclus dans un job de travail reste identifié comme ce travail.

Le cadre vert de 2 px est extérieur à la case, dans la hauteur de ligne native ; aucune modification du chiffre, du fond ou des bordures bleues. GUI.color est restaurée. L'infobulle française/anglaise précise le fournisseur actif. Le rendu du parent utilise le postfix DoCell existant : toujours 23 classes de patches. Aucune nouvelle donnée Scribe, mutation de priorité, recherche de jobs ou boucle par tick.

Contrôles à consigner après build et installation. La mise à jour réelle du cadre, son placement et la lisibilité doivent être observés en jeu ; aucun test en jeu effectué par l'agent.

Checklist manuelle sur une copie de partie :

- [ ] RimSort indique 0.4.0 ; relancer et charger sans nouvelle erreur. Ordre, priorités, exceptions et mode vertical déjà réglés conservés.
- [ ] Faire réparer ou construire un colon : catégorie repliée, cadre vert sur Construction uniquement. Déplier : cadre sur le seul enfant actif, plus sur le parent. Replier : retour sur le parent.
- [ ] Extraire cette tâche et Appliquer pendant qu'il travaille : cadre sur la colonne autonome uniquement, jamais sur son ancienne catégorie. Réintégrer et vérifier le même basculement selon le dépliage.
- [ ] Faire changer de travail le colon : cadre déplacé au prochain dessin. Repos ou activité sans workGiverDef : aucun cadre. Vérifier deux colons exécutant des tâches différentes, sans contamination d'une ligne à l'autre.
- [ ] Vérifier couleurs des priorités 1–9, valeurs zéro, passions natives, * du parent et bordure bleue d'une exception pendant l'activité. Le cadre ne recouvre pas les chiffres et ne se confond pas avec la ligne voisine.
- [ ] Tester les modes oui/non et numérique, en-têtes horizontaux/verticaux et défilement ; les gestes de priorité restent fonctionnels.
- [ ] Sauvegarder/recharger : affectations inchangées et repère calculé d'après le travail courant, sans état visuel obsolète.
- [ ] Validation utilisateur avant l'étape 6.

Contrôles 0.4.0 terminés : build Release final sans avertissement/erreur ; cohérence versions/XML/PNG/traductions ; 23 signatures de patches et contrats Pawn.CurJob / Job.workGiverDef validés sur les métadonnées locales ; 72 assertions de régression et contrats IL réussis. Les tests existants ne couvrent pas le rendu du cadre. Une attente initiale de namespace WorkGiverDef a été corrigée d'après les métadonnées réelles (RimWorld.WorkGiverDef) avant le build final. ZIP sept entrées vérifié et installé jeu fermé, sept fichiers conformes par SHA256. Sauvegarde de la 0.3.1 : artifacts/backups/WorkRework-20261023-231604-6758050.zip. Aucune sauvegarde de partie originale modifiée, aucun test en jeu effectué par l'agent. Validation de l'étape 5 attendue.

## Étape 6 — régression vanilla et ajout demandé de réinitialisation

Étape 5 validée par le retour utilisateur « tests OK ». Les sous-tests non détaillés restent des scénarios à observer, pas des résultats supposés.

Le bouton Rétablir l'organisation vanilla est implémenté dans Dialog_WorkOrganization : remplace seulement le brouillon par le catalogue d'origine, réinitialise sélection et défilement ; Appliquer utilise la migration existante pour réintégrer toutes les tâches autonomes en conservant les valeurs individuelles. Annuler / Échap / croix n'écrivent rien dans la partie.

Contrôles intermédiaires effectués : build 0.4.1 sans avertissement/erreur, XML/versions/PNG/traductions, 23 signatures, 290 assertions de logique et contrats IL réussis. Ajout de 186 assertions couvrant six changements appliqués successifs, trois profils de priorités et deux modes, puis 32 assertions sur le brouillon de réinitialisation et sa migration. Les reconstructions du modèle testées ne sont pas une validation de Verse/Scribe.

Périmètre du contrôle manuel : colons humains adultes capables de travailler, jeu local 1.7.4985 rev1269, contenus officiels + Harmony + Work Rework. Enfants, esclaves, prisonniers et méchanoïdes n'ont pas de validation dédiée ; aucun support externe annoncé. Grouped Pawns Lists reste le jalon suivant, après validation de cette régression.

Checklist de régression proposée :

- [ ] Nouvelle partie et copie de partie existante : mêmes valeurs initiales attendues, ouverture de Travail sans erreur.
- [ ] Deux colons avec priorités différentes (dont 0, 1, 6, 9), exceptions et résumé * : extraction / déplacement / réintégration successifs, aucune valeur effective perdue.
- [ ] Rétablir l'organisation vanilla puis Annuler : rien ne change. Refaire puis Appliquer : ordre d'origine et aucune tâche autonome restante, priorités individuelles conservées.
- [ ] Sauvegarder sous un nouveau nom, revenir au menu et recharger : ordre, exceptions, valeurs et cadre actif cohérents. Autre partie : organisation indépendante.
- [ ] Tâches ordinaires à priorité égale puis 2 contre 6 : ordre choisi uniquement à égalité. Urgence, ordre manuel forcé et colon incapable restent cohérents avec les restrictions natives.
- [ ] Deux orientations, oui/non ↔ manuel, défilement et cadre actif ; relever nombre de colons, résolution/échelle d'interface et éventuel ralentissement ressenti.
- [ ] Validation utilisateur de l'étape 6 avant la compatibilité Grouped Pawns Lists.

Demandes reçues pendant ce jalon : ajouter maximum de priorité réglable et champ de valeur par défaut. Questions en attente sur le traitement des chiffres existants au-dessus d'un maximum abaissé et les moments d'application de la valeur par défaut. Ne pas considérer la 0.4.1 intermédiaire comme la livraison finale de ces demandes ; elle n'a pas été déployée.

## Livraison 0.5.0 — réinitialisation et priorités configurables

Réponses utilisateur : baisser le maximum ramène les chiffres supérieurs au plafond dans la partie courante ; la priorité par défaut s'applique uniquement aux nouveaux colons. Ces décisions lèvent les questions précédentes.

- Réglages globaux MaximumPriority et InitialPriority (clés maximumPriority / initialPriority) : valeurs initiales 9 et 3, bornes 1–9 et 1–maximum. Champs en brouillon, appliqués seulement par Appliquer les réglages de priorité. Orientation verticale toujours indépendante.
- Les deux transpilers de widgets appellent le getter du maximum au lieu de charger une borne figée. SetPriority garde sa borne de sécurité 9 et plafonne les affectations humaines via prefix ; pas de réduction de la borne de sécurité des autres populations.
- Appliquer un maximum différent passe sur les bases natives et exceptions des humanoïdes initialisés trouvés par PawnsFinder et les références stockées, plafonne les valeurs et invalide leurs caches. Même normalisation à LoadedGame. Zéro et les chiffres déjà inférieurs restent inchangés ; une hausse ne restaure pas les chiffres convertis. L'agent ne modifie aucune sauvegarde originale ni préférence du joueur.
- InitialAssignment remplace le 3 vanilla seulement pendant EnableAndInitialize lorsque les réglages n'étaient pas déjà initialisés. Les tâches inactives restent à zéro. Le recrutement d'un colon déjà initialisé dans le monde ne réinitialise pas ses affectations ; la valeur par défaut n'intervient pas à la réactivation.
- Menus et cycles enfants adaptés au maximum ; copier/coller plafonné par les chemins SetPriority / SetOverride existants. La réinitialisation de l'organisation conserve les valeurs via la migration précédente.
- Contrôle intermédiaire : compilation sans avertissement/erreur, 23 signatures et 303 assertions réussies (290 précédentes + 13 sur plafond, cycles, défaut et réactivation). La livraison finale sera contrôlée et installée après mise à jour documentaire.

Checklist courte de la livraison :

- [ ] Paramètres : maximum 4 et défaut 2, puis Appliquer. Les anciens 5–9 deviennent 4 dans parents, enfants et tâches autonomes ; zéro reste zéro. Menus et cycles ne dépassent plus 4. Retour à 9 : les valeurs converties restent à 4.
- [ ] Défaut 2 : un nouveau colon à sa première initialisation a les tâches actives à 2 et les autres à zéro. Colons existants inchangés ; réactiver une tâche ne force pas 2. Tester aussi maximum 1 / défaut 1.
- [ ] Redémarrage et chargement : champs conservés et plafond appliqué aux valeurs de la partie chargée. Vérifier le copier/coller depuis une configuration ayant des exceptions.
- [ ] Plusieurs réagencements puis Rétablir l'organisation vanilla : Annuler conserve l'état précédent ; Appliquer remet les groupes et enfants à leur place, sans tâche autonome ni perte de priorité effective individuelle.
- [ ] Sauvegarder/recharger la partie, puis vérifier cadre actif, urgence, ordre forcé, incapacités et réactivité de l'onglet. Les tests purs ne valident pas cette partie en jeu.
- [ ] Validation utilisateur de cette livraison et de la régression vanilla avant Grouped Pawns Lists.

Livraison finale 0.5.0 : build Release sans avertissement/erreur, 23 signatures et contrats de données (dont Initialized et getter MaximumPriority), XML/PNG/versions/traductions, 303 assertions et contrats IL réussis. ZIP limité aux sept entrées prévues avec SHA256 vérifiés. Déploiement effectué jeu fermé dans D:/SteamLibrary/steamapps/common/RimWorld/Mods/WorkRework ; sept fichiers installés conformes. Sauvegarde de la 0.4.0 : artifacts/backups/WorkRework-20261023-232758-3010939.zip. L'agent n'a modifié aucune préférence globale ni sauvegarde originale et n'a pas effectué les tests en jeu. Attendre le retour utilisateur sur cette livraison et la régression vanilla avant la phase 2.

## Phase 2, étape 7 — pont Grouped Pawns Lists 0.6.0

Retour sur la 0.5.0 : « Tests OK ». Étape 6 et phase 1 validées globalement par l'utilisateur, sans détail individuel ; ne pas transformer ce retour en mesures de performances ou relevé complet de sous-tests.

Copies locales inspectées, non exécutées par l'agent : RimWorld 1.7.4985 rev1269, Harmony assembly 3.0.0.0 ; Grouped Pawns Lists About 2.3.2, assembly PawnTableGrouped.0 1.0.0.0 dans 1.6/Assemblies (SHA256 E6EA99CFE96B0ED8FBB3EA9B128E3372EA95EC2533D93208DE1DC3C103A8A22B). RWLayout Alpha 2, sans modVersion dans son About, 0RWLayout et 0RWLayoutMod assembly 1.0.0.0 dans 1.6/Assemblies ; empreinte 0RWLayout : 60EC8E3CB5F529D9E4D98ED5DB731FAC02FBEA0301CF765B0B958B90191963AA. Leurs LoadFolders n'annoncent que jusqu'à 1.6 ; fonctionnement en 1.7 signalé par l'utilisateur, pas observé par l'agent.

### Adaptation et contrôles

- Lecture locale décompilée : PawnTablePatches / Extensions, PawnTableGroupedImpl / Model / View, GroupColumnDefResolver, GroupColumnWorker / Def / WorkBase / WorkPriority / CopyPaste, PawnTableGroup / GroupColumn, lignes et colonnes du tableau. Deux incompatibilités établies dans le code : limite de groupe fixe 0–4 et absence de mapping pour nos enfants ; le getter ColumnDef attend un PawnColumnDef enregistré, contrairement à nos objets locaux.
- Pont sans référence de compilation obligatoire : détection du package actif après chargement via LongEventHandler, sept patches réflexifs. GetResolver lie nos colonnes aux workers de groupe, sans les ajouter à DefDatabase<PawnColumnDef> ; le getter ColumnDef retrouve les objets locaux. Les GroupColumnWorkerDefs générés par Grouped Pawns Lists restent sous son mécanisme natif.
- Valeurs/visibilité/commandes adaptées aux enfants ; commandes parents de groupe utilisant SetParent pour réinitialiser les exceptions des enfants rattachés. Enfants bloqués par parent zéro et incapacités exclus, tâches autonomes indépendantes. Cellules Work des groupes : chiffres jusqu'au maximum configuré, coche/vide, … entre colons et * pour mélange interne d'enfants. Clic milieu : menu ; Ctrl + clic droit enfant : héritage. Une tâche autonome ignore ce raccourci, comme la cellule individuelle.
- Cellules mixtes : gauche/droite unifie à 3 plafonné, sans utiliser le défaut réservé aux nouveaux colons. Les menus offrent un choix explicite. Héritage de catégorie réinitialise les enfants des colons concernés ; jamais les tâches extraites.
- Défilement horizontal délégué au tableau groupé seulement s'il prend réellement en charge ce tableau ; autorisé sur son modèle Work sans écrire ses préférences. Wrapper de scroll et extension de largeur vanilla de Work Rework désactivés dans ce cas. Rafraîchissement par révision de priorité après les changements, pour reconstruire groupes et caches sans traitement par tick. Cadre actif réservé aux vraies lignes de colons.
- 23 patches vanilla maintenus ; sept hooks facultatifs installés sous un owner Harmony distinct et retirés en cas d'échec d'initialisation. Détection absente = pas de résolution des types tiers. Aucun nouveau stockage Scribe ni DLL de dépendance embarquée.
- Contrôle intermédiaire : compilation sans avertissement/erreur, 303 assertions et contrats IL natifs réussis ; nouveau contrôle PEReader de 16 membres de Grouped Pawns Lists couvrant sept hooks, champs et propriétés, sans charger les anciennes références Unity ou exécuter de code tiers. Le build saute seulement ce contrôle optionnel si l'assembly n'est pas installée au chemin configuré.

### Checklist manuelle — profil de test, copie de partie

- [ ] Ordre Harmony → RWLayout → Grouped Pawns Lists → Work Rework ; vérifier 0.6.0 dans RimSort et le message Grouped Pawns Lists bridge enabled (optional, seven hooks) au chargement, sans nouvelle erreur. L'agent n'a activé ou modifié aucun profil.
- [ ] Ouvrir Travail avec les groupes de colons actifs. Si nécessaire, désactiver l'option Disable group row cells de Grouped Pawns Lists pour voir ses commandes. Déplier/replier un groupe de colons puis les enfants de Construction : cellules alignées et aucun message de colonne introuvable.
- [ ] Créer des priorités différentes sur deux colons d'un même groupe. Sur la cellule parent du groupe, clic milieu → 6 : enfants rattachés à 6, exceptions effacées ; une tâche autonome du même type reste inchangée. Tester * et … avant/après ; copier/coller individuel et collage sur groupe incluent les exceptions.
- [ ] Sur la cellule d'un enfant du groupe, choisir 1 : cet enfant seulement devient explicite pour les colons capables et dont le parent est actif. Ctrl + clic droit reprend l'héritage. Tester un parent zéro et un colon incapable : aucune réactivation par le groupe.
- [ ] Extraire puis déplacer l'enfant : commande de groupe disponible dans sa colonne autonome, toujours indépendante de son origine. Réintégrer / réinitialiser organisation : bons groupes, mêmes colons visés, aucune colonne ni valeur perdue.
- [ ] Maximum 9 : clics gauche/droite et menu accèdent à 5–9 ; maximum 4 : pas de valeur au-delà de 4. Mode oui/non puis manuel cohérents. Défaut des nouveaux colons inchangé par les clics de groupe mixtes.
- [ ] Tri et changement de regroupement ne modifient pas l'identité du colon ciblé ; cadre actif sur la bonne ligne, pas sur un en-tête collectif replié. Tester horizontal/vertical et défilement avec plusieurs groupes et tâches autonomes.
- [ ] Sauvegarder/recharger : regroupement/repli des colons gérés par Grouped Pawns Lists et organisation/priorités gérées par Work Rework conservés. Vérifier aussi un démarrage Work Rework + Harmony sans Grouped Pawns Lists/RWLayout, pour absence de dépendance imposée.
- [ ] Validation utilisateur de cette coexistence avant de choisir un autre mod.

Aucune preuve de compatibilité en jeu avant ce retour utilisateur. Les populations hors colons humains adultes ne reçoivent pas de validation dédiée ici. L'intégration n'active pas le module WorkTab de Grouped Pawns Lists : Work Tab reste désactivé/incompatible avec Work Rework.

Livraison finale 0.6.0 : build Release sans avertissement/erreur ; 23 signatures vanilla, absence de références assembly obligatoires vers les mods optionnels, 303 assertions, contrats IL et 16 contrats Grouped Pawns Lists couvrant les sept hooks réussis. Paquet sept entrées vérifié par SHA256 et installé jeu fermé dans D:/SteamLibrary/steamapps/common/RimWorld/Mods/WorkRework ; sept fichiers installés conformes. Sauvegarde de la 0.5.0 : artifacts/backups/WorkRework-20261023-234116-4053816.zip. Aucun profil de mods, préférence utilisateur ou sauvegarde originale modifié par l'agent. Coexistence en jeu, sept hooks à l'exécution et restauration des groupes restent à valider par l'utilisateur.

## Correctif 0.6.1 — texte de diagnostic, cellules de groupe et barre grise

Retour utilisateur sur 0.6.0 : « Tests OK », avec capture du 24 octobre à 01:42 montrant WorkPriority_Handling: RimWorld.PawnColumnWorker_WorkPriority au-dessus de notre infobulle. Après question, corrections visuelles retenues : harmoniser cases/infobulles et corriger la barre grise, sans changer la hauteur des en-têtes.

Origines confirmées par lecture locale : CPawnTableGroupRow.DoCell de Grouped Pawns Lists dessine le defName et le nom de classe lorsque showDummyColumns est actif ; il ne s'agit pas d'une clé Translate manquante. CTableGrid.DoScrollbars appelle GUI.HorizontalScrollbar ; le GUI.Scrollbar de UnityStubs de ce jeu ignore le style fourni et dessine un rectangle gris de toute la hauteur, ce qui explique la grosse barre observée.

Correction dans Work Rework uniquement :

- Un transpiler sur CPawnTableGroupRow.DoCell filtre la lecture du drapeau de diagnostic pour les colonnes WorkPriority et WorkGiver locales du seul tableau Travail. Ne modifie ni la préférence de Grouped Pawns Lists ni les diagnostics des autres colonnes/tableaux. L'infobulle localisée du pont est conservée et raccourcie.
- Cellules collectives : fond neutre plus cohérent, contour discret, survol et y + 2.5 px comme les cellules individuelles ; police et couleur numérique préservées. Aucun changement de sélection, de chiffre ou de commande.
- Un transpiler sur CTableGrid.DoScrollbars remplace uniquement son appel horizontal par un adaptateur. Si DataSource n'est pas une PawnTableGroupedView du tableau Travail, l'appel original est exécuté. Sinon : rail 4 px, curseur 8 px et zone de clic d'origine ; position de saisie mémorisée par grille, GUIUtility.hotControl pour le glissement et relâchement même hors du rail, bornes et proportion de contenu conservées. Molette, défilement vertical et noms figés gérés comme avant.
- Contrats PEReader étendus : 23 membres et deux sites IL (une lecture showDummyColumns, un appel HorizontalScrollbar), couvrant neuf hooks optionnels. Les transpilers refusent de s'installer si leurs nombres d'instructions attendus changent, avec retrait du pont facultatif en cas d'échec.
- Premier build : zéro avertissement/erreur, 303 assertions, contrats IL natifs, 23 contrats optionnels et deux sites IL réussis. Pas de test en jeu ni de rendu observé par l'agent.

Checklist de correction :

- [ ] Relancer en 0.6.1 : pont actif (nine hooks), aucune nouvelle erreur. Survol d'une case Work : plus de WorkPriority_… ni de nom de classe ; infobulle normale et courte en français et anglais.
- [ ] Cases de groupe alignées et lisibles, chiffres colorés et * / … compréhensibles. Gauche/droite et milieu gardent les mêmes actions ; priorités et exceptions inchangées.
- [ ] Barre horizontale fine : attraper le curseur en son bord sans saut initial, glisser à gauche/droite, relâcher hors du rail puis vérifier qu'il ne reste pas capturé. Cliquer le rail et tester les dernières colonnes, le défilement habituel et les noms figés.
- [ ] Replier/déplier les groupes, extraire/réintégrer une tâche, puis comparer horizontal/vertical. Hauteur des noms verticaux inchangée. Les autres tableaux conservent leur présentation.
- [ ] Validation utilisateur du correctif avant une autre cible de compatibilité.

Livraison finale 0.6.1 terminée avant l'interruption de session : build final sans avertissement/erreur, 303 assertions, contrats IL natifs, 23 membres optionnels et deux sites IL validés. Installation jeu fermé effectuée dans Mods/WorkRework avec les sept SHA256 conformes. Sauvegarde de la 0.6.0 : artifacts/backups/WorkRework-20261023-235142-1492880.zip. À la reprise, version About installée 0.6.1 et DLL installée identique au build final revérifiées. Rendu et interaction restent à valider en jeu par l'utilisateur.

## Correctif 0.6.2 — visibilité des barres horizontale et verticale

Retour détaillé sur 0.6.1 : infobulles OK, cases de groupe OK, interactions OK ; horizontale seulement plus fine mais encore présente, verticale encore présente. Demande : barres uniquement si les colonnes/lignes visibles débordent.

Lecture locale : CTableGrid.PostLayoutUpdate réserve toujours la largeur du scrollbar vertical pour comparer les colonnes et définir scrollOuterWidth, même lorsque vScrollVisible est faux. Une horizontale ainsi déclenchée peut réduire la hauteur et imposer à son tour une verticale. Le correctif précédent ne touchait que le rendu du curseur, pas cette décision de layout.

Correction : dixième hook optionnel, postfix de PostLayoutUpdate sur la seule grille du tableau Travail. Conserve les collections et mesures natives des colonnes/lignes, puis ScrollViewport.Calculate cherche le plus petit état stable des deux barres à partir de zéro, avec tolérance d'un pixel pour l'arrondi. Ne réserve la largeur/hauteur d'une barre que si elle est nécessaire. Remet à jour ensemble flags, viewport, rectangles de clipping/dessin/barres, bornes et positions, en conservant la largeur des noms figés et la hauteur des en-têtes. Replier un groupe ou une tâche, ou changer les dimensions de fenêtre, utilise le même recalcul. L'axe sans barre revient à zéro ; une capture de drag horizontal devenue inutile est libérée.

API BoundsRounded héritée de RWLayout.alpha2.CElement vérifiée dans l'assembly locale via décompilation. Contrats GroupedContracts étendus à PostLayoutUpdate et 19 champs de layout (types bool / float / Rect / Vector2), soit 43 membres optionnels au total et deux sites IL. Le calcul pur utilisé par le postfix est lié au projet de tests : 18 assertions supplémentaires sur exact-fit, marge inférieure à une barre, débordements simples/couplés, arrondi, repli, agrandissement et taille minimale. Build intermédiaire : zéro avertissement/erreur, 321 assertions, contrats natifs et optionnels réussis. Aucun test en jeu effectué par l'agent.

Checklist à valider après redémarrage en 0.6.2 :

- [ ] Peu de tâches visibles et tous les colons visibles : aucune barre horizontale ni verticale.
- [ ] Déplier assez de catégories pour déborder : horizontale présente et utilisable ; replier jusqu'à ce que tout tienne : elle disparaît, sans colonnes bloquées hors écran.
- [ ] Avec assez de lignes pour déborder : verticale présente et toutes les lignes accessibles ; replier les groupes de colons jusqu'à ce que les lignes tiennent : elle disparaît.
- [ ] Quand les deux dimensions débordent, les deux barres fonctionnent ; noms de colons figés, cases alignées et dernières colonnes/lignes accessibles.
- [ ] Infobulles normales, cases et clics toujours OK ; groupes et priorités inchangés. Vérifier horizontal/vertical et résolution/échelle habituelles.
- [ ] Validation utilisateur avant une autre cible de compatibilité.

Livraison finale 0.6.2 : compilation sans avertissement/erreur, 321 assertions, contrats IL natifs, 43 membres optionnels et deux sites IL réussis. ZIP sept fichiers vérifié et installé jeu fermé dans Mods/WorkRework ; sept empreintes installées conformes. Sauvegarde de la 0.6.1 : artifacts/backups/WorkRework-20261024-101629-0776417.zip. Aucun test en jeu effectué par l'agent, aucune sauvegarde originale ni préférence globale modifiée. Attendre validation utilisateur de la visibilité conditionnelle.

Retour utilisateur final sur 0.6.2 : « Tests OK ». Correctif de visibilité conditionnelle et jalon de compatibilité Grouped Pawns Lists acceptés. Les sous-tests non détaillés ne sont pas cochés individuellement. Aucun nouveau code, build, changement de version ou déploiement requis pour consigner ce retour. Version installée conservée : 0.6.2. Prochaine cible à choisir avec l'utilisateur.

## Préparation de publication 1.0.0 — 24 octobre 2026

Version et tag exacts 1.0.0 demandés par l'utilisateur, le périmètre initial étant atteint. Logique de jeu identique à 0.6.2 ; modifications de métadonnées, message de chargement, documents, visuels et scripts de paquet uniquement. Le retour utilisateur Tests OK sur 0.6.2 reste la validation en jeu du comportement ; aucun nouveau test en jeu 1.0.0 par l'agent.

Exécuté : ./prepare-workshop.ps1, incluant rebuild Release forcé. Résultats : zéro avertissement/erreur, versions About/assembly/file/listing/tag cohérentes 1.0.0, XML et clés/arguments FR/EN conformes, aperçu PNG carré 640px / 816832 octets, 23 cibles Harmony contrôlées. 321 assertions pures réussies, contrats IL natifs réussis, 43 membres et deux sites IL Grouped Pawns Lists vérifiés (dix hooks optionnels), sans exécution du code du jeu ou du mod tiers.

Paquet artifacts/packages/WorkRework-1.0.0.zip : dix entrées autorisées et SHA256 conformes. Dossier propre artifacts/workshop/WorkRework-1.0.0-20261024-104622-1501511/WorkRework : dix fichiers identiques, manifeste adjacent, aucun fichier de développement. Icône utilisateur SHA256 inchangée : B16285D5A55DE0E2CA5C8BFF42D51F1E7C33BD87A44C97A83D65142FF885A98F.

Couverture et textes FR/EN préparés pour relecture. Installation Mods conservée en 0.6.2 ; affichage de la nouvelle version dans RimSort et démarrage du paquet 1.0.0 restent à vérifier après installation. Aucun upload Steam, PublishedFileId encore absent. Sources préparées pour sauvegarde dans le dépôt existant, sur main avec tag 1.0.0 ; la vérification des références distantes est effectuée après le push.

### Relecture publique — retrait des références GitHub

Couverture et textes acceptés par l'utilisateur avec demande de retirer les références GitHub. Retrait effectué des textes publics FR/EN, métadonnées About, README livré et fiche de préparation ; aucune logique de jeu modifiée, version 1.0.0 conservée. prepare-workshop.ps1 réexécuté : compilation sans avertissement/erreur, 321 assertions et contrats natifs/optionnels réussis, dix fichiers ZIP et staging vérifiés par SHA256. Recherche complémentaire : aucune occurrence GitHub dans les textes publics ni dans le contenu texte du dernier dossier propre. Le manifeste interne adjacent conserve la destination de sauvegarde et n'est pas inclus dans le mod. Aucun envoi Steam, installation Mods toujours 0.6.2, aucun nouveau test en jeu.

### Installation finale 1.0.0 — 24 octobre 2026

Suite au « on continue » utilisateur, paquet final vérifié par check-project.ps1 et prepare-workshop.ps1 -SkipBuild, puis installé par deploy.ps1, jeu fermé. Dix SHA256 installés conformes ; inventaire exact des dix fichiers, sans fichier supplémentaire ni référence GitHub dans les textes livrés. About 1.0.0 et assembly 1.0.0.0 vérifiés. Sauvegarde de la 0.6.2 : artifacts/backups/WorkRework-20261024-105548-9946744.zip. Aucun réglage ni sauvegarde de partie modifié. Dernier contrôle utilisateur à faire : version dans RimSort, démarrage sans erreur et onglet Travail. Aucun lancement de jeu ni upload Steam par l'agent.

### Validation utilisateur finale 1.0.0 — 24 octobre 2026

Réponses explicites au formulaire interactif : RimSort affiche bien 1.0.0 — Oui, validé ; démarrage sans erreur liée à Work Rework — Oui, validé ; en partie, onglet Travail, priorités, dépliages et groupes — Oui, validé. Ces trois contrôles sont rapportés par l'utilisateur, pas exécutés par l'agent. Version finale acceptée pour l'étape de publication. Couverture et textes déjà acceptés, sans référence publique GitHub. Aucun upload Steam effectué ; visibilité et autorisation d'envoi restent à définir.

### Création Workshop privée et identité persistée

Envoi initial réalisé par l'utilisateur, identifiant fourni dans l'URL et confirmé dans le PublishedFileId.txt installé : 3826051737. Identifiant ajouté aux sources et paquet ; contrôle de cohérence avec listing.json ajouté. Chrome authentifié : visibilité Masquée confirmée par le bandeau Steam, aperçu présent, sauvegarde des descriptions EN/FR confirmée, Harmony déclaré et affiché dans les éléments requis. Aucun changement de visibilité ni nouveau transfert du contenu par l'agent. La consultation web non authentifiée échoue, ce qui n'est pas utilisé seul comme preuve de confidentialité. Capture de la page conservée hors Git sous artifacts/workshop-proof/page-privee.png.

Contrôles après conservation de l'identifiant : rebuild forcé réussi sans avertissement/erreur, 321 assertions, 23 cibles Harmony, contrats IL natifs, 43 membres et deux sites IL optionnels réussis. ZIP et staging onze fichiers vérifiés par SHA256. Le téléchargement sous SteamLibrary/steamapps/workshop/content/294100/3826051737 contient exactement les onze fichiers de l'installation envoyée ; toutes leurs empreintes SHA256 correspondent. Le README du nouveau paquet reflète désormais le statut privé ; aucun nouveau transfert de ce paquet effectué.

Note initiale auto-générée remplacée par les notes 1.0.0 validées EN/FR dans leurs langues respectives ; persistance contrôlée après rechargement des formulaires. Retour à la fiche principale : toujours Masquée. Aucun changement de visibilité ni autre réglage de compte. L'activation de la copie téléchargée dans une partie n'a pas été testée séparément ; seule l'intégrité du téléchargement est contrôlée.

### Correctif local — libellés absents dans Organiser les tâches

Signalement utilisateur : catégories de mods/DLC sans nom dans l'éditeur, malgré un nom visible dans Travail (Secourir, Capture, Nourrice, Guard, Rearm, Monitor, Manage, Recycler, Poisson, Études occultes, Repair gear). Cause dans notre éditeur : WorkTypeDef.LabelCap lit Def.label, tandis que l'en-tête natif PawnColumnWorker_WorkPriority et nos en-têtes verticaux lisent labelShort.CapitalizeFirst(). Confirmé sur les XML locaux Anomaly/DarkStudy et Complex Jobs 1.7/FSFNurse, FSFRearming : labelShort présent, label absent.

Correction limitée à Dialog_WorkOrganization et WorkCatalog.CategoryLabel : labelShort capitalisé en premier, puis LabelCap si label générique renseigné, enfin defName si aucun libellé disponible. Appliquée aux catégories de la liste principale, au nom de l'origine dans les détails d'une tâche autonome et au bouton de réintégration. Les WorkGiverDefs, les traductions fournies par les mods, les données Scribe, priorités et ordre sont inchangés. Aucun nouveau patch Harmony. Version 1.0.0 conservée pour le candidat local, sans nouveau tag ni envoi Workshop.

Contrôles exécutés : build.ps1 forcé, zéro avertissement/erreur, 321 assertions de régression réussies, 23 cibles Harmony vérifiées, contrats IL natifs et 43 membres/deux sites IL optionnels réussis. ZIP onze fichiers vérifié par SHA256. Pas de nouveau test automatisé pour cette correction visuelle limitée ; le rendu dépend des Defs et traductions chargées en jeu.

Tentative d'installation par deploy.ps1 : arrêt avant sauvegarde/extraction car RimWorld est ouvert. Aucun fichier installé modifié. Formulaire envoyé pour demander la fermeture du jeu ; déploiement en attente. À vérifier ensuite : noms des catégories signalées visibles comme dans Travail ; extraire une sous-tâche et vérifier le nom de son groupe d'origine dans le détail/bouton de retour, puis Annuler sans modifier l'organisation. Aucun test en jeu du correctif par l'agent, aucun transfert Steam.

### Installation du correctif local après fermeture utilisateur

L'utilisateur confirme RimWorld fermé. check-project.ps1 et prepare-workshop.ps1 -SkipBuild réussis, puis deploy.ps1 : correctif installé dans Mods/WorkRework, onze empreintes SHA256 conformes. Aucun dossier local précédent présent lors de cette installation, donc aucune nouvelle sauvegarde créée par le script. La copie Workshop existe séparément et reste inchangée ; sa DLL diffère du correctif local. PublishedFileId installé 3826051737 conservé, version 1.0.0 inchangée. Pour tester, choisir la copie locale dans RimSort et éviter d'activer en même temps les deux copies du même package. Aucun profil modifié par l'agent, aucun transfert Steam. Contrôle visuel utilisateur en attente.

### Second cas — enfant de Médecin sans nom

Retour utilisateur : noms de catégories corrigés, mais le premier enfant de Médecin est sans nom à la fois dans Travail et dans Organiser les tâches. Le second contrôle du formulaire ne désignait donc pas un échec confirmé du bouton de réintégration. L'identité exacte du WorkGiverDef chargé n'est pas encore connue ; ne pas l'attribuer à un mod précis. Les XML et traductions françaises locaux de DoctorTendEmergency sont renseignés, donc leur seule lecture ne reproduit pas le défaut chargé en partie.

API locale vérifiée par ILSpy : Def.LabelCap renvoie null si label vide ; WorkGiverDef fournit verb et gerund traduisibles. WorkCatalog.TaskLabel conserve LabelCap lorsqu'il est présent, sinon utilise verb, gerund, puis defName. Fonction utilisée dans les deux listes de l'éditeur, détails des autonomes, en-têtes enfants/autonomes horizontaux et verticaux, mesure de hauteur, colonnes locales et infobulles du travail courant/priorités. Catégories dans les infobulles alignées sur CategoryLabel. Aucun changement des Defs originaux ou des données de jeu.

Pour identifier le cas si le rendu reste vide malgré un label non vide, l'infobulle de l'éditeur affiche WorkGiverDef / WorkTypeDef et le defName uniquement lorsque Prefs.DevMode est actif. Pas de journalisation par frame, aucun changement de préférences par l'agent. L'utilisateur confirme le jeu fermé et autorise l'installation locale. Version/tag conservés, aucun upload Workshop.

Contrôles du second correctif : rebuild sans avertissement/erreur ; 321 assertions existantes, 23 cibles Harmony, contrats IL natifs, 43 membres et deux sites IL optionnels réussis. ZIP onze fichiers SHA256 conforme. Après fermeture confirmée du jeu, déploiement réussi dans Mods/WorkRework : onze empreintes installées conformes. Sauvegarde de la copie locale précédente : artifacts/backups/WorkRework-20261024-114936-9786239.zip. Aucun transfert Steam, aucune partie ni préférence modifiée. Test visuel utilisateur à faire sur le premier enfant de Médecin et après extraction ; si le texte reste invisible, lire le defName dans l'infobulle de l'éditeur en mode développeur. Le rendu n'est pas déclaré corrigé avant ce retour.

### Infobulle du parent Médecin — troisième correction ciblée

Retour utilisateur après deuxième correctif : « Stabilizing » visible dans l'éditeur ; affichage de la tâche autonome explicitement confirmé OK après rectification. Seule la liste des enfants au survol du parent Médecin reste sans nom pour cette tâche. Ne plus demander de retester l'autonomie comme défaut encore ouvert.

Origine vérifiée dans le jeu installé : PawnColumnWorker_WorkPriority.GetHeaderTip appelle la méthode privée statique SpecificWorkListString(WorkTypeDef def), qui concatène directement WorkGiverDef.LabelCap et ne passe pas par nos workers enfants. Ajout d'un postfix étroit à cette méthode : conserve la liste native si tous les labels sont renseignés ; sinon reconstruit les lignes avec WorkCatalog.TaskLabel, en conservant ordre, tirets, retours à la ligne et EmergencyWorkMarker traduit. Aucun changement des priorités, du comportement ou des sauvegardes. Vingt-quatrième cible Harmony et paramètre def vérifiés par check-api.ps1 ; nombre attendu mis à jour. Version/tag inchangés, aucun envoi Workshop.

Installation du correctif d'infobulle : utilisateur confirme le jeu fermé ; deploy.ps1 exécuté avec succès après build sans avertissement/erreur, 321 assertions et contrats réussis (24 cibles Harmony, contrats IL natifs, 43 membres/deux sites IL optionnels). Les onze empreintes du paquet installé sont conformes. Sauvegarde de la copie locale précédente créée par deploy.ps1 sous artifacts/backups. Aucun transfert Workshop, aucune préférence ni partie modifiée. Dernier contrôle requis : survol du parent Médecin dans Travail, la ligne auparavant vide doit afficher Stabilizing ; vérifier les deux orientations de libellés si utilisées. L'éditeur et l'autonomie sont déjà acceptés par l'utilisateur. Aucun test en jeu du nouveau postfix par l'agent.

### Correctif des libellés accepté — 24 octobre 2026

Dernière réponse utilisateur au formulaire : « Oui, l'infobulle est correcte ». Survol du parent Médecin validé avec Stabilizing à la place de la ligne vide. Ce retour complète les validations précédentes : catégories visibles dans Organiser les tâches, sous-tâche Stabilizing visible dans l'éditeur et affichage autonome confirmé OK. Le défaut d'affichage signalé est clos sur la copie locale. Contrôles de compilation/régression déjà réussis au dernier build ; aucune nouvelle modification de code, aucun nouveau test en jeu par l'agent. Version locale 1.0.0 inchangée. Le Workshop privé 3826051737 reste sur sa première DLL, sans les correctifs locaux ; toute mise à jour distante doit être consignée séparément, sans passer en public.

### Livraison 1.0.1 — notes, version et préparation Workshop

Demande explicite utilisateur : préparer notes, versionnement, push et tag GitHub ; mise à jour Workshop réalisée ensuite par lui. Version/tag 1.0.1, article existant 3826051737 inchangé. Notes FR/EN actualisées, notes initiales 1.0.0 archivées. Aucune nouvelle logique de jeu depuis le correctif accepté ; numéro et documents synchronisés.

Exécuté : prepare-workshop.ps1 (rebuild forcé), compilation zéro avertissement/erreur ; 321 assertions, contrats IL natifs, 24 cibles Harmony, 43 membres et deux sites IL optionnels réussis. XML, traductions, aperçu, version DLL/About/listing et PublishedFileId conformes. Paquet artifacts/packages/WorkRework-1.0.1.zip et dossier propre artifacts/workshop/WorkRework-1.0.1-20261024-120823-7403446/WorkRework : onze fichiers et empreintes vérifiés.

Après fermeture du jeu confirmée par l'utilisateur, deploy.ps1 installe la 1.0.1 dans Mods/WorkRework et vérifie les onze SHA256, avec sauvegarde de la copie locale précédente. Aucun nouveau test en jeu après le changement de numéro : code des libellés déjà accepté par l'utilisateur. Aucun transfert Workshop ni modification de visibilité ; dernière version distante connue 1.0.0 privée. Envoi et vérification de la copie téléchargée 1.0.1 à effectuer par le créateur.

Sauvegarde locale avant installation 1.0.1 : artifacts/backups/WorkRework-20261024-120907-8878903.zip. Version installée About 1.0.1 et PublishedFileId 3826051737 relus après déploiement.

### Mise à jour Workshop signalée — 24 octobre 2026

L'utilisateur confirme « Workshop à jour » après préparation et installation de la 1.0.1. Dernière version envoyée signalée : 1.0.1 sur l'article existant 3826051737. Contrôle local immédiat : About de la copie sous steamapps/workshop/content/294100/3826051737 encore en 1.0.0, sept fichiers sur onze identiques à l'installation locale 1.0.1. Le téléchargement du correctif n'est donc pas encore validé ; aucune conclusion d'échec de l'envoi n'en est tirée. Visibilité privée toujours demandée ; aucun changement Steam par l'agent, pas de nouvelle vérification de la page privée ni des notes de version à ce stade. Le tag 1.0.1 reste sur le commit de livraison ; seul le suivi évolue sur main.

### Téléchargement 1.0.1 vérifié — livraison clôturée

Après confirmation utilisateur « c'est à jour », contrôle de la copie Steam sous workshop/content/294100/3826051737 : About 1.0.1, assembly 1.0.1.0, PublishedFileId 3826051737, inventaire exact des onze fichiers et SHA256 tous identiques aux sources de la livraison 1.0.1. Le téléchargement précédemment resté en 1.0.0 est désormais à jour. Aucun nouveau build, déploiement ou transfert nécessaire. Les validations de comportement restent celles fournies par l'utilisateur avant la livraison ; aucune exécution du jeu par l'agent pour ce contrôle d'intégrité. Visibilité privée toujours demandée et inchangée par l'agent. Livraison 1.0.1 clôturée ; pas de nouveau développement en cours.

## Correctif local en validation — priorité initiale des colons de départ (28 octobre 2026)

Signalement utilisateur : nouvelle partie, maximum global 9 et défaut global 5 bien conservés, mais colon à 3. Jeu installé mis à jour en 1.7.4990 rev1069. Lecture locale fraîche de Pawn_WorkSettings, PawnGenerator et GameInitData ; traçage IL temporaire des appels EnableAndInitialize / SetPriority (sous artifacts, non redistribué). Cause confirmée : GameInitData.PrepForMapGen fait DisableAll puis trois sites SetPriority(w, 3) pour répartir le travail des colons sélectionnés après leur première initialisation. Notre ancien scope ne couvrait que EnableAndInitialize.

Correction : nouveau scope temporaire StartingPawnPriorityScope, limité à la liste vivante GameInitData.startingAndOptionalPawns pendant PrepForMapGen (les candidats non sélectionnés retirés par le jeu ne sont plus inclus). Préfixe capture la liste, finalizer restaure le scope précédent même en cas d'exception. SetPriority applique InitialAssignment lorsque le colon est dans cette liste, en plus du cas de première initialisation déjà supporté. Les tâches à zéro restent inactives, le choix natif des tâches et le plafond existant sont conservés. Pas de mutation rétroactive des parties déjà créées, pas de données Scribe nouvelles, pas de préférences modifiées. Aide FR/EN précisée.

QA : 25 cibles Harmony attendues ; contrat du champ List<Pawn> vérifié. Contrat IL ajouté pour le reset et les trois affectations natives à 3 dans PrepForMapGen. Premier build réussi sans avertissement/erreur, 321 assertions et contrats natifs/optionnels passés sur 1.7.4990 ; build final après traduction à effectuer. Version 1.0.1 conservée pour le test local ; pas de nouveau tag ou envoi Steam. Validation requise : nouvelle partie avec max 9/défaut 5 (priorités manuelles) ; puis recharger une partie existante avec ses affectations inchangées. Aucun test en jeu de ce correctif par l'agent.

Build final après mise à jour de l'aide : compilation sans avertissement/erreur, 321 assertions de logique/migration/viewport, 25 signatures Harmony, contrats IL natifs y compris le reset/les trois affectations des colons de départ, 43 membres et deux sites IL optionnels réussis. Paquet onze fichiers et SHA256 conformes. Aucune installation à ce stade : formulaire de fermeture du jeu envoyé. Tests en jeu toujours en attente.

Retour utilisateur : test reporté (« Je ferai le test plus tard »). Aucun déploiement local effectué. Correctif sauvegardé sur main (73c8acc), paquet local prêt sous artifacts/packages/WorkRework-1.0.1.zip. Reprendre seulement à la fermeture du jeu pour installer, puis tester une nouvelle partie en priorités manuelles, maximum 9 / défaut 5, et la conservation des valeurs d'une partie existante. Tag et Workshop inchangés.

### Installation locale du correctif de priorité initiale — 28 octobre 2026

L'utilisateur confirme avoir quitté le jeu. check-project.ps1 et prepare-workshop.ps1 -SkipBuild réussis : paquet du correctif conforme aux sources, onze fichiers SHA256 vérifiés. deploy.ps1 exécuté avec succès dans Mods/WorkRework : onze empreintes installées conformes. Aucun dossier local précédent présent au déploiement, donc aucune nouvelle sauvegarde créée. La copie Workshop reste inchangée. Version 1.0.1 conservée pour le candidat local, identifiant 3826051737 inclus. Pas de modification des profils de mods, des préférences ou des parties par l'agent. Tester en sélectionnant la copie locale dans RimSort, sans activer simultanément la copie Workshop. Validation utilisateur attendue : nouvelle partie max 9 / défaut 5, priorités manuelles ; puis affectations d'une partie existante conservées. Aucun nouveau test en jeu par l'agent, aucun envoi Workshop.

### Validation utilisateur du correctif de priorité initiale — 28 octobre 2026

Réponses explicites au formulaire après installation locale : nouvelle partie, maximum 9 et défaut 5 — « Oui, priorité 5 et tâches inactives conservées » ; rechargement d'une partie existante — « Oui, les priorités sont conservées ». Correctif accepté sur les deux parcours demandés. Ces tests en jeu sont rapportés par l'utilisateur, pas exécutés par l'agent. Aucun nouveau code, build ou déploiement nécessaire pour consigner ce retour. Candidat local toujours numéroté 1.0.1 ; tag et Workshop restent sur la livraison précédente. Préparation d'une nouvelle version et mise à jour distante à traiter séparément.

## Correctif local en validation — priorité des nouveaux esclaves (30 octobre 2026)

Signalement utilisateur : pion emprisonné puis recruté comme esclave, affectations initiales à 3 malgré le défaut configuré. Jeu local 1.7.4990 rev1069. API et chemin vérifiés dans les assemblies locales : Pawn_GuestTracker.SetGuestStatus(newHost, Slave) appelle Pawn.SetFaction si la faction change ; SetFaction appelle guest.SetGuestStatus(null), puis workSettings.EnableAndInitialize. Les réglages de travail existent déjà depuis l'emprisonnement, donc notre scope de première initialisation ne remplace pas le 3 natif.

Nouveau SlaveEnrollmentPriorityScope, temporaire et limité au pion concerné : actif seulement pour la demande Slave vers une faction joueur différente de sa faction actuelle. Préfixe SetGuestStatus conserve l'ancien contexte ; finalizer le restaure, y compris en cas d'exception. L'appel imbriqué SetGuestStatus(null) conserve le contexte externe jusqu'à l'initialisation du travail. SetPriority reconnaît ce pion pour appliquer InitialAssignment aux écritures natives à 3. Ne touche pas aux esclaves déjà présents, aux changements de statut sans changement de faction, aux factions non joueuses ou à l'emprisonnement seul. Pas de migration ni de nouvelles données Scribe. Tâches désactivées et restrictions d'esclavage laissées au jeu.

26 cibles Harmony contrôlées, contrat IL ajouté pour les appels SetGuestStatus -> SetFaction -> EnableAndInitialize et l'appel guest imbriqué. Premier build réussi sans avertissement/erreur, 321 assertions et contrats natifs/optionnels passés. Aide FR/EN et documentation précisées, build final à suivre. Version du candidat local 1.0.1 conservée, aucun tag ou transfert Workshop. Tests à faire : nouvel asservissement (ou reprise avant celui signalé), défaut 5/max 9, tâches autorisées à 5 et interdictions conservées ; esclave déjà présent avec priorités personnalisées inchangées au rechargement. L'esclave déjà asservi à 3 ne sera pas modifié rétroactivement.

Build final après mise à jour de l'aide réussi : zéro avertissement/erreur, 321 assertions existantes, 26 signatures Harmony, contrats IL natifs (dont initialisation des colons de départ et asservissement), 43 membres et deux sites IL optionnels. ZIP onze fichiers et SHA256 conformes. Fermeture du jeu demandée par formulaire ; installation locale et tests en jeu encore en attente. Aucun envoi Steam.
