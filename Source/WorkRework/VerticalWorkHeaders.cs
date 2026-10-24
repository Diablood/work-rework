using System;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace WorkRework;

internal static class VerticalWorkHeaders
{
    internal const int ColumnWidth = 32;
    private static readonly Func<PawnColumnWorker_WorkPriority, PawnTable, string> HeaderTip =
        AccessTools.MethodDelegate<Func<PawnColumnWorker_WorkPriority, PawnTable, string>>(
            AccessTools.Method(typeof(PawnColumnWorker_WorkPriority), "GetHeaderTip", new[] { typeof(PawnTable) }));
    private static readonly Action<PawnColumnWorker_WorkPriority, Rect, PawnTable> HeaderClicked =
        AccessTools.MethodDelegate<Action<PawnColumnWorker_WorkPriority, Rect, PawnTable>>(
            AccessTools.Method(typeof(PawnColumnWorker_WorkPriority), "HeaderClicked", new[] { typeof(Rect), typeof(PawnTable) }));

    internal static string Label(PawnColumnDef column) => column is WorkGiverColumnDef child
        ? WorkCatalog.TaskLabel(child.Giver)
        : WorkCatalog.CategoryLabel(column.workType);

    internal static float Measure(string label)
    {
        GameFont oldFont = Text.Font;
        try { Text.Font = GameFont.Small; return Text.CalcSize(label).x; }
        finally { Text.Font = oldFont; }
    }

    internal static Rect LabelRect(Rect header, string label)
    {
        float height = Mathf.Min(Measure(label), Mathf.Max(1f, header.height - 24f));
        // Share a bottom edge across short/long labels; reserve the sort marker.
        return new Rect(header.center.x - 10f, header.yMax - 18f - height, 20f, height);
    }

    internal static void DrawLabel(Rect header, string label, bool inherited)
    {
        Rect area = LabelRect(header, label);
        Rect rotated = new(area.center.x - area.height / 2f, area.center.y - area.width / 2f, area.height, area.width);
        Matrix4x4 oldMatrix = GUI.matrix;
        Color oldColor = GUI.color;
        GameFont oldFont = Text.Font;
        TextAnchor oldAnchor = Text.Anchor;
        bool oldWrap = Text.WordWrap;
        try
        {
            Text.Font = GameFont.Small;
            // After the -90° rotation, the text's left edge becomes its bottom.
            Text.Anchor = TextAnchor.MiddleLeft;
            Text.WordWrap = false;
            GUI.color = inherited ? Color.gray : Color.white;
            UI.RotateAroundPivot(-90f, area.center);
            Widgets.Label(rotated, label.Truncate(rotated.width));
        }
        finally
        {
            GUI.matrix = oldMatrix;
            GUI.color = oldColor;
            Text.Font = oldFont;
            Text.Anchor = oldAnchor;
            Text.WordWrap = oldWrap;
        }
    }

    internal static void DrawParent(PawnColumnWorker_WorkPriority worker, Rect header, PawnTable table)
    {
        string label = Label(worker.def);
        Rect hit = LabelRect(header, label);
        DrawLabel(header, label, inherited: false);
        if (Mouse.IsOver(hit))
        {
            Widgets.DrawHighlight(hit);
            TooltipHandler.TipRegion(hit, HeaderTip(worker, table));
        }
        if (Widgets.ButtonInvisible(hit))
            HeaderClicked(worker, header, table);

        if (table.SortingBy == worker.def)
        {
            TextAnchor oldAnchor = Text.Anchor;
            GameFont oldFont = Text.Font;
            try
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                Text.Font = GameFont.Tiny;
                Widgets.Label(new Rect(header.x, header.yMax - 18f, header.width, 18f), table.SortingDescending ? "▼" : "▲");
            }
            finally { Text.Anchor = oldAnchor; Text.Font = oldFont; }
        }
    }
}

[HarmonyPatch(typeof(PawnColumnWorker_WorkPriority), nameof(PawnColumnWorker_WorkPriority.GetMinWidth))]
internal static class VerticalParentMinWidthPatch
{
    private static void Postfix(PawnTable table, ref int __result)
    {
        if (WorkReworkMod.VerticalLabels && WorkTableAccess.IsWork(table))
            __result = VerticalWorkHeaders.ColumnWidth;
    }
}

[HarmonyPatch(typeof(PawnColumnWorker_WorkPriority), nameof(PawnColumnWorker_WorkPriority.GetOptimalWidth))]
internal static class VerticalParentOptimalWidthPatch
{
    private static void Postfix(PawnTable table, ref int __result)
    {
        if (WorkReworkMod.VerticalLabels && WorkTableAccess.IsWork(table))
            __result = VerticalWorkHeaders.ColumnWidth;
    }
}

[HarmonyPatch(typeof(PawnColumnWorker_WorkPriority), nameof(PawnColumnWorker_WorkPriority.GetMaxWidth))]
internal static class VerticalParentMaxWidthPatch
{
    private static void Postfix(PawnTable table, ref int __result)
    {
        if (WorkReworkMod.VerticalLabels && WorkTableAccess.IsWork(table))
            __result = VerticalWorkHeaders.ColumnWidth;
    }
}

[HarmonyPatch(typeof(PawnColumnWorker_WorkPriority), nameof(PawnColumnWorker_WorkPriority.GetMinHeaderHeight))]
internal static class VerticalParentHeightPatch
{
    private static void Postfix(PawnTable table, ref int __result)
    {
        if (WorkReworkMod.VerticalLabels && WorkTableAccess.IsWork(table))
            __result = WorkTableView.For(table).VerticalHeaderHeight(table);
    }
}
