using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

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

    //[HarmonyPatch]
    //class Patch_ToilFailConditions_FailOn
    //{
    //    static System.Reflection.MethodBase TargetMethod()
    //    {
    //        return typeof(ToilFailConditions).GetMethods().FirstOrDefault(
    //            x => x.Name.Equals("FailOn", StringComparison.OrdinalIgnoreCase) &&
    //            x.IsGenericMethod)?.MakeGenericMethod(typeof(Toil));
    //    }
    //    static bool Prefix(ref Func<bool> condition)
    //    {
    //        Log.Message("Hello from Patch_ToilFailConditions_FailOn: " + condition.ToString());
    //        return true;
    //    }
    //}

    [HarmonyPatch(typeof(MeditationUtility))]
    [HarmonyPatch("CanMeditateNow")]
    class Patch_MeditationUtility_CanMeditateNow
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
            //{
            //    return false;
            //}
            return false;
        }
    }
}
