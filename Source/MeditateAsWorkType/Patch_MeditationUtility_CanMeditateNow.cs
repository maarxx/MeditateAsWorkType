using HarmonyLib;
using RimWorld;
using Verse;

namespace MeditateAsWorkType;

[HarmonyPatch(typeof(MeditationUtility))]
[HarmonyPatch("CanMeditateNow")]
internal class Patch_MeditationUtility_CanMeditateNow
{
    public static bool Prefix(Pawn pawn, ref bool __result)
    {
        __result = true;
        if (pawn.needs.rest != null && (int)pawn.needs.rest.CurCategory >= 2)
        {
            __result = false;
        }

        if (pawn.needs.food.Starving)
        {
            __result = false;
        }

        if (!pawn.Awake())
        {
            __result = false;
        }

        //if (pawn.health.hediffSet.BleedRateTotal > 0f || (HealthAIUtility.ShouldSeekMedicalRest(pawn) && pawn.timetable?.CurrentAssignment != TimeAssignmentDefOf.Meditate) || HealthAIUtility.ShouldSeekMedicalRestUrgent(pawn))
        if (pawn.health.hediffSet.HasTendableHediff())
        {
            __result = false;
        }

        return false;
    }
}