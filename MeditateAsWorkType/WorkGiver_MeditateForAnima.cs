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
            foreach (LocalTargetInfo item in AllAnimaMeditationSpotCandidates(pawn))
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
                            if (localTargetInfo.Thing.TryGetComp<DiminishingGrassComp>()?.IsCurrentPenaltyAllowable() ?? false)
                            {
                                num2 += 1000f;
                                // TODO: Rank by Diminishing Penalty (Too Many Hours) and Grass Growth Multiplier (Buildings Nearby)
                            }
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

        public static IEnumerable<LocalTargetInfo> AllAnimaMeditationSpotCandidates(Pawn pawn, bool allowFallbackSpots = true)
        {
            if (pawn.royalty != null && pawn.royalty.AllTitlesInEffectForReading.Count > 0 && !pawn.IsPrisonerOfColony)
            {
                Building_Throne building_Throne = RoyalTitleUtility.FindBestUsableThrone(pawn);
                if (building_Throne != null)
                {
                    yield return building_Throne;
                }
            }
            if (!pawn.IsPrisonerOfColony)
            {
                foreach (Building item in from s in pawn.Map.listerBuildings.AllBuildingsColonistOfDef(ThingDefOf.MeditationSpot)
                                          where MeditationUtility.IsValidMeditationBuildingForPawn(s, pawn)
                                          select s)
                {
                    yield return item;
                }
            }
            List<Thing> list = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.MeditationFocus);
            foreach (Thing item2 in list)
            {
                if (item2.def != ThingDefOf.Wall)
                {
                    Room room2 = item2.GetRoom();
                    if ((room2 == null || MeditationUtility.CanUseRoomToMeditate(room2, pawn)) && item2.GetStatValueForPawn(StatDefOf.MeditationFocusStrength, pawn) > 0f)
                    {
                        LocalTargetInfo localTargetInfo = MeditationUtility.MeditationSpotForFocus(item2, pawn);
                        if (localTargetInfo.IsValid)
                        {
                            yield return localTargetInfo;
                        }
                    }
                }
            }
            Building_Bed bed = pawn.ownership.OwnedBed;
            Room room3 = bed?.GetRoom();
            if (room3 != null && !room3.PsychologicallyOutdoors && pawn.CanReserveAndReach(bed, PathEndMode.OnCell, pawn.NormalMaxDanger()))
            {
                foreach (LocalTargetInfo item3 in MeditationUtility.FocusSpotsInTheRoom(pawn, room3))
                {
                    yield return item3;
                }
                IntVec3 c2 = RCellFinder.RandomWanderDestFor(pawn, bed.Position, 10f, delegate (Pawn p, IntVec3 c, IntVec3 r)
                {
                    if (c.Standable(p.Map) && c.GetDoor(p.Map) == null)
                    {
                        return WanderRoomUtility.IsValidWanderDest(p, c, r);
                    }
                    return false;
                }, pawn.NormalMaxDanger());
                if (c2.IsValid)
                {
                    yield return c2;
                }
            }
            foreach (Room room in MeditationUtility.UsableWorshipRooms(pawn))
            {
                foreach (LocalTargetInfo item4 in MeditationUtility.FocusSpotsInTheRoom(pawn, room))
                {
                    if (pawn.CanReach(item4, PathEndMode.Touch, pawn.NormalMaxDanger()))
                    {
                        yield return item4;
                    }
                }
                IntVec3 randomCell = room.Districts.RandomElement().Regions.RandomElement().RandomCell;
                IntVec3 c2 = RCellFinder.RandomWanderDestFor(pawn, randomCell, 10f, delegate (Pawn p, IntVec3 c, IntVec3 r)
                {
                    if (c.GetRoom(p.Map) == room && c.Standable(p.Map) && c.GetDoor(p.Map) == null)
                    {
                        return WanderRoomUtility.IsValidWanderDest(p, c, r);
                    }
                    return false;
                }, pawn.NormalMaxDanger());
                if (c2.IsValid)
                {
                    yield return c2;
                }
            }
            if (!pawn.IsPrisonerOfColony)
            {
                IntVec3 colonyWanderRoot = WanderUtility.GetColonyWanderRoot(pawn);
                IntVec3 c2 = RCellFinder.RandomWanderDestFor(pawn, colonyWanderRoot, 10f, delegate (Pawn p, IntVec3 c, IntVec3 r)
                {
                    if (!c.Standable(p.Map) || c.GetDoor(p.Map) != null || !p.CanReserveAndReach(c, PathEndMode.OnCell, p.NormalMaxDanger()))
                    {
                        return false;
                    }
                    Room room4 = c.GetRoom(p.Map);
                    if (room4 != null && !MeditationUtility.CanUseRoomToMeditate(room4, pawn))
                    {
                        return false;
                    }
                    return true;
                }, pawn.NormalMaxDanger());
                if (c2.IsValid)
                {
                    yield return c2;
                }
            }
        }
    }
}
