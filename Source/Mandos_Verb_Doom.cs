using System;
using AbilityUser;
using Verse;
using RimWorld;
using Verse.AI;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Harmony;

namespace Wizardry
{
    public class Mandos_Verb_Doom : Verb_UseAbility
    {
        private LocalTargetInfo action = new LocalTargetInfo();
        bool validTarg;

        public override bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ)
        {
            if (targ.IsValid && targ.CenterVector3.InBounds(base.CasterPawn.Map) && !targ.Cell.Fogged(base.CasterPawn.Map) && targ.Cell.Walkable(base.CasterPawn.Map))
            {
                if ((root - targ.Cell).LengthHorizontal < this.verbProps.range)
                {
                    validTarg = true;
                }
                else
                {
                    //out of range
                    validTarg = false;
                }
            }
            else
            {
                validTarg = false;
            }
            return validTarg;
        }

        protected override bool TryCastShot()
        {
            bool flag = true;
            if(this.currentTarget.Thing != null)
            { 
                Pawn targetPawn = this.currentTarget.Thing as Pawn;
                if (targetPawn != null)
                {
                    HealthUtility.AdjustSeverity(targetPawn, HediffDef.Named("LotRW_DoomHD"), 1f);
                    for(int i = 0; i<4; i++)
                    {
                        EffectMaker.MakeEffect(ThingDef.Named("Mote_BlackSmoke"), targetPawn.DrawPos, targetPawn.Map, Rand.Range(.4f, .6f), Rand.Range(0, 360), Rand.Range(2, 3), Rand.Range(-200, 200), .15f, 0f, Rand.Range(.2f, .3f), true);
                    }
                }
            }
            base.PostCastShot(flag, out flag);
            return flag;
        }
    }
}
