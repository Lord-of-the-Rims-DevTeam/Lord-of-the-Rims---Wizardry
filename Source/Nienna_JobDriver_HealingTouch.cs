using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using Verse.AI;
using UnityEngine;
using AbilityUser;

namespace Wizardry
{
    internal class Nienna_JobDriver_HealingTouch : JobDriver
    {
        private const TargetIndex caster = TargetIndex.B;

        int age = -1;
        int lastHeal = 0;
        int ticksTillNextHeal = 30;
        public int duration = 1200;
        int injuryCount = 0;
        bool issueJobAgain = false;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (pawn.Reserve(TargetA, this.job, 1, 1, null, errorOnFailed))
            {
                return true;
            }
            return false;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Pawn patient = TargetA.Thing as Pawn;
            Toil gotoPatient = new Toil()
            {
                initAction = () =>
                {
                    pawn.pather.StartPath(TargetA, PathEndMode.Touch);
                },
                defaultCompleteMode = ToilCompleteMode.PatherArrival
            };
            yield return gotoPatient;
            Toil doHealing = new Toil();
            doHealing.initAction = delegate
            {
                if (age > duration)
                {
                    this.EndJobWith(JobCondition.Succeeded);
                }
                if (patient.DestroyedOrNull() || patient.Dead)
                {
                    this.EndJobWith(JobCondition.Incompletable);
                }
            };
            doHealing.tickAction = delegate
            {
                if(patient.DestroyedOrNull() || patient.Dead)
                {
                    this.EndJobWith(JobCondition.Incompletable);
                }
                if(Find.TickManager.TicksGame % 1 ==0)
                {
                    EffectMaker.MakeEffect(ThingDef.Named("Mote_HealingMote"), this.pawn.DrawPos, this.Map, Rand.Range(.3f, .5f), (Quaternion.AngleAxis(90, Vector3.up) * GetVector(this.pawn.Position, patient.Position)).ToAngleFlat() + Rand.Range(-10,10), 5f, 0);

                }
                if (age > (lastHeal + ticksTillNextHeal))
                {
                    DoHealingEffect(patient);
                    EffectMaker.MakeEffect(ThingDef.Named("Mote_HealingCircles"), patient.DrawPos, this.Map, Rand.Range(.3f, .4f), 0, 0, Rand.Range(400, 500), Rand.Range(0, 360), .08f, .01f, .24f, false);
                    lastHeal = age;
                    if(this.injuryCount == 0)
                    {
                        this.EndJobWith(JobCondition.Succeeded);
                    }
                }
                if (!patient.Drafted && patient.CurJobDef != JobDefOf.Wait)
                {
                    if (patient.jobs.posture == PawnPosture.Standing)
                    {
                        Job job = new Job(JobDefOf.Wait, patient);
                        patient.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                    }
                }
                age++;
                ticksLeftThisToil = duration - age;
                if (age > duration)
                {
                    this.EndJobWith(JobCondition.Succeeded);
                }
            };
            doHealing.defaultCompleteMode = ToilCompleteMode.Delay;
            doHealing.defaultDuration = this.duration;
            doHealing.WithProgressBar(TargetIndex.B, delegate
            {
                if (this.pawn.DestroyedOrNull() || this.pawn.Dead)
                {
                    return 1f;
                }
                return 1f - (float)doHealing.actor.jobs.curDriver.ticksLeftThisToil / this.duration;

            }, false, 0f);
            doHealing.AddFinishAction(delegate
            {
                CompWizardry comp = pawn.GetComp<CompWizardry>();
                PawnAbility pawnAbility = comp.AbilityData.Powers.FirstOrDefault((PawnAbility x) => x.Def == WizardryDefOf.LotRW_Nienna_HealingTouch);
                pawnAbility.PostAbilityAttempt();
                patient.jobs.EndCurrentJob(JobCondition.Succeeded, true);
            });
            yield return doHealing;
        }

        private void DoHealingEffect(Pawn patient)
        {
            int num = 1;
            this.injuryCount = 0; 
            using (IEnumerator<BodyPartRecord> enumerator = patient.health.hediffSet.GetInjuredParts().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    BodyPartRecord rec = enumerator.Current;
                    bool flag2 = num > 0;
                    int num2 = 1;
                    if (flag2)
                    {
                        IEnumerable<Hediff_Injury> arg_BB_0 = patient.health.hediffSet.GetHediffs<Hediff_Injury>();
                        Func<Hediff_Injury, bool> arg_BB_1;                        
                        arg_BB_1 = ((Hediff_Injury injury) => injury.Part == rec);                        
                        foreach (Hediff_Injury current in arg_BB_0.Where(arg_BB_1))
                        {
                            bool flag3 = num2 > 0;
                            if (flag3)
                            {
                                bool flag5 = current.CanHealNaturally() && !current.IsPermanent();
                                if (flag5)
                                {
                                    this.injuryCount++;
                                    current.Heal(Rand.Range(1f, 2f));
                                    num--;
                                    num2--;
                                }
                            }
                        }
                    }
                }
            }
        }

        public Vector3 GetVector(IntVec3 center, IntVec3 objectPos)
        {
            Vector3 heading = (objectPos - center).ToVector3();
            float distance = heading.magnitude;
            Vector3 direction = heading / distance;
            return direction;
        }
    }
}
