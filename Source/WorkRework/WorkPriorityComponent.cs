using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace WorkRework;

public sealed class PawnWorkPriorityOverrides : IExposable
{
    internal Dictionary<string, int> Values = new();
    internal readonly Dictionary<WorkTypeDef, (int Parent, bool Manual, PrioritySummary Summary)> Summaries = new();

    public void ExposeData()
    {
        Scribe_Collections.Look(ref Values, "giverPriorities", LookMode.Value, LookMode.Value);
        Values ??= new Dictionary<string, int>();
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
            foreach (string key in Values.Keys.ToList())
                Values[key] = System.Math.Clamp(Values[key], 0, PriorityRules.Maximum);
    }
}

public sealed partial class WorkPriorityComponent : GameComponent
{
    private Dictionary<Pawn, PawnWorkPriorityOverrides> overrides = new();
    private List<Pawn>? pawnKeys;
    private List<PawnWorkPriorityOverrides>? pawnValues;

    public WorkPriorityComponent(Game game) { }

    internal static WorkPriorityComponent? Current => Verse.Current.Game?.GetComponent<WorkPriorityComponent>();
    internal bool HasOverrides(Pawn pawn) => overrides.TryGetValue(pawn, out var values) && values.Values.Count > 0;
    internal int? GetOverride(Pawn pawn, WorkGiverDef giver)
    {
        EnsureStandalone(pawn);
        return overrides.TryGetValue(pawn, out var values) && values.Values.TryGetValue(giver.defName, out int value) ? value : null;
    }

    internal void SetOverride(Pawn pawn, WorkGiverDef giver, int? value)
    {
        if (value.HasValue)
        {
            if (!overrides.TryGetValue(pawn, out var record))
                overrides.Add(pawn, record = new PawnWorkPriorityOverrides());
            record.Values[giver.defName] = PriorityRules.Limit(value.Value, WorkReworkMod.MaximumPriority);
        }
        else if (overrides.TryGetValue(pawn, out var record))
        {
            record.Values.Remove(giver.defName);
            if (record.Values.Count == 0)
                overrides.Remove(pawn);
        }
        Dirty(pawn);
        if (overrides.TryGetValue(pawn, out var changed)) changed.Summaries.Clear();
    }

    internal void ResetCategory(Pawn pawn, WorkTypeDef type)
    {
        foreach (WorkGiverDef giver in WorkCatalog.Children(type))
            SetOverride(pawn, giver, null);
    }

    internal Dictionary<string, int> Copy(Pawn pawn)
    {
        EnsureStandalone(pawn);
        return overrides.TryGetValue(pawn, out var record) ? new Dictionary<string, int>(record.Values) : new Dictionary<string, int>();
    }

    internal PrioritySummary Summary(Pawn pawn, WorkTypeDef type, int parent, bool manual)
    {
        int fallback = PriorityRules.Effective(parent, null, manual);
        if (HasOrganization && Layout.Children[type.defName].Count == 0 && type.workGiversByPriority.Count > 0)
            return new PrioritySummary(0, false);
        if (!overrides.TryGetValue(pawn, out var record)) return new PrioritySummary(fallback, false);
        if (record.Summaries.TryGetValue(type, out var cached) && cached.Parent == parent && cached.Manual == manual)
            return cached.Summary;
        var values = new List<int>();
        foreach (WorkGiverDef giver in WorkCatalog.Children(type))
            values.Add(PriorityRules.Effective(parent, record.Values.TryGetValue(giver.defName, out int value) ? value : null, manual));
        PrioritySummary summary = PriorityRules.Summarize(values, fallback);
        record.Summaries[type] = (parent, manual, summary);
        return summary;
    }

    internal void Paste(Pawn pawn, Dictionary<string, int> values)
    {
        overrides.Remove(pawn);
        foreach (var pair in values)
        {
            WorkGiverDef? giver = DefDatabase<WorkGiverDef>.GetNamedSilentFail(pair.Key);
            if (giver?.workType != null && !pawn.WorkTypeIsDisabled(giver.workType))
                SetOverride(pawn, giver, pair.Value);
        }
        standaloneInitialized.Remove(pawn);
        EnsureStandalone(pawn);
        Dirty(pawn);
    }

    internal int PriorityRevision { get; private set; }
    internal void NotifyViewChanged() => PriorityRevision++;
    private static void Dirty(Pawn pawn)
    {
        pawn.workSettings?.Notify_UseWorkPrioritiesChanged();
        Current?.NotifyViewChanged();
    }

    public override void ExposeData()
    {
        ExposeOrganization();
        Scribe_Collections.Look(ref overrides, "workPriorityOverrides", LookMode.Reference, LookMode.Deep, ref pawnKeys, ref pawnValues);
        overrides ??= new Dictionary<Pawn, PawnWorkPriorityOverrides>();
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            overrides = overrides.Where(pair => pair.Key != null && pair.Value != null).ToDictionary(pair => pair.Key, pair => pair.Value);
            foreach (Pawn pawn in overrides.Keys)
                Dirty(pawn);
        }
    }
}

internal static class WorkPriorities
{
    private static readonly AccessTools.FieldRef<Pawn_WorkSettings, DefMap<WorkTypeDef, int>> NativePriorities =
        AccessTools.FieldRefAccess<Pawn_WorkSettings, DefMap<WorkTypeDef, int>>("priorities");

    internal static int Parent(Pawn pawn, WorkTypeDef type) => pawn.workSettings != null && pawn.workSettings.EverWork
        ? NativePriorities(pawn.workSettings)[type] : 0;
    internal static int? Explicit(Pawn pawn, WorkGiverDef giver) => WorkPriorityComponent.Current?.GetOverride(pawn, giver);
    internal static int Raw(Pawn pawn, WorkGiverDef giver) => Explicit(pawn, giver) ?? Parent(pawn, giver.workType);
    internal static int Effective(Pawn pawn, WorkGiverDef giver) => PriorityRules.Effective(WorkCatalog.Standalone(giver) ? 1 : Parent(pawn, giver.workType), Explicit(pawn, giver), Find.PlaySettings.useWorkPriorities);
    internal static PrioritySummary Summary(Pawn pawn, WorkTypeDef type) => WorkPriorityComponent.Current?.Summary(pawn, type, Parent(pawn, type), Find.PlaySettings.useWorkPriorities)
        ?? new PrioritySummary(PriorityRules.Effective(Parent(pawn, type), null, Find.PlaySettings.useWorkPriorities), false);

    internal static void SetParent(Pawn pawn, WorkTypeDef type, int value)
    {
        pawn.workSettings.SetPriority(type, value);
        WorkPriorityComponent.Current?.ResetCategory(pawn, type);
    }
}
