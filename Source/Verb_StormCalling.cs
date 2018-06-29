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
    public class Verb_StormCalling : Verb_UseAbility
    {

        protected override bool TryCastShot()
        {
            Map map = base.CasterPawn.Map;
            Pawn pawn = base.CasterPawn;

            if (map.weatherManager.curWeather.defName == "Rain" || map.weatherManager.curWeather.defName == "RainyThunderstorm" || map.weatherManager.curWeather.defName == "FoggyRain" || 
                map.weatherManager.curWeather.defName == "SnowHard" || map.weatherManager.curWeather.defName == "SnowGentle" || map.weatherManager.curWeather.defName == "DryThunderstorm")
            {
                Job job = new Job(WizardryDefOf.JobDriver_StormCalling);
                pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            }
            else
            {
                Messages.Message("unable to call lightning under these weather conditions", MessageTypeDefOf.RejectInput);
            }
            this.Ability.PostAbilityAttempt();
            this.burstShotsLeft = 0;
            return false;
        }
    }
}
