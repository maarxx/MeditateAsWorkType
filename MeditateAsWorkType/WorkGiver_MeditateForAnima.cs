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
            if (MeditationUtility.CanMeditateNow(pawn))
            {
                return GetAnimaMeditationJob(pawn);
            }
            return null;
        }

        public static Job GetAnimaMeditationJob(Pawn pawn)
        {
            MeditationSpotAndFocus meditationSpotAndFocus = FindAnimaMeditationSpot(pawn);
            if (meditationSpotAndFocus.IsValid)
            {
                Job job = JobMaker.MakeJob(JobDefOf.Meditate, meditationSpotAndFocus.spot, null, meditationSpotAndFocus.focus);
                job.ignoreJoyTimeAssignment = true;
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
            if (animaTree != null && animaTree.TryGetComp<DiminishingGrassComp>().IsCurrentPenaltyAllowable())
            {
                CompMeditationFocus compMeditationFocus = animaTree.TryGetComp<CompMeditationFocus>();
                if (compMeditationFocus != null && compMeditationFocus.CanPawnUse(pawn))
                {
                    if (pawn.HasPsylink && animaTree.GetStatValueForPawn(StatDefOf.MeditationFocusStrength, pawn) > float.Epsilon)
                    {
                        spot = MeditationUtility.MeditationSpotForFocus(animaTree, pawn);
                        focus = animaTree;
                        return new MeditationSpotAndFocus(spot, focus);
                    }
                    else
                    {
                        Area area = pawn.playerSettings.AreaRestriction;
                        IntVec3 c2 = RCellFinder.RandomWanderDestFor(
                            pawn,
                            animaTree.Position,
                            4,
                            delegate (Pawn p, IntVec3 c, IntVec3 r)
                            {
                                return c.Standable(p.Map) && c.GetDoor(p.Map) == null && (area == null || area[c]);
                            },
                            pawn.NormalMaxDanger()
                        );
                        if (c2.IsValid && (area == null || area[c2]))
                        {
                            return new MeditationSpotAndFocus(c2, null);
                        }
                    }
                }
            }
            return new MeditationSpotAndFocus(spot, focus);
        }
    }
}
