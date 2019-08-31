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
            harmonyInstance.Patch(AccessTools.Method(typeof(Projectile), "Launch", new Type[]
                {
                    typeof(Thing),
                    typeof(Vector3),
                    typeof(LocalTargetInfo),
                    typeof(LocalTargetInfo),
                    typeof(ProjectileHitFlags),
                    typeof(Thing),
                    typeof(ThingDef)
                }, null), new HarmonyMethod(typeof(Harmony_LotRW), "Projectile_Launch_Prefix", null), null, null);
            harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        }

        public static bool Projectile_Launch_Prefix(Projectile __instance, Thing launcher, Vector3 origin, ref LocalTargetInfo usedTarget, ref LocalTargetInfo intendedTarget)
        {
            if (launcher is Pawn)
            {
                Pawn launcherPawn = (Pawn)launcher;
                if (launcherPawn.health.hediffSet.HasHediff(HediffDef.Named("LotRW_DoomHD"), false))
                {
                    if (launcherPawn.equipment.PrimaryEq != null && launcherPawn.equipment.Primary.def.IsRangedWeapon)
                    {
                        float maxRange = launcherPawn.equipment.Primary.def.Verbs.FirstOrDefault().range;
                        List<Pawn> doomTargets = new List<Pawn>();
                        List<Pawn> mapPawns = launcherPawn.Map.mapPawns.AllPawnsSpawned;
                        doomTargets.Clear();
                        for (int i = 0; i < mapPawns.Count; i++)
                        {
                            if (mapPawns[i].Faction == launcherPawn.Faction && (mapPawns[i].Position - launcherPawn.Position).LengthHorizontal < maxRange)
                            {
                                doomTargets.Add(mapPawns[i]);
                            }
                        }
                        if (doomTargets.Count > 0)
                        {
                            LocalTargetInfo doomTarget = doomTargets.RandomElement();
                            if (doomTarget == launcherPawn)
                            {
                                doomTarget = usedTarget;
                            }
                            else
                            {
                                if (Rand.Chance(.5f))
                                {
                                    HealthUtility.AdjustSeverity(doomTarget.Thing as Pawn, HediffDef.Named("LotRW_DoomHD"), 1f);
                                    for (int i = 0; i < 4; i++)
                                    {
                                        EffectMaker.MakeEffect(ThingDef.Named("Mote_BlackSmoke"), doomTarget.Thing.DrawPos, doomTarget.Thing.Map, Rand.Range(.4f, .6f), Rand.Range(0, 360), Rand.Range(.2f, .4f), Rand.Range(-200, 200), .15f, 2f, Rand.Range(.2f, .3f), true);
                                    }

                                }
                            }

                            Vector3 drawPos = launcherPawn.DrawPos;
                            ThingDef moteDef;
                            if (doomTarget.Cell.x < launcherPawn.Position.x)
                            {
                                drawPos.x += .6f;
                                moteDef = ThingDef.Named("Mote_ReaperWest");
                            }
                            else
                            {
                                drawPos.x -= .6f;
                                moteDef = ThingDef.Named("Mote_ReaperEast");
                            }
                            drawPos.z += .5f;
                            usedTarget = doomTarget;
                            intendedTarget = doomTarget;
                            EffectMaker.MakeEffect(moteDef, drawPos, launcherPawn.Map, .8f, 0, 0, 0, .2f, .1f, .4f, false);

                        }
                    }
                }
            }
            return true;
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
