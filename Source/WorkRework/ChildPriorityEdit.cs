using System.Collections.Generic;
using System.Linq;

namespace WorkRework;

internal sealed record ChildPriorityEdit(int? ParentPriority, Dictionary<string, int?> Overrides)
{
    internal static ChildPriorityEdit Build(IReadOnlyCollection<string> attached, string task, int parent, int? value, int maximum)
    {
        int? priority = value.HasValue ? PriorityRules.Limit(value.Value, maximum) : null;
        if (parent > 0 || priority.GetValueOrDefault() == 0 || !attached.Contains(task))
            return new(null, new() { [task] = priority });

        // Raising a disabled parent must not enable inherited or dormant siblings.
        return new(priority, attached.ToDictionary(id => id, id => id == task ? priority : (int?)0));
    }
}
