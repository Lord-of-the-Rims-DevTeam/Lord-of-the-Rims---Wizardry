using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using Verse.AI;
using UnityEngine;


namespace Wizardry
{
    internal class JobDriver_StormCalling : JobDriver
    {
        private const TargetIndex building = TargetIndex.A;

        int age = -1;
        int lastStrike = 0;
        int ticksTillNextStrike = 120;
        int duration = 1200;
        List<Pawn> targetList = new List<Pawn>();

        public override bool TryMakePreToilReservations()
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {           
            Toil commandStorm = new Toil();
            commandStorm.initAction = delegate
            {
                if (age > duration)
                {
                    this.EndJobWith(JobCondition.Succeeded);
                }
                Map map = this.pawn.Map;
                bool flag = map.weatherManager.curWeather.defName == "Rain" || map.weatherManager.curWeather.defName == "RainyThunderstorm" || map.weatherManager.curWeather.defName == "FoggyRain" || 
                map.weatherManager.curWeather.defName == "SnowHard" || map.weatherManager.curWeather.defName == "SnowGentle" || map.weatherManager.curWeather.defName == "DryThunderstorm";
                if (!flag)
                {
                    this.EndJobWith(JobCondition.Succeeded);
                }
                this.GetTargetList();
                if (targetList.Count < 1)
                {
                    this.EndJobWith(JobCondition.Succeeded);
                }
            };
            commandStorm.tickAction = delegate
            {
                if (age > (lastStrike + ticksTillNextStrike))
                {
                    DoWeatherEffect();
                    ticksTillNextStrike = Rand.Range(20, 200);
                    lastStrike = age;
                }
                if(Find.TickManager.TicksGame % 4 ==0)
                {
                    float direction = Rand.Range(0, 360);
                    EffectMaker.MakeEffect(WizardryDefOf.Mote_CastingBeam, pawn.DrawPos, pawn.Map, Rand.Range(.1f, .4f), direction, Rand.Range(8, 10), 0, direction, 0.2f, .02f, .1f, false);
                }
                age++;
                ticksLeftThisToil = duration - age;
                if (age > duration)
                {
                    this.EndJobWith(JobCondition.Succeeded);
                }
                if (this.Map.weatherManager.curWeather.defName == "Clear")
                {
                    this.EndJobWith(JobCondition.Succeeded);
                }
            };
            commandStorm.defaultCompleteMode = ToilCompleteMode.Delay;
            commandStorm.defaultDuration = this.duration;
            commandStorm.WithProgressBar(TargetIndex.A, delegate
            {
                if (this.pawn.DestroyedOrNull() || this.pawn.Dead || this.pawn.Downed)
                {
                    return 1f;
                }
                return 1f - (float)commandStorm.actor.jobs.curDriver.ticksLeftThisToil / this.duration;

            }, false, 0f);
            commandStorm.AddFinishAction(delegate
            {
                Log.Message("ending storm calling");
                //do soemthing?
            });
            yield return commandStorm;
        }

        private void GetTargetList()
        {
            List<Pawn> mapPawns = this.Map.mapPawns.AllPawnsSpawned;
            for (int i =0; i < mapPawns.Count(); i++)
            {
                if(!mapPawns[i].DestroyedOrNull() && !mapPawns[i].Dead && !mapPawns[i].Downed && mapPawns[i].HostileTo(this.pawn))
                {
                    targetList.Add(mapPawns[i]);                    
                }
            }
        }

        private void DoWeatherEffect()
        {
            
            Pawn pawn = targetList.RandomElement();
            float rnd = Rand.Range(0f, 1f);
            if(rnd > .8f)
            {
                rnd = 3;
            }
            else if(rnd > .5f)
            {
                rnd = 2;
            }
            else
            {
                rnd = 1;
            }
            for (int i = 0; i < rnd; i++)
            {
                IntVec3 strikeLoc = pawn.Position;
                strikeLoc.x += Rand.Range(-2, 2);
                strikeLoc.z += Rand.Range(-2, 2);
                Map.weatherManager.eventHandler.AddEvent(new WeatherEvent_LightningStrike(this.Map, strikeLoc));
                //want a larger explosion or more effects like stun?
                //GenExplosion.DoExplosion(this.centerLocation.ToIntVec3, this.Map, this.areaRadius, DamageDefOf.Bomb, null, Rand.Range(6, 16), SoundDefOf.Thunder_OffMap, null, null, null, 0f, 1, false, null, 0f, 1, 0.1f, true);
            }
        }
    }
}
