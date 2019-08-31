using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RimWorld;
using Verse;
using AbilityUser;

namespace Wizardry
{
    public class Mandos_Effect_Haunt : Verb_UseAbility
    {
        bool validTarg;

        public override bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ)
        {
            if (targ.IsValid && targ.CenterVector3.InBounds(base.CasterPawn.Map) && !targ.Cell.Fogged(base.CasterPawn.Map) && targ.Cell.Walkable(base.CasterPawn.Map))
            {
                if ((root - targ.Cell).LengthHorizontal < this.verbProps.range)
                {
                    if (this.CasterIsPawn && this.CasterPawn.apparel != null)
                    {
                        List<Apparel> wornApparel = this.CasterPawn.apparel.WornApparel;
                        for (int i = 0; i < wornApparel.Count; i++)
                        {
                            if (!wornApparel[i].AllowVerbCast(root, this.caster.Map, targ, this))
                            {
                                return false;
                            }
                        }
                        validTarg = true;
                    }
                    else
                    {
                        validTarg = true;
                    }
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

        public virtual void Effect()
        {
            LocalTargetInfo t = this.currentTarget;
            bool flag = t.Cell != default(IntVec3);
            if (flag)
            {
                Thing launchedThing = new Thing()
                {
                    def = WizardryDefOf.FlyingObject_Haunt
                };
                //Pawn casterPawn = base.CasterPawn;
                LongEventHandler.QueueLongEvent(delegate
                {
                    Mandos_FlyingObject_Haunt flyingObject = (Mandos_FlyingObject_Haunt)GenSpawn.Spawn(ThingDef.Named("FlyingObject_Haunt"), this.CasterPawn.Position, this.CasterPawn.Map);
                    flyingObject.AdvancedLaunch(this.CasterPawn, ThingDef.Named("Mote_BlackSmoke"), 1, Rand.Range(10,30), 20, false, this.CasterPawn.DrawPos, this.currentTarget, launchedThing, 25, false, Rand.Range(180, 240), 1f, 0, 0, WizardryDefOf.LotRW_HauntDD, null);
                }, "LaunchingFlyer", false, null);
            }
        }

        public override void PostCastShot(bool inResult, out bool outResult)
        {
            if (inResult)
            {
                this.Effect();
                outResult = true;
            }
            outResult = inResult;
        }
    }
}
