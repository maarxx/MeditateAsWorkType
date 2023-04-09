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
            bool shouldMeditate = MeditationUtility.CanMeditateNow(pawn); // && is a tribal && withinDiminishingSlider;
            if (shouldMeditate)
            {
                Job meditationJob = MeditationUtility.GetMeditationJob(pawn);
                ThingDef animaTreeDef = DefDatabase<ThingDef>.GetNamed("Plant_TreeAnima");
                if (meditationJob.targetA.Thing.def == animaTreeDef || meditationJob.targetC.Thing.def == animaTreeDef)
                {
                    return meditationJob;
                }
            }
            return null;
        }
    }
}
