using RimWorld;
using Verse;
using Verse.AI;

namespace MeditateAsWorkType;

public class WorkGiver_MeditateForPsyfocus : WorkGiver
{
    public override Job NonScanJob(Pawn pawn)
    {
        var shouldMeditate =
            pawn.HasPsylink && pawn.psychicEntropy.CurrentPsyfocus < pawn.psychicEntropy.TargetPsyfocus;
        if (shouldMeditate && MeditationUtility.CanMeditateNow(pawn))
        {
            return MeditationUtility.GetMeditationJob(pawn);
        }

        return null;
    }
}