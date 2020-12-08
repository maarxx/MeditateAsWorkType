using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace MeditateAsWorkType
{
    public class WorkGiver_MeditateForPsyfocus : WorkGiver
    {
        public override Job NonScanJob(Pawn pawn)
        {
            bool shouldMeditate = pawn.HasPsylink && pawn.psychicEntropy.CurrentPsyfocus < pawn.psychicEntropy.TargetPsyfocus;
            if (shouldMeditate && MeditationUtility.CanMeditateNow(pawn))
            {
                return MeditationUtility.GetMeditationJob(pawn);
            }
            return null;
        }
    }
}
