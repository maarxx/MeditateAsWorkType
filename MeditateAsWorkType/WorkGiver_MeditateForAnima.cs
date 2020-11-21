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
                Log.Message("GetAnimaMeditationJob has job");
                return job;
            }
            Log.Message("GetAnimaMeditationJob has NO job");
            return null;
        }

        public static MeditationSpotAndFocus FindAnimaMeditationSpot(Pawn pawn)
        {
            float num = float.MinValue;
            LocalTargetInfo spot = LocalTargetInfo.Invalid;
            LocalTargetInfo focus = LocalTargetInfo.Invalid;
            if (!ModLister.RoyaltyInstalled)
            {
                Log.ErrorOnce("Psyfocus meditation is a Royalty-specific game system. If you want to use this code please check ModLister.RoyaltyInstalled before calling it.", 657324);
                return new MeditationSpotAndFocus(spot, focus);
            }
            Room ownedRoom = pawn.ownership.OwnedRoom;
            foreach (LocalTargetInfo item in MeditationUtility.AllMeditationSpotCandidates(pawn))
            {
                if (MeditationUtility.SafeEnvironmentalConditions(pawn, item.Cell, pawn.Map))
                {
                    //LocalTargetInfo localTargetInfo = (item.Thing is Building_Throne) ? ((LocalTargetInfo)item.Thing) : MeditationUtility.BestFocusAt(item, pawn);
                    LocalTargetInfo localTargetInfo = MeditationUtility.BestFocusAt(item, pawn);
                    float num2 = 1f / Mathf.Max(item.Cell.DistanceToSquared(pawn.Position), 0.1f);
                    //if (pawn.HasPsylink && localTargetInfo.IsValid)
                    if (!(localTargetInfo.HasThing && localTargetInfo.Thing.def.defName == "Plant_TreeAnima"))
                    {
                        continue;
                    }
                    if (localTargetInfo.IsValid)
                    {
                        num2 += localTargetInfo.Thing.GetStatValueForPawn(StatDefOf.MeditationFocusStrength, pawn) * 100f;
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
                    if (!item.Cell.Standable(pawn.Map))
                    {
                        num2 = float.NegativeInfinity;
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
