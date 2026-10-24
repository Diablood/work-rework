using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace WorkRework;

public sealed partial class WorkPriorityComponent
{
    private WorkOrganizationData? organization;
    private WorkLayout? layout;
    private readonly HashSet<Pawn> standaloneInitialized = new();
    internal int OrganizationRevision { get; private set; }
    internal bool HasOrganization => organization != null;
    internal WorkLayout Layout => layout ??= new WorkLayout(WorkCatalog.Categories, organization?.Roots, organization?.ChildOrder);

    public override void LoadedGame()
        => ApplyMaximumPriority();

    internal void ApplyMaximumPriority()
    {
        foreach (Pawn pawn in PawnsFinder.AllMapsWorldAndTemporary_Alive.Concat(overrides.Keys).Distinct()
            .Where(p => p.RaceProps.Humanlike && p.workSettings?.EverWork == true).ToList())
        {
            EnsureStandalone(pawn);
            foreach (WorkTypeDef type in DefDatabase<WorkTypeDef>.AllDefsListForReading)
            {
                int value = WorkPriorities.Parent(pawn, type);
                if (value > WorkReworkMod.MaximumPriority)
                    pawn.workSettings.SetPriority(type, pawn.WorkTypeIsDisabled(type) ? 0 : WorkReworkMod.MaximumPriority);
            }
            if (overrides.TryGetValue(pawn, out var record))
            {
                foreach (string key in record.Values.Keys.ToList())
                    record.Values[key] = PriorityRules.Limit(record.Values[key], WorkReworkMod.MaximumPriority);
                record.Summaries.Clear();
            }
            Dirty(pawn);
        }
    }

    internal void EnsureStandalone(Pawn pawn)
    {
        if (!HasOrganization || PawnWorkInitializationScope.Depth > 0 || Scribe.mode != LoadSaveMode.Inactive ||
            pawn.workSettings?.EverWork != true || !pawn.RaceProps.Humanlike || standaloneInitialized.Contains(pawn)) return;
        standaloneInitialized.Add(pawn);
        foreach (string root in Layout.Roots.Where(r => r.StartsWith("T:", System.StringComparison.Ordinal)))
        {
            WorkGiverDef? giver = WorkCatalog.Giver(root[2..]);
            if (giver == null || overrides.TryGetValue(pawn, out var record) && record.Values.ContainsKey(giver.defName)) continue;
            SetOverride(pawn, giver, WorkPriorities.Parent(pawn, giver.workType));
        }
    }

    internal void ApplyOrganization(WorkLayout draft)
    {
        // Normalize a detached copy before touching any pawn or the live layout.
        var next = new WorkLayout(WorkCatalog.Categories, draft.Roots, draft.Children.Values.SelectMany(c => c));
        var pawns = PawnsFinder.AllMapsWorldAndTemporary_Alive.Concat(overrides.Keys).Distinct()
            .Where(p => p.RaceProps.Humanlike && p.workSettings?.EverWork == true).ToList();
        var migrations = new Dictionary<Pawn, OrganizationPriorityMigration>();
        foreach (Pawn pawn in pawns)
        {
            EnsureStandalone(pawn);
            migrations[pawn] = OrganizationPriorityMigration.Build(Layout, next,
                WorkCatalog.Categories.ToDictionary(c => c.Id, c => WorkPriorities.Parent(pawn, WorkCatalog.Type(c.Id)!)), Copy(pawn));
        }
        layout = next;
        organization = new WorkOrganizationData { Roots = new(next.Roots), ChildOrder = next.Children.Values.SelectMany(c => c).ToList() };
        standaloneInitialized.Clear();
        foreach (Pawn pawn in pawns)
        {
            OrganizationPriorityMigration migration = migrations[pawn];
            if (migration.Overrides.Count > 0) overrides[pawn] = new PawnWorkPriorityOverrides { Values = migration.Overrides };
            else overrides.Remove(pawn);
            foreach (var change in migration.ParentChanges)
            {
                WorkTypeDef type = WorkCatalog.Type(change.Key)!;
                if (!pawn.WorkTypeIsDisabled(type)) pawn.workSettings.SetPriority(type, change.Value);
            }
            EnsureStandalone(pawn);
            Dirty(pawn);
        }
        OrganizationRevision++;
    }

    private void ExposeOrganization()
    {
        Scribe_Deep.Look(ref organization, "workOrganization");
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            layout = null;
            standaloneInitialized.Clear();
            OrganizationRevision++;
        }
    }
}
