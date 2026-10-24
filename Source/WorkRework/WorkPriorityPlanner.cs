using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;
using RimWorld;
using Verse;

namespace WorkRework;

internal sealed class WorkPriorityPlan
{
    internal readonly List<WorkGiver> Normal = new();
    internal readonly List<WorkGiver> Emergency = new();
    internal readonly Dictionary<WorkGiverDef, int> ScanGroups = new();
}

internal static class WorkPriorityPlanner
{
    private static readonly ConditionalWeakTable<Pawn_WorkSettings, WorkPriorityPlan> Plans = new();
    private sealed record Candidate(WorkGiverDef Def, int Priority, int TypeIndex, int GiverIndex);

    internal static void Forget(Pawn_WorkSettings settings) => Plans.Remove(settings);

    internal static WorkPriorityPlan Build(Pawn pawn)
    {
        var candidates = new List<Candidate>();
        WorkPriorityComponent? component = WorkPriorityComponent.Current;
        component?.EnsureStandalone(pawn);
        Dictionary<string, int>? ranks = component?.HasOrganization == true
            ? component.Layout.OrderedTasks().Select((id, rank) => (id, rank)).ToDictionary(p => p.id, p => p.rank) : null;
        int bestNormal = int.MaxValue;
        List<WorkTypeDef> types = DefDatabase<WorkTypeDef>.AllDefsListForReading;
        for (int i = 0; i < types.Count; i++)
        {
            WorkTypeDef type = types[i];
            if (pawn.WorkTypeIsDisabled(type))
                continue;
            for (int j = 0; j < type.workGiversByPriority.Count; j++)
            {
                WorkGiverDef giver = type.workGiversByPriority[j];
                int priority = WorkPriorities.Effective(pawn, giver);
                if (priority <= 0)
                    continue;
                candidates.Add(new Candidate(giver, priority, i, j));
                if (!giver.emergency)
                    bestNormal = Math.Min(bestNormal, priority);
            }
        }
        candidates.Sort((a, b) => ranks != null
            ? WorkLayout.Compare(a.Priority, ranks[a.Def.defName], b.Priority, ranks[b.Def.defName])
            : PriorityRules.Compare(a.Priority, a.Def.workType.naturalPriority, a.TypeIndex, a.GiverIndex,
                b.Priority, b.Def.workType.naturalPriority, b.TypeIndex, b.GiverIndex));
        var plan = new WorkPriorityPlan();
        var groups = new Dictionary<(int, int), int>();
        foreach (Candidate candidate in candidates)
        {
            // Preserve native scanner grouping at the same numeric priority, while
            // preventing a pending target from crossing into a lower-priority tier.
            var key = (candidate.Priority, ranks != null ? ranks[candidate.Def.defName] : candidate.Def.priorityInType);
            if (!groups.TryGetValue(key, out int group))
                groups.Add(key, group = groups.Count + 1);
            plan.ScanGroups[candidate.Def] = group;
            var list = PriorityRules.InEmergencyPass(candidate.Def.emergency, candidate.Priority, bestNormal) ? plan.Emergency : plan.Normal;
            list.Add(candidate.Def.Worker);
        }
        Plans.Remove(pawn.workSettings);
        Plans.Add(pawn.workSettings, plan);
        return plan;
    }

    internal static int ScanGroup(WorkGiverDef giver, Pawn pawn) => pawn.workSettings != null && Plans.TryGetValue(pawn.workSettings, out var plan) && plan.ScanGroups.TryGetValue(giver, out int key)
        ? key : giver.priorityInType;
}
