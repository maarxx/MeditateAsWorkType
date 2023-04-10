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
            bool shouldMeditate = MeditationFocusDefOf.Natural.CanPawnUse(pawn) && MeditationUtility.CanMeditateNow(pawn);
            if (shouldMeditate)
            {
                Job meditationJob = GetAnimaMeditationJob(pawn);
                Thing tree = meditationJob.targetC.Thing;
                if (tree != null && (tree.TryGetComp<DiminishingGrassComp>()?.IsCurrentPenaltyAllowable() ?? false))
                {
                    return meditationJob;
                }
            }
            return null;
        }

        public static Job GetAnimaMeditationJob(Pawn pawn, bool forJoy = false)
        {
            MeditationSpotAndFocus meditationSpotAndFocus = FindAnimaMeditationSpot(pawn);
            if (meditationSpotAndFocus.IsValid)
            {
                Building_Throne t;
                Job job;
                if ((t = (meditationSpotAndFocus.focus.Thing as Building_Throne)) != null)
                {
                    job = JobMaker.MakeJob(JobDefOf.Reign, t, null, t);
                }
                else
                {
                    JobDef def = JobDefOf.Meditate;
                    IdeoFoundation_Deity ideoFoundation_Deity;
                    if (forJoy && ModsConfig.IdeologyActive && pawn.Ideo != null && (ideoFoundation_Deity = (pawn.Ideo.foundation as IdeoFoundation_Deity)) != null && ideoFoundation_Deity.DeitiesListForReading.Any())
                    {
                        def = JobDefOf.MeditatePray;
                    }
                    job = JobMaker.MakeJob(def, meditationSpotAndFocus.spot, null, meditationSpotAndFocus.focus);
                }
                job.ignoreJoyTimeAssignment = !forJoy;
                return job;
            }
            return null;
        }

        public static MeditationSpotAndFocus FindAnimaMeditationSpot(Pawn pawn)
        {
            float num = float.MinValue;
            LocalTargetInfo spot = LocalTargetInfo.Invalid;
            LocalTargetInfo focus = LocalTargetInfo.Invalid;
            if (!ModLister.CheckRoyalty("Psyfocus"))
            {
                return new MeditationSpotAndFocus(spot, focus);
            }
            Room ownedRoom = pawn.ownership.OwnedRoom;
            foreach (LocalTargetInfo item in MeditationUtility.AllMeditationSpotCandidates(pawn))
            {
                if (MeditationUtility.SafeEnvironmentalConditions(pawn, item.Cell, pawn.Map) && item.Cell.Standable(pawn.Map) && !item.Cell.IsForbidden(pawn))
                {
                    float num2 = 1f / Mathf.Max(item.Cell.DistanceToSquared(pawn.Position), 0.1f);
                    LocalTargetInfo localTargetInfo = (item.Thing is Building_Throne) ? ((LocalTargetInfo)item.Thing) : MeditationUtility.BestFocusAt(item, pawn);
                    if (pawn.HasPsylink && localTargetInfo.IsValid)
                    {
                        num2 += localTargetInfo.Thing.GetStatValueForPawn(StatDefOf.MeditationFocusStrength, pawn) * 100f;
                    }
                    if (!pawn.HasPsylink && MeditationFocusDefOf.Natural.CanPawnUse(pawn))
                    {
                        if (localTargetInfo.Thing?.def?.defName == "Plant_TreeAnima")
                        {
                            num2 += 100f;
                            // TODO: Rank by Diminishing Penalty (Too Many Hours) and Grass Growth Multiplier (Buildings Nearby)
                        }
                    }
                    Room room = item.Cell.GetRoom(pawn.Map);
                    if (room != null && ownedRoom == room)
                    {
                        num2 += 1f;
                    }
                    Building building;
                    if (item.Thing != null && (building = (item.Thing as Building)) != null && building.GetAssignedPawn() == pawn)
                    {
                        num2 += (float)((building.def == ThingDefOf.MeditationSpot) ? 200 : 100);
                    }
                    if (room != null && ModsConfig.IdeologyActive && room.Role == RoomRoleDefOf.WorshipRoom)
                    {
                        num2 += 100f;
                        foreach (Thing containedAndAdjacentThing in room.ContainedAndAdjacentThings)
                        {
                            num2 += containedAndAdjacentThing.GetStatValue(StatDefOf.StyleDominance);
                        }
                    }
                    if (num2 > num)
                    {
                        spot = item;
                        focus = localTargetInfo;
                        num = num2;
                    }
                }
            }
            return new MeditationSpotAndFocus(spot, focus);
        }
    }
}
