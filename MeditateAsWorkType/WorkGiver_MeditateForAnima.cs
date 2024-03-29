﻿using RimWorld;
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
                return meditationJob;
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
            foreach (LocalTargetInfo item in AllAnimaMeditationSpotCandidates(pawn))
            {
                if (pawn.CanReserveAndReach(item, PathEndMode.OnCell, pawn.NormalMaxDanger()) && MeditationUtility.SafeEnvironmentalConditions(pawn, item.Cell, pawn.Map) && item.Cell.Standable(pawn.Map) && !item.Cell.IsForbidden(pawn))
                {
                    float num2 = 1f / Mathf.Max(item.Cell.DistanceToSquared(pawn.Position), 0.1f);
                    LocalTargetInfo localTargetInfo = (item.Thing is Building_Throne) ? ((LocalTargetInfo)item.Thing) : MeditationUtility.BestFocusAt(item, pawn);
                    if (pawn.HasPsylink && localTargetInfo.IsValid)
                    {
                        num2 += localTargetInfo.Thing.GetStatValueForPawn(StatDefOf.MeditationFocusStrength, pawn) * 100f;
                    }
                    Room room = item.Cell.GetRoom(pawn.Map);
                    if (room != null && ownedRoom == room)
                    {
                        num2 += 1f;
                    }
                    Building building;
                    if (item.Thing != null && (building = (item.Thing as Building)) != null)
                    {
                        Pawn assignedPawn = building.GetAssignedPawn();
                        if (assignedPawn == null || assignedPawn == pawn)
                        {
                            num2 += (assignedPawn == null) ? 50 : 100;

                            if (building.def == ThingDefOf.MeditationSpot)
                            {
                                num2 += 100;
                            }
                        }
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

        public static IEnumerable<LocalTargetInfo> AllAnimaMeditationSpotCandidates(Pawn pawn, bool allowFallbackSpots = true)
        {
            Map map = pawn.Map;
            List<Thing> trees = map.listerThings.ThingsOfDef(ThingDefOf.Plant_TreeAnima);
            foreach (Thing tree in trees)
            {
                if (tree.TryGetComp<DiminishingGrassComp>()?.IsCurrentPenaltyAllowable() ?? false)
                {
                    foreach (IntVec3 cell in GenRadial.RadialCellsAround(tree.Position, MeditationUtility.FocusObjectSearchRadius, false))
                    {
                        if (cell.Standable(map))
                        {
                            Thing meditationSpot = cell.GetFirstThing(map, ThingDefOf.MeditationSpot);
                            if (meditationSpot != null)
                            {
                                yield return meditationSpot;
                            }
                            else
                            {
                                yield return cell;
                            }
                        }
                    }
                }
            }
        }
    }
}
