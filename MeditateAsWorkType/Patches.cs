using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MeditateAsWorkType
{
    [StaticConstructorOnStartup]
    class Main
    {
        static Main()
        {
            //Log.Message("Hello from Harmony in scope: com.github.harmony.rimworld.maarx.meditateasworktype");
            var harmony = new Harmony("com.github.harmony.rimworld.maarx.meditateasworktype");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [HarmonyPatch(typeof(MeditationUtility))]
    [HarmonyPatch("CanMeditateNow")]
    class Patch_MeditationUtility_CanMeditateNow
    {
        public static bool Prefix(Pawn pawn, ref bool __result)
        {
            //Log.Message("Hello from meditateasworktype CanMeditateNow");
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
            //{
            //    return false;
            //}
            return false;
        }
    }
}
