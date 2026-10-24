using Verse;

namespace WorkRework;

public sealed class WorkReworkSettings : ModSettings
{
    public bool VerticalLabels;
    public int MaximumPriority = 9;
    public int InitialPriority = 3;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref VerticalLabels, "verticalLabels", false);
        Scribe_Values.Look(ref MaximumPriority, "maximumPriority", 9);
        Scribe_Values.Look(ref InitialPriority, "initialPriority", 3);
        MaximumPriority = System.Math.Clamp(MaximumPriority, 1, PriorityRules.Maximum);
        InitialPriority = System.Math.Clamp(InitialPriority, 1, MaximumPriority);
    }
}
