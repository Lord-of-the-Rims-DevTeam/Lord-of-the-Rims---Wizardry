using System;
using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using AbilityUser;
using Verse.AI;

namespace Wizardry
{
    public class Nienna_WorkGiver_HealingTouch : WorkGiver_Tend
    {

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            bool result = false;
            Pawn pawn2 = t as Pawn;
            if (pawn.story.traits.HasTrait(WizardryDefOf.LotRW_Istari))
            {
                CompWizardry comp = pawn.GetComp<CompWizardry>();
                PawnAbility pawnAbility = comp.AbilityData.Powers.FirstOrDefault((PawnAbility x) => x.Def == WizardryDefOf.LotRW_Nienna_HealingTouch);
                if (pawn2 != null && (!this.def.tendToHumanlikesOnly || pawn2.RaceProps.Humanlike) && (!this.def.tendToAnimalsOnly || pawn2.RaceProps.Animal) && WorkGiver_Tend.GoodLayingStatusForTend(pawn2, pawn) && HasHediffInjuries(pawn2) && pawn != pawn2 && pawnAbility.CooldownTicksLeft <=0)
                {
                    LocalTargetInfo target = pawn2;
                    if (pawn.CanReserve(target, 1, -1, null, forced))
                    {
                        return true;
                    }
                }
            }
            return result;
        }

        public static bool HasHediffInjuries(Pawn pawn)
        {
            IEnumerable<Hediff_Injury> pawnInjuries = pawn.health.hediffSet.GetHediffs<Hediff_Injury>();
            return pawnInjuries.Count() > 0;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Pawn pawn2 = t as Pawn;
            return new Job(WizardryDefOf.JobDriver_HealingTouch, pawn2, pawn);
        }
    }
}
