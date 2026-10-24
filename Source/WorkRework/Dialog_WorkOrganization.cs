using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace WorkRework;

internal sealed class Dialog_WorkOrganization : Window
{
    private readonly WorkPriorityComponent component;
    private WorkLayout draft;
    private string selectedRoot;
    private string? selectedChild;
    private Vector2 rootScroll;
    private Vector2 childScroll;
    private Action? pending;
    public override Vector2 InitialSize => new(Mathf.Min(1000f, UI.screenWidth - 32f), Mathf.Min(760f, UI.screenHeight - 40f));

    internal Dialog_WorkOrganization(WorkPriorityComponent component)
    {
        this.component = component;
        draft = component.Layout.Clone();
        selectedRoot = draft.Roots[0];
        forcePause = true;
        absorbInputAroundWindow = true;
        doCloseX = true;
        closeOnAccept = false;
    }

    public override void DoWindowContents(Rect inRect)
    {
        Action? action = pending;
        pending = null;
        action?.Invoke();
        GameFont oldFont = Text.Font;
        TextAnchor oldAnchor = Text.Anchor;
        Color oldColor = GUI.color;
        try
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width - 35f, 36f), "WR_Organize".Translate());
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(0f, 42f, inRect.width, 64f), "WR_OrganizationHelp".Translate());
            float split = (inRect.width - 16f) * 0.46f;
            float bottom = inRect.height - 100f;
            Rect left = new(0f, 132f, split, bottom - 182f);
            Rect right = new(split + 16f, 132f, inRect.width - split - 16f, left.height);
            Widgets.Label(new Rect(left.x, 108f, left.width, 24f), "WR_MainColumns".Translate());
            Widgets.Label(new Rect(right.x, 108f, right.width, 24f), "WR_GroupChildren".Translate());
            DrawList(left, draft.Roots, ref rootScroll, true);
            bool group = selectedRoot.StartsWith("G:", StringComparison.Ordinal);
            if (group)
                DrawList(right, draft.Children[selectedRoot[2..]], ref childScroll, false);
            else
            {
                Widgets.DrawMenuSection(right);
                Widgets.Label(right.ContractedBy(12f), "WR_StandaloneDetails".Translate(Label(selectedRoot), WorkCatalog.Type(draft.Origin(selectedRoot[2..]))!.LabelCap));
            }
            DrawMoveButtons(new Rect(left.x, left.yMax + 8f, left.width, 32f), draft.Roots, selectedRoot);
            if (group)
                DrawMoveButtons(new Rect(right.x, right.yMax + 8f, right.width, 32f), draft.Children[selectedRoot[2..]], selectedChild);

            if (group && selectedChild != null)
            {
                string task = selectedChild;
                if (Widgets.ButtonText(new Rect(right.x, bottom, right.width, 32f), "WR_Extract".Translate()))
                    pending = () => { if (draft.Extract(task)) { selectedRoot = WorkLayout.Task(task); selectedChild = null; } };
            }
            else if (!group)
            {
                string task = selectedRoot[2..];
                if (Widgets.ButtonText(new Rect(right.x, bottom, right.width, 32f), "WR_Reintegrate".Translate(WorkCatalog.Type(draft.Origin(task))!.LabelCap)))
                    pending = () => { if (draft.Reintegrate(task, draft.Origin(task))) { selectedRoot = WorkLayout.Group(draft.Origin(task)); selectedChild = task; } };
            }
            Rect reset = new(left.x, bottom, left.width, 32f);
            if (Widgets.ButtonText(reset, "WR_ResetOrganization".Translate()))
                pending = () =>
                {
                    draft = new WorkLayout(WorkCatalog.Categories);
                    selectedRoot = draft.Roots[0];
                    selectedChild = null;
                    rootScroll = childScroll = Vector2.zero;
                };
            TooltipHandler.TipRegion(reset, "WR_ResetOrganizationTip".Translate());
            Widgets.Label(new Rect(0f, inRect.height - 60f, inRect.width - 260f, 55f), "WR_OrganizationSaveNote".Translate());
            if (Widgets.ButtonText(new Rect(inRect.width - 250f, inRect.height - 40f, 120f, 36f), "WR_Cancel".Translate())) Close();
            if (Widgets.ButtonText(new Rect(inRect.width - 120f, inRect.height - 40f, 120f, 36f), "WR_Apply".Translate()))
            {
                component.ApplyOrganization(draft);
                Close();
            }
        }
        finally { Text.Font = oldFont; Text.Anchor = oldAnchor; GUI.color = oldColor; }
    }

    private void DrawList(Rect rect, List<string> items, ref Vector2 scroll, bool roots)
    {
        Widgets.DrawMenuSection(rect);
        Rect viewport = rect.ContractedBy(4f);
        Rect content = new(0f, 0f, viewport.width - 18f, Mathf.Max(viewport.height, items.Count * 44f));
        Rect absoluteViewport = new(UI.GUIToScreenPoint(viewport.position), viewport.size);
        Widgets.BeginScrollView(viewport, ref scroll, content);
        int group = ReorderableWidget.NewGroup((from, insertion) =>
        {
            // Separate lists deliberately have no native multi-group. Ignore a
            // release in another pane, rather than interpreting its row index here.
            if (absoluteViewport.Contains(UI.GUIToScreenPoint(Event.current.mousePosition)))
                pending = () => WorkLayout.Move(items, from, insertion);
        }, ReorderableDirection.Vertical, content, 4f);
        for (int i = 0; i < items.Count; i++)
        {
            string item = items[i];
            Rect row = new(0f, i * 44f, content.width, 40f);
            if ((roots ? selectedRoot : selectedChild) == item)
                Widgets.DrawBoxSolid(row, new Color(0.25f, 0.36f, 0.47f, 0.6f));
            // Dragging the handle must not change the selected group and rebuild
            // the other pane while the native widget is starting a drag.
            Rect selection = new(row.x + 32f, row.y, row.width - 32f, row.height);
            if (Event.current.type == EventType.mouseDown && Event.current.button == 0 && Mouse.IsOver(selection))
            {
                if (roots && selectedRoot != item) { selectedRoot = item; selectedChild = null; childScroll = Vector2.zero; }
                else if (!roots) selectedChild = item;
            }
            Widgets.Label(new Rect(5f, row.y + 8f, 24f, 26f), "≡");
            string label = roots ? Label(item) : WorkCatalog.Giver(item)!.LabelCap.ToString();
            string marker = roots ? (item.StartsWith("G:", StringComparison.Ordinal) ? "WR_GroupMarker" : "WR_StandaloneMarker").Translate().ToString() : "WR_TaskMarker".Translate().ToString();
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(32f, row.y + 1f, row.width - 36f, 23f), label.Truncate(row.width - 36f));
            Text.Font = GameFont.Tiny;
            GUI.color = Color.gray;
            Widgets.Label(new Rect(32f, row.y + 22f, row.width - 36f, 18f), marker);
            GUI.color = Color.white;
            Text.Font = GameFont.Small;
            TooltipHandler.TipRegion(row, label + "\n" + "WR_DragWithinList".Translate());
            ReorderableWidget.Reorderable(group, row);
        }
        Widgets.EndScrollView();
    }

    private void DrawMoveButtons(Rect rect, List<string> items, string? selected)
    {
        int index = selected == null ? -1 : items.IndexOf(selected);
        float width = (rect.width - 8f) / 2f;
        if (Widgets.ButtonText(new Rect(rect.x, rect.y, width, rect.height), "WR_MoveUp".Translate(), active: index > 0))
            pending = () => WorkLayout.Move(items, index, index - 1);
        if (Widgets.ButtonText(new Rect(rect.x + width + 8f, rect.y, width, rect.height), "WR_MoveDown".Translate(), active: index >= 0 && index < items.Count - 1))
            pending = () => WorkLayout.Move(items, index, index + 2);
    }

    private static string Label(string root) => root.StartsWith("G:", StringComparison.Ordinal)
        ? WorkCatalog.Type(root[2..])!.LabelCap.ToString() : WorkCatalog.Giver(root[2..])!.LabelCap.ToString();
}
