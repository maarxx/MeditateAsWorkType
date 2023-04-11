using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MeditateAsWorkType
{
    public class DiminishingGrassComp : ThingComp
    {
        private ThingWithComps Tree => this.parent;
        public float allowableProgressPenalty = 1.0f;

        public float currentProgressPenalty
        {
            get
            {
                CompSpawnSubplant compSpawnSubplant = Tree.TryGetComp<CompPsylinkable>().CompSubplant;
                return 1.0f - (float)(compSpawnSubplant.GetType().GetProperty("ProgressMultiplier", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(compSpawnSubplant));
            }
        }

        public bool IsCurrentPenaltyAllowable()
        {
            return currentProgressPenalty < allowableProgressPenalty;
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref allowableProgressPenalty, "allowableProgressPenalty", 1.0f);
        }

        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref allowableProgressPenalty, "allowableProgressPenalty", 1.0f);
        }

        public override void CompTickRare()
        {
            //
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo item in base.CompGetGizmosExtra())
            {
                yield return item;
            }
            if (true)
            {
                yield return new DiminishingGrassGizmo(this);
            }
        }
    }
}
