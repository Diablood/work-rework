using System;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace WorkRework;

public sealed class PawnColumnWorker_WorkGiverPreview : PawnColumnWorker
{
    private static readonly Action<Rect, Pawn, WorkTypeDef> DrawNativeBackground =
        AccessTools.MethodDelegate<Action<Rect, Pawn, WorkTypeDef>>(
            AccessTools.Method(typeof(WidgetsWork), "DrawWorkBoxBackground", new[] { typeof(Rect), typeof(Pawn), typeof(WorkTypeDef) }));
    private WorkGiverDef Giver => ((WorkGiverColumnDef)def).Giver;
    public override bool VisibleCurrently => def.workType.VisibleCurrently;
    public override int GetMinWidth(PawnTable table) => WorkReworkMod.VerticalLabels ? VerticalWorkHeaders.ColumnWidth : WorkCatalog.Standalone(Giver) ? 32 : 62;
    public override int GetOptimalWidth(PawnTable table) => WorkReworkMod.VerticalLabels ? VerticalWorkHeaders.ColumnWidth : WorkCatalog.Standalone(Giver) ? 39 : 72;
    public override int GetMaxWidth(PawnTable table) => WorkReworkMod.VerticalLabels ? VerticalWorkHeaders.ColumnWidth : WorkCatalog.Standalone(Giver) ? 80 : 90;
    public override int GetMinHeaderHeight(PawnTable table) => WorkReworkMod.VerticalLabels ? WorkTableView.For(table).VerticalHeaderHeight(table) : WorkCatalog.Standalone(Giver) ? 50 : 110;

    public override void DoHeader(Rect rect, PawnTable table)
    {
        if (WorkReworkMod.VerticalLabels)
        {
            VerticalWorkHeaders.DrawLabel(rect, WorkCatalog.TaskLabel(Giver), inherited: !WorkCatalog.Standalone(Giver));
            TooltipHandler.TipRegion(rect, (WorkCatalog.Standalone(Giver) ? "WR_StandaloneHeader" : "WR_ChildHeader").Translate(WorkCatalog.TaskLabel(Giver), WorkCatalog.CategoryLabel(def.workType)));
            return;
        }
        GameFont oldFont = Text.Font;
        TextAnchor oldAnchor = Text.Anchor;
        Color oldColor = GUI.color;
        try
        {
            if (WorkCatalog.Standalone(Giver))
            {
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleCenter;
                GUI.color = Color.white;
                Vector2 size = Text.CalcSize(WorkCatalog.TaskLabel(Giver));
                Rect label = new(rect.center.x - size.x / 2f, rect.y + (def.moveWorkTypeLabelDown ? 20f : 0f), size.x, size.y);
                Widgets.Label(label, WorkCatalog.TaskLabel(Giver));
                GUI.color = new Color(1f, 1f, 1f, 0.3f);
                Widgets.DrawLineVertical(label.center.x, label.yMax - 3f, rect.y + 50f - label.yMax + 3f);
                Widgets.DrawLineVertical(label.center.x + 1f, label.yMax - 3f, rect.y + 50f - label.yMax + 3f);
                TooltipHandler.TipRegion(label, "WR_StandaloneHeader".Translate(WorkCatalog.TaskLabel(Giver), WorkCatalog.CategoryLabel(def.workType)));
                return;
            }
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleCenter;
            GUI.color = Color.white;
            Widgets.Label(new Rect(rect.x + 3f, rect.y, rect.width - 6f, rect.height - 20f), WorkCatalog.TaskLabel(Giver));
            Text.Anchor = TextAnchor.LowerCenter;
            GUI.color = Color.gray;
            Widgets.Label(new Rect(rect.x, rect.yMax - 20f, rect.width, 18f), (WorkCatalog.Standalone(Giver) ? "WR_StandaloneMarker" : "WR_TaskMarker").Translate());
            TooltipHandler.TipRegion(rect, (WorkCatalog.Standalone(Giver) ? "WR_StandaloneHeader" : "WR_ChildHeader").Translate(WorkCatalog.TaskLabel(Giver), WorkCatalog.CategoryLabel(def.workType)));
        }
        finally
        {
            Text.Font = oldFont;
            Text.Anchor = oldAnchor;
            GUI.color = oldColor;
        }
    }

    public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
    {
        if (pawn.Dead || pawn.workSettings == null || !pawn.workSettings.EverWork)
            return;

        bool disabled = pawn.WorkTypeIsDisabled(def.workType) || pawn.WorkTagIsDisabled(Giver.workTags);
        bool lacksCapacity = false;
        foreach (PawnCapacityDef capacity in Giver.requiredCapacities)
            if (!pawn.health.capacities.CapableOf(capacity))
                lacksCapacity = true;

        Rect box = new(rect.center.x - 12.5f, rect.y + 2.5f, 25f, 25f);
        int? explicitPriority = WorkPriorities.Explicit(pawn, Giver);
        int raw = WorkPriorities.Raw(pawn, Giver);
        int priority = WorkPriorities.Effective(pawn, Giver);
        bool standalone = WorkCatalog.Standalone(Giver);
        bool parentDisabled = !standalone && WorkPriorities.Parent(pawn, def.workType) <= 0;
        bool manualPriorities = Find.PlaySettings.useWorkPriorities;
        GameFont oldFont = Text.Font;
        TextAnchor oldAnchor = Text.Anchor;
        Color oldColor = GUI.color;
        try
        {
            GUI.color = Color.white;
            if (standalone)
            {
                if (!disabled)
                {
                    if (lacksCapacity) GUI.color = new Color(1f, 0.3f, 0.3f);
                    DrawNativeBackground(box, pawn, def.workType);
                }
            }
            else Widgets.DrawBoxSolid(box, new Color(0.16f, 0.16f, 0.16f, 0.65f));
            Text.Font = standalone ? GameFont.Medium : GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            if (disabled || lacksCapacity && !standalone)
            {
                if (!standalone)
                {
                    GUI.color = Color.gray;
                    Widgets.Label(box, "—");
                }
            }
            else if (manualPriorities && raw > 0)
            {
                GUI.color = parentDisabled ? Color.gray : WidgetsWork.ColorOfPriority(raw);
                Widgets.Label(standalone ? box.ContractedBy(-3f) : box, raw.ToString());
            }
            else if (!manualPriorities && priority > 0)
            {
                GUI.color = Color.white;
                GUI.DrawTexture(box, WidgetsWork.WorkBoxCheckTex);
            }
            if (explicitPriority.HasValue && !standalone)
            {
                GUI.color = new Color(0.65f, 0.8f, 1f);
                Widgets.DrawBox(box);
            }
            ActiveWorkHighlight.Task(box, pawn, Giver);
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
            string value = manualPriorities ? raw.ToString() : (raw > 0 ? "WR_Enabled" : "WR_Disabled").Translate().ToString();
            string origin = (standalone ? "WR_StandaloneMarker" : explicitPriority.HasValue ? "WR_ExplicitPriority" : "WR_InheritedPriority").Translate().ToString();
            string tip = (standalone ? "WR_StandalonePriorityTip" : "WR_ChildPriorityTip").Translate(WorkCatalog.TaskLabel(Giver), WorkCatalog.CategoryLabel(def.workType), origin, value);
            if (parentDisabled)
                tip += "\n\n" + "WR_ParentBlocksChildren".Translate();
            if (disabled && manualPriorities)
                tip += "\n\n" + WidgetsWork.TipForPawnWorker(pawn, def.workType, lacksCapacity);
            else if (disabled)
            {
                tip += "\n\n" + "CannotDoThisWork".Translate(pawn.LabelShort, pawn);
                var reasons = pawn.GetReasonsForDisabledWorkType(def.workType);
                if (reasons != null && reasons.Count > 0)
                    tip += "\n" + string.Join(". ", reasons);
            }
            else if (lacksCapacity)
                tip += "\n\n" + "IncapableOfWorkTypeBecauseOfCapacities".Translate();
            TooltipHandler.TipRegion(rect, tip);
            if (!disabled && !lacksCapacity && !parentDisabled && Event.current.type == EventType.mouseDown && Mouse.IsOver(box))
            {
                Event current = Event.current;
                if (current.button == 1 && current.control)
                {
                    if (!standalone) WorkPriorityComponent.Current?.SetOverride(pawn, Giver, null);
                }
                else if (current.button == 2)
                    WorkPriorityMenus.Child(pawn, Giver);
                else if (current.button == 0 || current.button == 1)
                    WorkPriorityComponent.Current?.SetOverride(pawn, Giver, manualPriorities
                        ? PriorityRules.Cycle(raw, current.button == 0, WorkReworkMod.MaximumPriority) : raw > 0 ? 0 : 3);
                else return;
                current.Use();
            }
        }
        finally
        {
            Text.Font = oldFont;
            Text.Anchor = oldAnchor;
            GUI.color = oldColor;
        }
    }
}
