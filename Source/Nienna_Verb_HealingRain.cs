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
    public class Nienna_Verb_HealingRain : Verb_UseAbility
    {
        protected override bool TryCastShot()
        {
            Map map = base.CasterPawn.Map;
            Pawn pawn = base.CasterPawn;
            IntVec3 centerCell = this.currentTarget.Cell;
            if ((centerCell.IsValid && centerCell.InBounds(map)))
            {
                bool flag = map.weatherManager.curWeather.defName == "Rain" || map.weatherManager.curWeather.defName == "RainyThunderstorm" || map.weatherManager.curWeather.defName == "FoggyRain";
                if (flag)
                {
                    map.weatherDecider.DisableRainFor(0);
                    map.weatherManager.TransitionTo(WizardryDefOf.LotRW_HealingRainWD);
                }
                else
                {
                    Messages.Message("unable to invoke healing rain - weather is not rain or is transitioning", MessageTypeDefOf.RejectInput);
                }
            }
            this.Ability.PostAbilityAttempt();
            this.burstShotsLeft = 0;
            return false;
        }
        
    }
}
