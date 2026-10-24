using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace WorkRework;

internal static class WorkTableAccess
{
    internal const float ScrollbarSpace = 20f;
    internal static readonly AccessTools.FieldRef<MainTabWindow_PawnTable, PawnTable> Table =
        AccessTools.FieldRefAccess<MainTabWindow_PawnTable, PawnTable>("table");
    private static readonly AccessTools.FieldRef<PawnTable, PawnTableDef> Definition =
        AccessTools.FieldRefAccess<PawnTable, PawnTableDef>("def");
    internal static bool IsWork(PawnTable table) => Definition(table) == PawnTableDefOf.Work;
    internal static readonly Func<PawnColumnWorker_WorkPriority, Rect, Rect> LabelRect =
        AccessTools.MethodDelegate<Func<PawnColumnWorker_WorkPriority, Rect, Rect>>(
            AccessTools.Method(typeof(PawnColumnWorker_WorkPriority), "GetLabelRect", new[] { typeof(Rect) }));
}

[HarmonyPatch(typeof(PawnTable), nameof(PawnTable.Columns), MethodType.Getter)]
internal static class WorkColumnsPatch
{
    private static void Postfix(PawnTable __instance, PawnTableDef ___def, List<PawnColumnDef> __result)
    {
        if (___def != PawnTableDefOf.Work)
            return;
        WorkTableView view = WorkTableView.For(__instance);
        if (WorkPriorityComponent.Current is { HasOrganization: true } component)
        {
            var parents = __result.Where(c => c.Worker is PawnColumnWorker_WorkPriority && c.workType != null).ToList();
            if (parents.Count == 0) return;
            int first = __result.IndexOf(parents[0]);
            var organized = new List<PawnColumnDef>();
            foreach (string root in component.Layout.Roots)
            {
                if (root.StartsWith("T:", StringComparison.Ordinal))
                {
                    WorkGiverDef? giver = WorkCatalog.Giver(root[2..]);
                    if (giver?.workType.VisibleCurrently == true) organized.Add(view.Column(giver));
                }
                else
                {
                    PawnColumnDef? parent = parents.Find(c => c.workType.defName == root[2..]);
                    if (parent == null) continue;
                    organized.Add(parent);
                    if (view.IsExpanded(parent.workType)) organized.AddRange(view.Children(parent.workType));
                }
            }
            __result.RemoveAll(parents.Contains);
            __result.InsertRange(first, organized);
            return;
        }
        // Insert backwards so existing native column positions are preserved.
        for (int i = __result.Count - 1; i >= 0; i--)
        {
            PawnColumnDef parent = __result[i];
            if (parent.workType != null && parent.Worker is PawnColumnWorker_WorkPriority && view.IsExpanded(parent.workType))
                __result.InsertRange(i + 1, view.Children(parent.workType));
        }
    }
}

[HarmonyPatch(typeof(PawnColumnWorker_WorkPriority), "GetHeaderTip")]
internal static class WorkHeaderTipPatch
{
    private static void Postfix(PawnColumnWorker_WorkPriority __instance, PawnTable table, ref string __result)
    {
        if (!WorkTableAccess.IsWork(table))
            return;
        WorkTableView view = WorkTableView.For(table);
        if (view.Children(__instance.def.workType).Count > 0)
            __result += "\n\n" + (view.IsExpanded(__instance.def.workType) ? "WR_Collapse" : "WR_Expand").Translate();
    }
}

[HarmonyPatch(typeof(PawnColumnWorker_WorkPriority), nameof(PawnColumnWorker_WorkPriority.DoHeader))]
internal static class WorkHeaderTogglePatch
{
    private static bool Prefix(PawnColumnWorker_WorkPriority __instance, Rect rect, PawnTable table)
    {
        if (!WorkTableAccess.IsWork(table))
            return true;
        Event current = Event.current;
        bool vertical = WorkReworkMod.VerticalLabels;
        WorkTableView view = WorkTableView.For(table);
        if (current.type == EventType.mouseDown && current.button == 0 && current.control &&
            view.Children(__instance.def.workType).Count > 0)
        {
            Rect label = vertical ? VerticalWorkHeaders.LabelRect(rect, VerticalWorkHeaders.Label(__instance.def))
                : WorkTableAccess.LabelRect(__instance, rect);
            if (Mouse.IsOver(label))
            {
                view.QueueToggle(__instance.def.workType);
                // Consume before native sorting or Shift-click bulk-priority handling.
                current.Use();
            }
        }
        if (!vertical)
            return true;
        VerticalWorkHeaders.DrawParent(__instance, rect, table);
        return false;
    }
}

[HarmonyPatch(typeof(MainTabWindow_Work), nameof(MainTabWindow_Work.DoWindowContents))]
internal static class WorkWindowPendingPatch
{
    private static void Postfix(Rect rect)
    {
        if (WorkPriorityComponent.Current is { } component &&
            Widgets.ButtonText(new Rect(160f, rect.y + 5f, 190f, 28f), "WR_Organize".Translate()))
            Find.WindowStack.Add(new Dialog_WorkOrganization(component));
    }
    private static void Prefix(MainTabWindow_Work __instance)
    {
        PawnTable table = WorkTableAccess.Table(__instance);
        if (table != null && WorkTableView.For(table).ApplyPending())
            __instance.Notify_PawnsChanged();
    }
}

[HarmonyPatch(typeof(PawnTable), "RecacheSize")]
internal static class WorkTableSizePatch
{
    private static void Postfix(PawnTable __instance, PawnTableDef ___def, int ___maxTableHeight, ref Vector2 ___cachedSize)
    {
        if (___def != PawnTableDefOf.Work)
            return;
        WorkTableView view = WorkTableView.For(__instance);
        view.NeedsScroll = false;
        if (GroupedPawnsCompatibility.Handles(__instance)) return;
        if (!view.HasExpanded && !view.HasStandalone && !WorkReworkMod.VerticalLabels)
            return;
        float width = 16f;
        foreach (PawnColumnDef column in __instance.Columns)
            if (!column.ignoreWhenCalculatingOptimalTableSize)
                width += Mathf.Max(column.Worker.GetOptimalWidth(__instance), 0);
        view.NeedsScroll = width > ___cachedSize.x;
        if (view.NeedsScroll)
        {
            ___cachedSize.x = width;
            ___cachedSize.y = Mathf.Max(1f, Mathf.Min(___cachedSize.y, ___maxTableHeight - WorkTableAccess.ScrollbarSpace));
        }
    }
}

[HarmonyPatch(typeof(MainTabWindow_PawnTable), nameof(MainTabWindow_PawnTable.RequestedTabSize), MethodType.Getter)]
internal static class WorkWindowSizePatch
{
    private static void Postfix(MainTabWindow_PawnTable __instance, ref Vector2 __result)
    {
        if (__instance is not MainTabWindow_Work)
            return;
        PawnTable table = WorkTableAccess.Table(__instance);
        if (table != null && WorkTableView.For(table).NeedsScroll)
        {
            __result.x = Mathf.Min(__result.x, UI.screenWidth);
            __result.y += WorkTableAccess.ScrollbarSpace;
        }
    }
}

[HarmonyPatch(typeof(PawnTable), nameof(PawnTable.PawnTableOnGUI))]
internal static class WorkTableHorizontalScrollPatch
{
    [HarmonyPriority(Priority.First)]
    private static void Prefix(PawnTable __instance, PawnTableDef ___def, ref Vector2 position, out bool __state)
    {
        __state = false;
        if (___def != PawnTableDefOf.Work || Event.current.type == EventType.layout)
            return;
        if (GroupedPawnsCompatibility.Handles(__instance)) return;
        Vector2 size = __instance.Size;
        WorkTableView view = WorkTableView.For(__instance);
        if (!view.NeedsScroll)
            return;
        float visibleWidth = Mathf.Min(size.x, UI.screenWidth - 12f);
        view.Scroll.y = 0f;
        view.Scroll.x = Mathf.Clamp(view.Scroll.x, 0f, size.x - visibleWidth);
        Widgets.BeginScrollView(new Rect(position.x, position.y, visibleWidth, size.y + WorkTableAccess.ScrollbarSpace),
            ref view.Scroll, new Rect(0f, 0f, size.x, size.y));
        position = Vector2.zero;
        __state = true;
    }

    // A finalizer also balances the outer scroll view if the native drawing throws.
    private static void Finalizer(bool __state)
    {
        if (__state)
            Widgets.EndScrollView();
    }
}
