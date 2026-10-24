using System.Collections.Generic;
using RimWorld;
using Verse;

namespace WorkRework;

internal static class WorkPriorityMenus
{
    private static string GroupDefault(Pawn pawn, WorkTypeDef type)
    {
        int value = WorkPriorities.Parent(pawn, type);
        return Find.PlaySettings.useWorkPriorities ? value.ToString() : (value > 0 ? "WR_Enabled" : "WR_Disabled").Translate().ToString();
    }
    internal static void Parent(Pawn pawn, WorkTypeDef type)
    {
        var options = new List<FloatMenuOption>();
        if (Find.PlaySettings.useWorkPriorities)
            for (int i = 0; i <= WorkReworkMod.MaximumPriority; i++)
            {
                int value = i;
                options.Add(new FloatMenuOption(value == 0 ? "WR_Disabled".Translate().ToString() : "WR_PriorityValue".Translate(value).ToString(),
                    () => WorkPriorities.SetParent(pawn, type, value)));
            }
        else
        {
            options.Add(new FloatMenuOption("WR_Enabled".Translate(), () => WorkPriorities.SetParent(pawn, type, 3)));
            options.Add(new FloatMenuOption("WR_Disabled".Translate(), () => WorkPriorities.SetParent(pawn, type, 0)));
        }
        options.Add(new FloatMenuOption("WR_ResetCategoryChildren".Translate(type.LabelCap, GroupDefault(pawn, type)),
            () => WorkPriorityComponent.Current?.ResetCategory(pawn, type)));
        Find.WindowStack.Add(new FloatMenu(options));
    }

    internal static void Child(Pawn pawn, WorkGiverDef giver)
    {
        var options = new List<FloatMenuOption>();
        if (!WorkCatalog.Standalone(giver))
            options.Add(new FloatMenuOption("WR_UseParentPriority".Translate(giver.workType.LabelCap, GroupDefault(pawn, giver.workType)), () => WorkPriorityComponent.Current?.SetOverride(pawn, giver, null)));
        if (Find.PlaySettings.useWorkPriorities)
        {
            for (int i = 0; i <= WorkReworkMod.MaximumPriority; i++)
            {
                int value = i;
                string label = value == 0 ? "WR_Disabled".Translate().ToString() : "WR_PriorityValue".Translate(value).ToString();
                options.Add(new FloatMenuOption(label, () => WorkPriorityComponent.Current?.SetOverride(pawn, giver, value)));
            }
        }
        else
        {
            options.Add(new FloatMenuOption("WR_Enabled".Translate(), () => WorkPriorityComponent.Current?.SetOverride(pawn, giver, 3)));
            options.Add(new FloatMenuOption("WR_Disabled".Translate(), () => WorkPriorityComponent.Current?.SetOverride(pawn, giver, 0)));
        }
        if (!WorkCatalog.Standalone(giver))
            options.Add(new FloatMenuOption("WR_ResetCategoryChildren".Translate(giver.workType.LabelCap, GroupDefault(pawn, giver.workType)),
                () => WorkPriorityComponent.Current?.ResetCategory(pawn, giver.workType)));
        Find.WindowStack.Add(new FloatMenu(options));
    }
}
