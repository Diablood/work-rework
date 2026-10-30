using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace WorkRework;

internal static class PawnWorkInitializationScope
{
    [System.ThreadStatic] internal static int Depth;
    [System.ThreadStatic] internal static bool NewPawn;
}

internal static class StartingPawnPriorityScope
{
    [System.ThreadStatic] internal static List<Pawn>? Pawns;
}

internal static class PlayerFactionPriorityScope
{
    [System.ThreadStatic] internal static Pawn? Pawn;
}

[HarmonyPatch(typeof(Pawn), nameof(Pawn.SetFaction))]
internal static class PlayerFactionPrioritiesPatch
{
    private static void Prefix(Pawn __instance, Faction newFaction, out Pawn? __state)
    {
        __state = PlayerFactionPriorityScope.Pawn;
        // RecruitUtility clears guest status before SetFaction, so capture the
        // actual faction transition shared by recruitment and enslavement.
        if (__instance.RaceProps.Humanlike && newFaction?.IsPlayer == true && __instance.Faction != newFaction)
            PlayerFactionPriorityScope.Pawn = __instance;
    }

    private static void Finalizer(Pawn? __state) => PlayerFactionPriorityScope.Pawn = __state;
}

[HarmonyPatch(typeof(GameInitData), nameof(GameInitData.PrepForMapGen))]
internal static class StartingPawnPrioritiesPatch
{
    private static void Prefix(GameInitData __instance, out List<Pawn>? __state)
    {
        __state = StartingPawnPriorityScope.Pawns;
        // Keep the live list: vanilla removes optional, unselected pawns first.
        StartingPawnPriorityScope.Pawns = __instance.startingAndOptionalPawns;
    }

    private static void Finalizer(List<Pawn>? __state) => StartingPawnPriorityScope.Pawns = __state;
}

[HarmonyPatch(typeof(Pawn_WorkSettings), nameof(Pawn_WorkSettings.EnableAndInitialize))]
internal static class InitializeStandalonePrioritiesPatch
{
    private static void Prefix(Pawn_WorkSettings __instance, out (int Depth, bool NewPawn) __state)
    {
        __state = (PawnWorkInitializationScope.Depth, PawnWorkInitializationScope.NewPawn);
        PawnWorkInitializationScope.Depth++;
        PawnWorkInitializationScope.NewPawn |= !__instance.Initialized;
    }
    private static void Finalizer((int Depth, bool NewPawn) __state, Pawn ___pawn)
    {
        PawnWorkInitializationScope.Depth = __state.Depth;
        PawnWorkInitializationScope.NewPawn = __state.NewPawn;
        WorkPriorityComponent.Current?.EnsureStandalone(___pawn);
    }
}

[HarmonyPatch(typeof(Pawn_WorkSettings), nameof(Pawn_WorkSettings.WorkIsActive))]
internal static class StandaloneWorkActivePatch
{
    private static void Postfix(Pawn ___pawn, WorkTypeDef w, ref bool __result)
    {
        if (__result || !___pawn.RaceProps.Humanlike || ___pawn.WorkTypeIsDisabled(w) || WorkPriorityComponent.Current?.HasOrganization != true) return;
        foreach (WorkGiverDef giver in w.workGiversByPriority)
            if (WorkCatalog.Standalone(giver) && WorkPriorities.Effective(___pawn, giver) > 0) { __result = true; return; }
    }
}

[HarmonyPatch(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.Notify_WorkTypeDisabled))]
internal static class StandaloneJobProtectionPatch
{
    private static bool Prefix(Pawn_JobTracker __instance, Pawn ___pawn, WorkTypeDef wType)
    {
        if (!___pawn.RaceProps.Humanlike || ___pawn.WorkTypeIsDisabled(wType) || WorkPriorityComponent.Current?.HasOrganization != true ||
            !wType.workGiversByPriority.Exists(WorkCatalog.Standalone)) return true;
        // Keep the native hard-incapacity path; only a group-priority zero is narrowed.
        __instance.jobQueue.RemoveAll(___pawn, job => job.workGiverDef?.workType == wType && !job.playerForced && !WorkCatalog.Standalone(job.workGiverDef));
        Job? current = __instance.curJob;
        if (current?.workGiverDef?.workType == wType && !current.playerForced && !WorkCatalog.Standalone(current.workGiverDef))
            __instance.EndCurrentJob(JobCondition.InterruptForced);
        return false;
    }
}
