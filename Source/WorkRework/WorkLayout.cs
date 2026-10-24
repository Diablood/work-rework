using System;
using System.Collections.Generic;
using System.Linq;

namespace WorkRework;

internal sealed record WorkCategory(string Id, IReadOnlyList<string> Tasks);

// Pure organization model. A task's origin comes exclusively from the catalog.
internal sealed class WorkLayout
{
    internal readonly List<string> Roots = new();
    internal readonly Dictionary<string, List<string>> Children = new();
    private readonly Dictionary<string, string> origins = new();
    internal static string Group(string id) => "G:" + id;
    internal static string Task(string id) => "T:" + id;
    internal bool IsStandalone(string task) => Roots.Contains(Task(task));
    internal string Origin(string task) => origins[task];

    internal WorkLayout(IReadOnlyList<WorkCategory> catalog, IEnumerable<string>? roots = null, IEnumerable<string>? childOrder = null)
    {
        foreach (WorkCategory category in catalog)
        {
            Children[category.Id] = new List<string>();
            foreach (string task in category.Tasks)
                origins.TryAdd(task, category.Id);
        }
        var seen = new HashSet<string>();
        foreach (string root in roots ?? Array.Empty<string>())
            if ((root.StartsWith("G:", StringComparison.Ordinal) && Children.ContainsKey(root[2..]) ||
                 root.StartsWith("T:", StringComparison.Ordinal) && origins.ContainsKey(root[2..])) && seen.Add(root))
                Roots.Add(root);
        foreach (WorkCategory category in catalog)
            if (seen.Add(Group(category.Id))) Roots.Add(Group(category.Id));
        seen.Clear();
        foreach (string task in (childOrder ?? Array.Empty<string>()).Concat(catalog.SelectMany(c => c.Tasks)))
            if (origins.TryGetValue(task, out string? origin) && !IsStandalone(task) && seen.Add(task))
                Children[origin].Add(task);
    }

    internal WorkLayout Clone() => new(Catalog(), Roots, Children.Values.SelectMany(c => c));
    private List<WorkCategory> Catalog() => Children.Keys.Select(id => new WorkCategory(id, origins.Where(p => p.Value == id).Select(p => p.Key).ToList())).ToList();

    internal bool Extract(string task)
    {
        if (!origins.TryGetValue(task, out string? origin) || !Children[origin].Remove(task)) return false;
        Roots.Insert(Roots.IndexOf(Group(origin)) + 1, Task(task));
        return true;
    }

    internal bool Reintegrate(string task, string target)
    {
        if (!origins.TryGetValue(task, out string? origin) || origin != target || !Roots.Remove(Task(task))) return false;
        Children[origin].Add(task);
        return true;
    }

    // Native ReorderableWidget supplies an insertion boundary in the original list.
    internal static void Move(List<string> items, int from, int insertion)
    {
        if (from < 0 || from >= items.Count || insertion < 0 || insertion > items.Count) return;
        string item = items[from];
        items.RemoveAt(from);
        items.Insert(insertion > from ? insertion - 1 : insertion, item);
    }

    internal IEnumerable<string> OrderedTasks()
    {
        foreach (string root in Roots)
            if (root.StartsWith("T:", StringComparison.Ordinal)) yield return root[2..];
            else foreach (string task in Children[root[2..]]) yield return task;
    }

    internal static int Compare(int priorityA, int orderA, int priorityB, int orderB)
    {
        int value = priorityA.CompareTo(priorityB);
        return value == 0 ? orderA.CompareTo(orderB) : value;
    }
}
