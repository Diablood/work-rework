using HarmonyLib;
using UnityEngine;
using Verse;

namespace WorkRework;

public sealed class WorkReworkMod : Mod
{
    internal const string PackageId = "diablood.workrework";

    internal Harmony HarmonyInstance { get; }
    private static WorkReworkSettings? settings;
    internal static bool VerticalLabels => settings?.VerticalLabels ?? false;
    internal static int MaximumPriority => settings?.MaximumPriority ?? 9;
    internal static int InitialPriority => settings?.InitialPriority ?? 3;
    private int draftMaximum;
    private int draftInitial;
    private string maximumBuffer;
    private string initialBuffer;

    public WorkReworkMod(ModContentPack content) : base(content)
    {
        settings = GetSettings<WorkReworkSettings>();
        draftMaximum = settings.MaximumPriority;
        draftInitial = settings.InitialPriority;
        maximumBuffer = draftMaximum.ToString();
        initialBuffer = draftInitial.ToString();
        HarmonyInstance = new Harmony(PackageId);
        HarmonyInstance.PatchAll(typeof(WorkReworkMod).Assembly);
        LongEventHandler.ExecuteWhenFinished(GroupedPawnsCompatibility.Initialize);
        string version = typeof(WorkReworkMod).Assembly.GetName().Version!.ToString(3);
        Log.Message($"[Work Rework {version}] Loaded. Configurable work priorities enabled.");
    }

    public override string SettingsCategory() => "Work Rework";

    public override void DoSettingsWindowContents(Rect inRect)
    {
        if (settings == null)
            return;
        bool previous = settings.VerticalLabels;
        var listing = new Listing_Standard();
        listing.Begin(inRect);
        listing.CheckboxLabeled("WR_VerticalLabels".Translate(), ref settings.VerticalLabels, "WR_VerticalLabelsTip".Translate());
        listing.Gap();
        listing.Label("WR_MaximumPriority".Translate(), tooltip: "WR_MaximumPriorityTip".Translate());
        Widgets.TextFieldNumeric(listing.GetRect(30f), ref draftMaximum, ref maximumBuffer, 1f, PriorityRules.Maximum);
        listing.Label("WR_InitialPriority".Translate(), tooltip: "WR_InitialPriorityTip".Translate());
        Widgets.TextFieldNumeric(listing.GetRect(30f), ref draftInitial, ref initialBuffer, 1f, draftMaximum);
        if (listing.ButtonText("WR_ApplyPrioritySettings".Translate()))
        {
            int previousMaximum = settings.MaximumPriority;
            settings.MaximumPriority = System.Math.Clamp(draftMaximum, 1, PriorityRules.Maximum);
            settings.InitialPriority = System.Math.Clamp(draftInitial, 1, settings.MaximumPriority);
            draftMaximum = settings.MaximumPriority;
            draftInitial = settings.InitialPriority;
            maximumBuffer = draftMaximum.ToString();
            initialBuffer = draftInitial.ToString();
            WriteSettings();
            if (previousMaximum != settings.MaximumPriority) WorkPriorityComponent.Current?.ApplyMaximumPriority();
        }
        listing.Gap();
        if (WorkPriorityComponent.Current is { } component)
        {
            if (listing.ButtonText("WR_Organize".Translate())) Find.WindowStack.Add(new Dialog_WorkOrganization(component));
        }
        else listing.Label("WR_OrganizationNeedsGame".Translate());
        listing.End();
        if (previous != settings.VerticalLabels)
            WriteSettings();
    }
}
