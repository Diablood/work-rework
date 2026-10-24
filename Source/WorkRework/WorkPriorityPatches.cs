using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace WorkRework;

internal static class PriorityTranspilers
{
    internal static IEnumerable<CodeInstruction> ExtendRange(IEnumerable<CodeInstruction> instructions, int expected, bool configurable = true)
    {
        var code = instructions.Select(instruction => new CodeInstruction(instruction)).ToList();
        int replaced = 0;
        for (int i = 0; i + 1 < code.Count; i++)
        {
            CodeInstruction instruction = code[i];
            OpCode next = code[i + 1].opcode;
            if (instruction.LoadsConstant(4) && (next.FlowControl == FlowControl.Cond_Branch || next.Name?.StartsWith("stloc", StringComparison.Ordinal) == true))
            {
                instruction.opcode = configurable ? OpCodes.Call : OpCodes.Ldc_I4;
                instruction.operand = configurable ? AccessTools.PropertyGetter(typeof(WorkReworkMod), nameof(WorkReworkMod.MaximumPriority)) : PriorityRules.Maximum;
                replaced++;
            }
        }
        if (replaced != expected)
            throw new InvalidOperationException($"[Work Rework] Priority range patch expected {expected} constants, found {replaced}. Game API changed.");
        return code;
    }
}

[HarmonyPatch(typeof(Pawn_WorkSettings), nameof(Pawn_WorkSettings.SetPriority))]
internal static class PriorityRangePatch
{
    private static void Prefix(Pawn ___pawn, ref int priority)
    {
        WorkPriorityComponent.Current?.EnsureStandalone(___pawn);
        if (___pawn.RaceProps.Humanlike)
            priority = PriorityRules.InitialAssignment(priority, WorkReworkMod.MaximumPriority, PawnWorkInitializationScope.NewPawn, WorkReworkMod.InitialPriority);
    }
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => PriorityTranspilers.ExtendRange(instructions, 1, configurable: false);
    private static void Postfix(Pawn ___pawn, WorkTypeDef w)
    {
        WorkPriorityComponent.Current?.NotifyViewChanged();
        if (ParentGroupCommandScope.Depth > 0)
            WorkPriorityComponent.Current?.ResetCategory(___pawn, w);
    }
}

internal static class ParentGroupCommandScope
{
    [ThreadStatic] internal static int Depth;
}

[HarmonyPatch(typeof(WidgetsWork), nameof(WidgetsWork.DrawWorkBoxFor))]
internal static class PriorityBoxRangePatch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => PriorityTranspilers.ExtendRange(instructions, 2);
    private static void Prefix(out int __state) { __state = ParentGroupCommandScope.Depth; ParentGroupCommandScope.Depth++; }
    private static void Finalizer(int __state) => ParentGroupCommandScope.Depth = __state;
}

[HarmonyPatch(typeof(PawnColumnWorker_WorkPriority), "HeaderClicked")]
internal static class PriorityHeaderRangePatch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => PriorityTranspilers.ExtendRange(instructions, 2);
    private static void Prefix(out int __state) { __state = ParentGroupCommandScope.Depth; ParentGroupCommandScope.Depth++; }
    private static void Finalizer(int __state) => ParentGroupCommandScope.Depth = __state;
}

[HarmonyPatch(typeof(Pawn_WorkSettings), nameof(Pawn_WorkSettings.GetPriority))]
internal static class ParentSummaryPriorityPatch
{
    private static void Postfix(Pawn ___pawn, WorkTypeDef w, ref int __result)
    {
        if (___pawn.RaceProps.Humanlike && WorkPriorityComponent.Current is { } component && (component.HasOrganization || component.HasOverrides(___pawn)))
            __result = WorkPriorities.Summary(___pawn, w).Priority;
    }
}

[HarmonyPatch(typeof(Pawn_WorkSettings), "CacheWorkGiversInOrder")]
internal static class PriorityCachePatch
{
    private static bool Prefix(Pawn_WorkSettings __instance, Pawn ___pawn, List<WorkGiver> ___workGiversInOrderNormal,
        List<WorkGiver> ___workGiversInOrderEmerg, ref bool ___workGiversDirty)
    {
        if (!___pawn.RaceProps.Humanlike || WorkPriorityComponent.Current is not { } component || !component.HasOrganization && !component.HasOverrides(___pawn))
        {
            WorkPriorityPlanner.Forget(__instance);
            return true;
        }
        WorkPriorityPlan plan = WorkPriorityPlanner.Build(___pawn);
        ___workGiversInOrderNormal.Clear();
        ___workGiversInOrderNormal.AddRange(plan.Normal);
        ___workGiversInOrderEmerg.Clear();
        ___workGiversInOrderEmerg.AddRange(plan.Emergency);
        ___workGiversDirty = false;
        return false;
    }
}

[HarmonyPatch(typeof(JobGiver_Work), nameof(JobGiver_Work.TryIssueJobPackage))]
internal static class PriorityScannerBoundaryPatch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        FieldInfo field = AccessTools.Field(typeof(WorkGiverDef), nameof(WorkGiverDef.priorityInType));
        MethodInfo group = AccessTools.Method(typeof(WorkPriorityPlanner), nameof(WorkPriorityPlanner.ScanGroup));
        var result = new List<CodeInstruction>();
        int replaced = 0;
        foreach (CodeInstruction original in instructions)
        {
            var instruction = new CodeInstruction(original);
            if (instruction.LoadsField(field))
            {
                var pawn = new CodeInstruction(OpCodes.Ldarg_1);
                pawn.labels.AddRange(instruction.labels);
                pawn.blocks.AddRange(instruction.blocks);
                result.Add(pawn);
                result.Add(new CodeInstruction(OpCodes.Call, group));
                replaced++;
            }
            else result.Add(instruction);
        }
        if (replaced != 2)
            throw new InvalidOperationException($"[Work Rework] Scanner boundary patch expected 2 fields, found {replaced}. Game API changed.");
        return result;
    }
}

internal static class ChildPriorityClipboard
{
    internal static Dictionary<string, int>? Values;
}

[HarmonyPatch(typeof(PawnColumnWorker_CopyPasteWorkPriorities), "CopyFrom")]
internal static class CopyChildPrioritiesPatch
{
    private static void Postfix(Pawn p, DefMap<WorkTypeDef, int> ___clipboard)
    {
        ChildPriorityClipboard.Values = WorkPriorityComponent.Current?.Copy(p);
        // Copy the stored base, not the displayed aggregate, so inherited children
        // keep their values when the complete configuration is pasted elsewhere.
        foreach (WorkTypeDef type in DefDatabase<WorkTypeDef>.AllDefsListForReading)
            ___clipboard[type] = p.WorkTypeIsDisabled(type) ? 0 : WorkPriorities.Parent(p, type);
    }
}

[HarmonyPatch(typeof(PawnColumnWorker_CopyPasteWorkPriorities), "PasteTo")]
internal static class PasteChildPrioritiesPatch
{
    private static void Postfix(Pawn p)
    {
        if (ChildPriorityClipboard.Values != null)
            WorkPriorityComponent.Current?.Paste(p, ChildPriorityClipboard.Values);
    }
}

[HarmonyPatch(typeof(PawnColumnWorker_WorkPriority), nameof(PawnColumnWorker_WorkPriority.DoCell))]
internal static class ParentPriorityMenuPatch
{
    private static void Prefix(PawnColumnWorker_WorkPriority __instance, Rect rect, Pawn pawn, PawnTable table)
    {
        if (!WorkTableAccess.IsWork(table) || Event.current.type != EventType.mouseDown || Event.current.button != 2 ||
            pawn.Dead || pawn.workSettings == null || !pawn.workSettings.EverWork || pawn.WorkTypeIsDisabled(__instance.def.workType))
            return;
        Rect box = new(rect.center.x - 12.5f, rect.y + 2.5f, 25f, 25f);
        if (!Mouse.IsOver(box)) return;
        WorkPriorityMenus.Parent(pawn, __instance.def.workType);
        Event.current.Use();
    }

    private static void Postfix(PawnColumnWorker_WorkPriority __instance, Rect rect, Pawn pawn, PawnTable table)
    {
        if (WorkTableAccess.IsWork(table) && !pawn.Dead)
            ActiveWorkHighlight.Parent(new Rect(rect.center.x - 12.5f, rect.y + 2.5f, 25f, 25f), pawn, __instance.def.workType, table);
        if (!WorkTableAccess.IsWork(table) || pawn.Dead || pawn.workSettings == null || !pawn.workSettings.EverWork || pawn.WorkTypeIsDisabled(__instance.def.workType))
            return;
        Rect box = new(rect.center.x - 12.5f, rect.y + 2.5f, 25f, 25f);
        PrioritySummary summary = WorkPriorities.Summary(pawn, __instance.def.workType);
        if (!summary.Mixed) return;
        GameFont oldFont = Text.Font;
        TextAnchor oldAnchor = Text.Anchor;
        Color oldColor = GUI.color;
        try
        {
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
            Widgets.Label(new Rect(box.xMax - 7f, box.y - 2f, 9f, 16f), "*");
            TooltipHandler.TipRegion(box, "WR_ParentMixedTip".Translate());
        }
        finally { Text.Font = oldFont; Text.Anchor = oldAnchor; GUI.color = oldColor; }
    }
}
