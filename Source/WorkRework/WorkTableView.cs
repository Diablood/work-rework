using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using RimWorld;
using UnityEngine;
using Verse;

namespace WorkRework;

// Presentation only: no mutation of WorkTypeDef, WorkGiverDef or pawn work settings.
internal sealed class WorkTableView
{
    private static readonly ConditionalWeakTable<PawnTable, WorkTableView> Views = new();
    private readonly Dictionary<WorkGiverDef, PawnColumnDef> columns = new();
    private readonly HashSet<WorkTypeDef> expanded = new();
    private Game? game;
    private WorkTypeDef? pendingToggle;
    private bool verticalLabels;
    private int? verticalHeaderHeight;
    private object? measuredLanguage;
    private int organizationRevision = -1;
    private int priorityRevision = -1;

    internal Vector2 Scroll;
    internal bool HasExpanded => expanded.Count > 0;
    internal bool HasStandalone => WorkPriorityComponent.Current?.Layout.Roots.Any(r => r.StartsWith("T:", System.StringComparison.Ordinal)) == true;
    internal bool NeedsScroll { get; set; }
    internal static WorkTableView For(PawnTable table) => Views.GetValue(table, _ => new WorkTableView());
    internal bool IsExpanded(WorkTypeDef type) => expanded.Contains(type);
    internal void QueueToggle(WorkTypeDef type) => pendingToggle = type;

    internal bool ApplyPending()
    {
        bool changed = false;
        if (!ReferenceEquals(game, Current.Game))
        {
            game = Current.Game;
            expanded.Clear();
            pendingToggle = null;
            Scroll = Vector2.zero;
            changed = true;
        }
        if (pendingToggle != null)
        {
            if (!expanded.Add(pendingToggle))
                expanded.Remove(pendingToggle);
            pendingToggle = null;
            changed = true;
        }
        if (verticalLabels != WorkReworkMod.VerticalLabels)
        {
            verticalLabels = WorkReworkMod.VerticalLabels;
            Scroll = Vector2.zero;
            changed = true;
        }
        int revision = WorkPriorityComponent.Current?.OrganizationRevision ?? 0;
        if (revision != organizationRevision)
        {
            organizationRevision = revision;
            Scroll = Vector2.zero;
            changed = true;
        }
        int priorities = WorkPriorityComponent.Current?.PriorityRevision ?? 0;
        if (priorities != priorityRevision)
        {
            priorityRevision = priorities;
            changed = true;
        }
        if (changed)
            verticalHeaderHeight = null;
        return changed;
    }

    internal int VerticalHeaderHeight(PawnTable table)
    {
        if (verticalHeaderHeight.HasValue && ReferenceEquals(measuredLanguage, LanguageDatabase.activeLanguage))
            return verticalHeaderHeight.Value;
        float longest = 0f;
        foreach (PawnColumnDef column in table.Columns)
            if (column is WorkGiverColumnDef || column.Worker is PawnColumnWorker_WorkPriority)
                longest = Mathf.Max(longest, VerticalWorkHeaders.Measure(VerticalWorkHeaders.Label(column)));
        measuredLanguage = LanguageDatabase.activeLanguage;
        verticalHeaderHeight = Mathf.Clamp(Mathf.CeilToInt(longest) + 28, 110, 260);
        return verticalHeaderHeight.Value;
    }

    internal List<PawnColumnDef> Children(WorkTypeDef type)
        => WorkCatalog.Children(type).Select(Column).ToList();

    internal PawnColumnDef Column(WorkGiverDef giver)
    {
        if (!columns.TryGetValue(giver, out PawnColumnDef? result))
            columns[giver] = result = new WorkGiverColumnDef
            {
                defName = $"WorkRework_View_{giver.workType.defName}_{giver.defName}",
                label = WorkCatalog.TaskLabel(giver),
                workType = giver.workType,
                Giver = giver,
                workerClass = typeof(PawnColumnWorker_WorkGiverPreview),
                sortable = false
            };
        return result;
    }
}

// These are local view objects, never registered in DefDatabase or serialized.
public sealed class WorkGiverColumnDef : PawnColumnDef
{
    public WorkGiverDef Giver = null!;
}
