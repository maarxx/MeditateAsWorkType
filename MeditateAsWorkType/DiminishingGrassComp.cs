using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MeditateAsWorkType
{
    public class DiminishingGrassComp : ThingComp
    {
        private ThingWithComps Tree => this.parent;
        public float diminishingGrassThreshold;

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref diminishingGrassThreshold, "diminishingGrassThreshold", 1.0f);
        }

        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref diminishingGrassThreshold, "diminishingGrassThreshold");
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
