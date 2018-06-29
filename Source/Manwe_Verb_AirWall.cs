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
    public class Manwe_Verb_AirWall : Verb_UseAbility
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
            base.TryCastShot();
            this.PostCastShot(flag, out flag);
            return flag;
        }

        public override void PostCastShot(bool inResult, out bool outResult)
        {
            CompWizardry comp = base.CasterPawn.GetComp<CompWizardry>();
            comp.SecondTarget = null;

            Find.Targeter.StopTargeting();
            this.BeginTargetingWithVerb(WizardryDefOf.CompVerb, WizardryDefOf.CompVerb.MainVerb.targetParams, delegate (LocalTargetInfo info)
            {
                this.action = info;
                comp = this.CasterPawn.GetComp<CompWizardry>();
                comp.SecondTarget = info;
            }, this.CasterPawn, null, null);
            outResult = inResult;
        }

        public void BeginTargetingWithVerb(WizardAbilityDef verbToAdd, TargetingParameters targetParams, Action<LocalTargetInfo> action, Pawn caster = null, Action actionWhenFinished = null, Texture2D mouseAttachment = null)
        {
            Find.Targeter.targetingVerb = null;
            Find.Targeter.targetingVerbAdditionalPawns = null;
            AccessTools.Field(typeof(Targeter), "action").SetValue(Find.Targeter, action);
            AccessTools.Field(typeof(Targeter), "targetParams").SetValue(Find.Targeter, targetParams);
            AccessTools.Field(typeof(Targeter), "caster").SetValue(Find.Targeter, caster);
            AccessTools.Field(typeof(Targeter), "actionWhenFinished").SetValue(Find.Targeter, actionWhenFinished);
            AccessTools.Field(typeof(Targeter), "mouseAttachment").SetValue(Find.Targeter, mouseAttachment);
        }
    }
}
