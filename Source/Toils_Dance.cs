using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using RimWorld;
using UnityEngine;

namespace Wizardry
{
    public static class Toils_Dance
    {
        static int ticksTillEffects = 0;
        static int age = -1;
        static int danceDuration = 20;

        public static Toil Dance()
        {
            Toil dance = new Toil();
            dance.tickAction = delegate
            {
                Pawn actor = dance.actor;
                Job curJob = actor.jobs.curJob;
                JobDriver curDriver = actor.jobs.curDriver;
                Log.Message("dancing age " + age + " ticks till rotate " + ticksTillEffects);
                if (ticksTillEffects > 5)
                {
                    EffectMaker.MakeEffect(WizardryDefOf.Mote_Sparks, actor.DrawPos, actor.Map, .6f, Rand.Range(-60, 60), Rand.Range(2,3), Rand.Range(100,200));                                           
                    ticksTillEffects = 0;
                }
                if (age > danceDuration)
                {
                    if (actor != null && !actor.Downed && !actor.Dead)
                    {
                        curDriver.ReadyForNextToil();                        
                        age = 0;
                    }
                }
                age++;
                ticksTillEffects++;
            };
            dance.defaultCompleteMode = ToilCompleteMode.Never;
            return dance;
        }
    }
}
