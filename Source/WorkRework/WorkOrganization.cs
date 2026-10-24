using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace WorkRework;

internal static class WorkCatalog
{
    private static List<WorkCategory>? categories;
    internal static IReadOnlyList<WorkCategory> Categories => categories ??= DefDatabase<WorkTypeDef>.AllDefsListForReading
        .OrderByDescending(type => type.naturalPriority)
        .Select(type => new WorkCategory(type.defName, type.workGiversByPriority.Where(g => g.workType == type).Select(g => g.defName).Distinct().ToList())).ToList();
    internal static WorkGiverDef? Giver(string id) => DefDatabase<WorkGiverDef>.GetNamedSilentFail(id);
    internal static WorkTypeDef? Type(string id) => DefDatabase<WorkTypeDef>.GetNamedSilentFail(id);
    // Work tab headers use labelShort. Mod/DLC WorkTypeDefs may omit Def.label.
    internal static string CategoryLabel(WorkTypeDef type) => !string.IsNullOrWhiteSpace(type.labelShort)
        ? type.labelShort.CapitalizeFirst()
        : !string.IsNullOrWhiteSpace(type.label) ? type.LabelCap.ToString() : type.defName;
    internal static string TaskLabel(WorkGiverDef giver)
    {
        string label = giver.LabelCap.ToString();
        if (!string.IsNullOrWhiteSpace(label)) return label;
        // Some loaded defs/translations have no label but still supply job text.
        if (!string.IsNullOrWhiteSpace(giver.verb)) return giver.verb.CapitalizeFirst();
        if (!string.IsNullOrWhiteSpace(giver.gerund)) return giver.gerund.CapitalizeFirst();
        return giver.defName;
    }
    internal static bool Standalone(WorkGiverDef giver) => WorkPriorityComponent.Current?.Layout.IsStandalone(giver.defName) == true;
    internal static IEnumerable<WorkGiverDef> Children(WorkTypeDef type) => WorkPriorityComponent.Current is { } component
        ? component.Layout.Children[type.defName].Select(Giver).OfType<WorkGiverDef>() : type.workGiversByPriority;
}

public sealed class WorkOrganizationData : IExposable
{
    internal List<string> Roots = new();
    internal List<string> ChildOrder = new();
    public void ExposeData()
    {
        Scribe_Collections.Look(ref Roots, "roots", LookMode.Value);
        Scribe_Collections.Look(ref ChildOrder, "childOrder", LookMode.Value);
        Roots ??= new();
        ChildOrder ??= new();
    }
}
