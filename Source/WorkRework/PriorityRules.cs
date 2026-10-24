using System;
using System.Collections.Generic;

namespace WorkRework;

internal static class PriorityRules
{
    internal const int Maximum = 9;

    internal static int Effective(int parent, int? child, bool manual)
    {
        if (parent <= 0)
            return 0;
        int value = Math.Clamp(child ?? parent, 0, Maximum);
        return !manual && value > 0 ? 3 : value;
    }

    internal static int Limit(int value, int maximum) => Math.Clamp(value, 0, Math.Clamp(maximum, 1, Maximum));
    internal static int InitialAssignment(int value, int maximum, bool newPawn, int initialPriority)
        => Limit(newPawn && value == 3 ? Math.Clamp(initialPriority, 1, Math.Clamp(maximum, 1, Maximum)) : value, maximum);
    internal static int Cycle(int value, bool decrease, int maximum = Maximum)
    {
        maximum = Math.Clamp(maximum, 1, Maximum);
        return (Limit(value, maximum) + (decrease ? maximum : 1)) % (maximum + 1);
    }

    internal static int Compare(int priorityA, int naturalA, int typeA, int giverA, int priorityB, int naturalB, int typeB, int giverB)
    {
        int result = priorityA.CompareTo(priorityB);
        if (result == 0) result = naturalB.CompareTo(naturalA);
        if (result == 0) result = typeA.CompareTo(typeB);
        if (result == 0) result = giverA.CompareTo(giverB);
        return result;
    }

    internal static bool InEmergencyPass(bool emergency, int priority, int bestNormal) => emergency && priority > 0 && priority <= bestNormal;

    internal static PrioritySummary Summarize(IEnumerable<int> values, int fallback)
    {
        int minimum = int.MaxValue;
        int? first = null;
        bool mixed = false;
        foreach (int value in values)
        {
            if (first.HasValue && first.Value != value) mixed = true;
            first ??= value;
            if (value > 0) minimum = Math.Min(minimum, value);
        }
        return new PrioritySummary(first.HasValue ? minimum == int.MaxValue ? 0 : minimum : fallback, mixed);
    }
}

internal readonly record struct PrioritySummary(int Priority, bool Mixed);
