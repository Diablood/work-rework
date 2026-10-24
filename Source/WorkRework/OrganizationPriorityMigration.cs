using System.Collections.Generic;
using System.Linq;

namespace WorkRework;

internal sealed record OrganizationPriorityMigration(Dictionary<string, int> Overrides, Dictionary<string, int> ParentChanges)
{
    internal static OrganizationPriorityMigration Build(WorkLayout previous, WorkLayout next, IReadOnlyDictionary<string, int> parents, IReadOnlyDictionary<string, int> stored)
    {
        var result = new OrganizationPriorityMigration(new(stored), new());
        List<string> tasks = previous.OrderedTasks().ToList();
        List<string> changed = tasks.Where(t => previous.IsStandalone(t) != next.IsStandalone(t)).ToList();
        foreach (string origin in changed.Select(previous.Origin).Distinct())
        {
            int parent = parents[origin];
            var snapshots = tasks.Where(t => previous.Origin(t) == origin).ToDictionary(t => t,
                t => PriorityRules.Effective(previous.IsStandalone(t) ? 1 : parent, stored.TryGetValue(t, out int value) ? value : null, true));
            foreach (string task in changed.Where(t => previous.Origin(t) == origin)) result.Overrides[task] = snapshots[task];
            int minimum = next.Children[origin].Select(t => snapshots[t]).Where(v => v > 0).DefaultIfEmpty(0).Min();
            if (parent == 0 && minimum > 0)
            {
                result.ParentChanges[origin] = minimum;
                foreach (string task in next.Children[origin]) result.Overrides[task] = snapshots[task];
            }
        }
        return result;
    }
}
