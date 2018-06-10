using System;
using AbilityUser;
using Verse;
using RimWorld;
using Verse.AI;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Wizardry
{
    public class Ulmo_Effect_RainDance : Verb_UseAbility
    {

        public virtual void Effect()
        {
            LocalTargetInfo t = base.CasterPawn;
            IntVec3 targetCell = base.CasterPawn.Position;
            targetCell.z += 3;
            t = targetCell;
            bool flag = targetCell.InBounds(base.CasterPawn.Map) && targetCell.IsValid;
            if (flag)
            {          
                //base.CasterPawn.rotationTracker.Face(targetCell.ToVector3());
                LongEventHandler.QueueLongEvent(delegate
                {
                    Ulmo_FlyingObject_RainDance flyingObject = (Ulmo_FlyingObject_RainDance)GenSpawn.Spawn(ThingDef.Named("FlyingObject_RainDance"), this.CasterPawn.Position, this.CasterPawn.Map);
                    flyingObject.Launch(this.CasterPawn, t.Cell, base.CasterPawn);
                }, "LaunchingFlyer", false, null);
            }
            else
            {
                Log.Message("not enough height to use this ability");
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
