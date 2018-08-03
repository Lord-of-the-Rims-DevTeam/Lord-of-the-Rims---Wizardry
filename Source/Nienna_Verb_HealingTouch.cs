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
    public class Nienna_Verb_HealingTouch : Verb_UseAbility
    {
        protected override bool TryCastShot()
        {
            if (this.currentTarget.Thing is Pawn)
            {
                Pawn pawn = this.CasterPawn;

                if (!pawn.DestroyedOrNull() && !pawn.Dead && pawn.RaceProps.IsFlesh)
                {
                    Job job = new Job(WizardryDefOf.JobDriver_HealingTouch, currentTarget, pawn);                  
                    pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                }
                else
                {
                    Messages.Message("pawn is incapable of being healed", MessageTypeDefOf.RejectInput);
                }
            }
            else
            {
                Messages.Message("invalid target for healing touch", MessageTypeDefOf.RejectInput);
            }
            this.Ability.PostAbilityAttempt();
            this.burstShotsLeft = 0;
            return false;
        }
    }
}
