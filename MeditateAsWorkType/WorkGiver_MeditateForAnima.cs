using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MeditateAsWorkType
{
    public class WorkGiver_MeditateForAnima : WorkGiver
    {
        public override Job NonScanJob(Pawn pawn)
        {
            return GetAnimaMeditationJob(pawn);
        }

        public static Job GetAnimaMeditationJob(Pawn pawn, bool forJoy = false)
        {
            MeditationSpotAndFocus meditationSpotAndFocus = FindAnimaMeditationSpot(pawn);
            if (meditationSpotAndFocus.IsValid)
            {
                Job job = JobMaker.MakeJob(JobDefOf.Meditate, meditationSpotAndFocus.spot, null, meditationSpotAndFocus.focus);
                job.ignoreJoyTimeAssignment = !forJoy;
                return job;
            }
            return null;
        }

        public static MeditationSpotAndFocus FindAnimaMeditationSpot(Pawn pawn)
        {
            LocalTargetInfo spot = LocalTargetInfo.Invalid;
            LocalTargetInfo focus = LocalTargetInfo.Invalid;
            if (!ModLister.RoyaltyInstalled)
            {
                Log.ErrorOnce("Psyfocus meditation is a Royalty-specific game system. If you want to use this code please check ModLister.RoyaltyInstalled before calling it.", 657324);
                return new MeditationSpotAndFocus(spot, focus);
            }
            Thing animaTree = pawn.Map.listerThings.ThingsOfDef(DefDatabase<ThingDef>.GetNamed("Plant_TreeAnima")).FirstOrDefault();
            if (animaTree != null)
            {
                CompMeditationFocus compMeditationFocus = animaTree.TryGetComp<CompMeditationFocus>();
                if (compMeditationFocus != null && compMeditationFocus.CanPawnUse(pawn))
                {
                    spot = MeditationUtility.MeditationSpotForFocus(animaTree, pawn);
                    focus = animaTree;
                    return new MeditationSpotAndFocus(spot, focus);
                }
            }
            return new MeditationSpotAndFocus(spot, focus);
        }
    }
}
