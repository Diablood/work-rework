# Work Rework — guide français

**RimWorld 1.7 · Version 1.0.1 · Harmony requis**

Work Rework reprend les catégories vanilla dans un onglet Travail plus précis : priorités par sous-tâche, ordre personnalisé, tâches autonomes et repère du travail courant.

## Commandes

- **Ctrl + clic gauche sur le nom d'une catégorie** : afficher ou masquer ses enfants.
- **Clic gauche / droit sur une cellule** : faire défiler les priorités.
- **Clic milieu** : choisir directement une valeur.
- **Ctrl + clic droit sur un enfant rattaché** : rétablir l'héritage.
- **Organiser les tâches** : ouvrir l'éditeur depuis Travail ou les paramètres du mod.

Le parent résume ses enfants par le plus petit chiffre non nul ; * indique des différences. Modifier le parent applique la valeur à tous ses enfants rattachés et efface leurs exceptions. Un parent désactivé bloque ces enfants ; une tâche autonome reste indépendante.

## Organisation

Liste de gauche : colonnes principales. Liste de droite : enfants du groupe sélectionné. Glisser une poignée ≡ dans sa liste, ou utiliser Monter / Descendre.

**Rendre autonome** place l'enfant après son origine dans la liste principale ; le déplacer ensuite à la position voulue. **Réintégrer** le remet uniquement dans son origine, à la fin des enfants. Les priorités effectives individuelles sont conservées. Le retour d'une tâche active dans un groupe éteint réactive la base nécessaire tout en maintenant les autres enfants à zéro.

**Rétablir l'organisation vanilla** restaure l'ordre des groupes et enfants et réintègre les tâches autonomes. Toutes ces actions modifient le brouillon ; **Appliquer** confirme, **Annuler**, Échap ou la croix abandonnent. L'organisation est commune aux colons de la partie, les chiffres restent individuels.

Le chiffre de priorité prime sur l'ordre visuel. L'ordre principal puis celui des enfants départagent les tâches ordinaires à priorité égale. Urgences et ordres forcés gardent leurs règles natives.

## Paramètres et affichage

Maximum réglable de 1 à 9, initialement 9. **Appliquer les réglages de priorité** confirme la saisie. Réduire le maximum ramène les chiffres supérieurs au nouveau plafond ; remonter ensuite ne restaure pas les anciens chiffres. Ce plafond global est aussi appliqué aux parties lors de leur chargement.

La priorité par défaut, initialement 3, concerne les tâches activées pour les colons de départ lors de la préparation d'une nouvelle carte, puis la première initialisation du travail des autres nouveaux colons. Les tâches que le jeu laisse inactives restent à zéro. Les colons déjà initialisés d'une partie existante, y compris ceux déjà présents dans le monde avant recrutement, restent inchangés. La réactivation d'une tâche ne force pas ce défaut.

Libellés verticaux optionnels, alignés sur le bas. Un cadre vert identifie le travail en cours : enfant déplié, parent replié ou colonne autonome. Une activité sans fournisseur identifiable n'allume aucune colonne. Les barres n'apparaissent que si le contenu visible déborde.

## Installation et compatibilité

Installer le dossier compilé sous `Mods/WorkRework`, avec Harmony avant Work Rework. Le dépôt source seul n'inclut pas la DLL compilée. **Ne pas activer Work Tab en même temps.**

Grouped Pawns Lists est optionnel. Avec lui : **Harmony → RWLayout → Grouped Pawns Lists → Work Rework**. Intégration testée par l'utilisateur avec Grouped Pawns Lists 2.3.2 sous RimWorld 1.7. Ses cellules de groupe gèrent parents, enfants et autonomes, avec le maximum choisi. … signifie des chiffres différents entre colons ; * des différences entre sous-tâches. Les incapacités et parents éteints restent respectés.

Organisation et exceptions sont sauvegardées par partie ; le dépliage des catégories reste temporaire. L'ajout à une partie existante et la sauvegarde/relecture ont été testés par l'utilisateur. Retrait et rétrogradation non validés. Périmètre initial : colons humains adultes ; autres populations sans validation dédiée.

Code sous MIT ; visuels traités séparément dans `LICENSING.md`.
