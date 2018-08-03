using Harmony;
using RimWorld;
using AbilityUser;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;
using Verse.AI;
using System.Reflection.Emit;

namespace Wizardry
{
    [StaticConstructorOnStartup]
    internal class Harmony_LotRW
    {
        static Harmony_LotRW()
        {
            HarmonyInstance harmonyInstance = HarmonyInstance.Create("rimworld.lotrw.wizardry");
            harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [HarmonyPatch(typeof(WeatherWorker), "WeatherTick", null)]
    public class WeatherWorker_Patch
    {
        public static void Postfix(WeatherManager __instance, Map map)
        {
            if (map.weatherManager.curWeather.defName == "LotRW_HealingRainWD")
            {
                if (Find.TickManager.TicksGame % 10 == 0)
                {
                    Pawn pawn = map.mapPawns.AllPawnsSpawned.RandomElement();
                    if (!pawn.Position.Roofed(map))
                    {
                        IEnumerable<Hediff_Injury> injuries = pawn.health.hediffSet.GetHediffs<Hediff_Injury>();
                        if (injuries != null && injuries.Count() > 0)
                        {
                            Hediff_Injury injury = injuries.RandomElement();
                            if (injury.CanHealNaturally() && !injury.IsPermanent())
                            {
                                injury.Heal(Rand.Range(.2f, 2f));
                                if (Rand.Chance(.5f))
                                {
                                    EffectMaker.MakeEffect(ThingDef.Named("Mote_HealingWaves"), pawn.DrawPos, map, Rand.Range(.4f, .6f), 180, 1f, 0);
                                }
                                else
                                {
                                    EffectMaker.MakeEffect(ThingDef.Named("Mote_HealingWaves"), pawn.DrawPos, map, Rand.Range(.4f, .6f), 180, 1f, 0, 180, .1f, .02f, .19f, false);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Verb), "TryFindShootLineFromTo", null)]
    public static class TryFindShootLineFromTo_Base_Patch
    {
        public static bool Prefix(Verb __instance, IntVec3 root, LocalTargetInfo targ, out ShootLine resultingLine, ref bool __result)
        {
            if (__instance.verbProps.IsMeleeAttack)
            {
                resultingLine = new ShootLine(root, targ.Cell);
                __result = ReachabilityImmediate.CanReachImmediate(root, targ, __instance.caster.Map, PathEndMode.Touch, null);
                return false;
            }
            if (__instance.verbProps.verbClass.ToString() == "Wizardry.Verb_BLOS")
            {
                //Ignores line of sight
                resultingLine = new ShootLine(root, targ.Cell);
                __result = true;
                return false;
            }
            resultingLine = default(ShootLine);
            __result = true;
            return true;
        }
    }

    [HarmonyPatch(typeof(GenDraw), "DrawRadiusRing", null)]
    public class DrawRadiusRing_Patch
    {
        public static bool Prefix(IntVec3 center, float radius)
        {
            if (radius > GenRadial.MaxRadialPatternRadius)
            {
                return false;
            }
            return true;
        }
    }
}
