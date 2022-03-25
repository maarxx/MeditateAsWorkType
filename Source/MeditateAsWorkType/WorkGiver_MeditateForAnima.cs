using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace MeditateAsWorkType;

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
        var meditationSpotAndFocus = FindAnimaMeditationSpot(pawn);
        if (!meditationSpotAndFocus.IsValid)
        {
            return null;
        }

        var job = JobMaker.MakeJob(JobDefOf.Meditate, meditationSpotAndFocus.spot, null,
            meditationSpotAndFocus.focus);
        job.ignoreJoyTimeAssignment = true;
        return job;
    }

    public static MeditationSpotAndFocus FindAnimaMeditationSpot(Pawn pawn)
    {
        var spot = LocalTargetInfo.Invalid;
        var focus = LocalTargetInfo.Invalid;
        if (!ModLister.RoyaltyInstalled)
        {
            Log.ErrorOnce(
                "Psyfocus meditation is a Royalty-specific game system. If you want to use this code please check ModLister.RoyaltyInstalled before calling it.",
                657324);
            return new MeditationSpotAndFocus(spot, focus);
        }

        var animaTree = pawn.Map.listerThings.ThingsOfDef(DefDatabase<ThingDef>.GetNamed("Plant_TreeAnima"))
            .FirstOrDefault();
        if (animaTree == null)
        {
            return new MeditationSpotAndFocus(spot, focus);
        }

        var compMeditationFocus = animaTree.TryGetComp<CompMeditationFocus>();
        if (compMeditationFocus == null || !compMeditationFocus.CanPawnUse(pawn))
        {
            return new MeditationSpotAndFocus(spot, focus);
        }

        if (pawn.HasPsylink && animaTree.GetStatValueForPawn(StatDefOf.MeditationFocusStrength, pawn) >
            float.Epsilon)
        {
            spot = MeditationUtility.MeditationSpotForFocus(animaTree, pawn);
            focus = animaTree;
            return new MeditationSpotAndFocus(spot, focus);
        }

        var area = pawn.playerSettings.AreaRestriction;
        var c2 = RCellFinder.RandomWanderDestFor(
            pawn,
            animaTree.Position,
            4,
            (p, c, _) => c.Standable(p.Map) && c.GetDoor(p.Map) == null && (area == null || area[c]),
            pawn.NormalMaxDanger()
        );
        if (c2.IsValid && (area == null || area[c2]))
        {
            return new MeditationSpotAndFocus(c2, null);
        }

        return new MeditationSpotAndFocus(spot, focus);
    }
}