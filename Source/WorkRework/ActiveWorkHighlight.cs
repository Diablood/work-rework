using RimWorld;
using UnityEngine;
using Verse;

namespace WorkRework;

internal static class ActiveWorkHighlight
{
    private static readonly Color ActiveColor = new(0.2f, 0.95f, 0.65f);

    internal static void Parent(Rect box, Pawn pawn, WorkTypeDef type, PawnTable table)
    {
        WorkGiverDef? active = pawn.CurJob?.workGiverDef;
        if (active != null && active.workType == type && !WorkCatalog.Standalone(active) && !WorkTableView.For(table).IsExpanded(type))
            Draw(box, active);
    }

    internal static void Task(Rect box, Pawn pawn, WorkGiverDef giver)
    {
        if (pawn.CurJob?.workGiverDef == giver) Draw(box, giver);
    }

    private static void Draw(Rect box, WorkGiverDef giver)
    {
        Color previous = GUI.color;
        try
        {
            // Outside the work box: keep the native skill background, passion,
            // priority color and any blue child-override border fully visible.
            GUI.color = ActiveColor;
            Widgets.DrawBox(box.ContractedBy(-2f), 2);
        }
        finally { GUI.color = previous; }
        TooltipHandler.TipRegion(box, "WR_CurrentWork".Translate(WorkCatalog.TaskLabel(giver)));
    }
}
