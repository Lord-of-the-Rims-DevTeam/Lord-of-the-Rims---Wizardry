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
    public class Manwe_Effect_WindControl : Verb_UseAbility
    {
        private LocalTargetInfo action = new LocalTargetInfo();
        Thing launchableThing;

        public virtual void Effect()
        {
            try
            {
                CompWizardry comp = this.CasterPawn.GetComp<CompWizardry>();
                comp.SecondTarget = null;

                LocalTargetInfo t = this.currentTarget;
                IntVec3 targetCell = t.Cell;

                launchableThing = t.Cell.GetFirstPawn(this.CasterPawn.Map);

                if (launchableThing == null)
                {
                    List<Thing> cellThings = t.Cell.GetThingList(this.CasterPawn.Map);
                    for (int i = 0; i < cellThings.Count(); i++)
                    {
                        if (cellThings[i].def.EverHaulable)
                        {
                            launchableThing = cellThings[i];
                            i = cellThings.Count();
                        }
                    }
                }

                if (launchableThing != null)
                {
                    bool flag = targetCell.InBounds(base.CasterPawn.Map) && targetCell.IsValid;
                    if (flag)
                    {
                        LongEventHandler.QueueLongEvent(delegate
                        {
                            Manwe_FlyingObject_WindControl flyingObject = (Manwe_FlyingObject_WindControl)GenSpawn.Spawn(ThingDef.Named("FlyingObject_WindControl"), this.currentTarget.Cell, this.CasterPawn.Map);
                            flyingObject.Launch(this.CasterPawn, t.Cell, launchableThing);
                        }, "LaunchingFlyer", false, null);
                    }
                    else
                    {
                        Log.Message("invalid map or cell");
                    }

                    Find.Targeter.StopTargeting();
                    this.BeginTargetingWithVerb(WizardryDefOf.CompVerb, WizardryDefOf.CompVerb.MainVerb.targetParams, delegate (LocalTargetInfo info)
                    {
                        this.action = info;
                        comp = this.CasterPawn.GetComp<CompWizardry>();
                        comp.SecondTarget = info;
                    }, this.CasterPawn, null, null);
                }
            }
            catch(NullReferenceException ex)
            {
                Log.Message(ex.ToString());
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
