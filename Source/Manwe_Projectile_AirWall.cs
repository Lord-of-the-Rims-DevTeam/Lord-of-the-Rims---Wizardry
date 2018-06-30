using Verse;
using Verse.Sound;
using RimWorld;
using AbilityUser;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Verse.AI;
using Harmony;

namespace Wizardry
{
    [StaticConstructorOnStartup]
    public class Manwe_Projectile_AirWall : Projectile_AbilityBase
    {
        private bool initialized = false;
        private bool wallActive = false;
        private int wallLength = 0;
        private int wallLengthMax = 20;
        private Vector3 wallPos = default(Vector3);
        private int age = -1;
        private int duration = 300;
        Vector3 wallDir = default(Vector3);
        IntVec3 wallEnd = default(IntVec3);
        List<IntVec3> wallPositions = new List<IntVec3>();
        List<Thing> despawnedThingList = new List<Thing>();
        Pawn caster;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.initialized, "initialized", false, false);
            Scribe_Values.Look<int>(ref this.age, "age", -1, false);
            //Scribe_Values.Look<IntVec3>(ref this.anglePos, "anglePos", default(IntVec3), false);
            //Scribe_Collections.Look<float>(ref this.beamMaxSize, "beamMaxSize", LookMode.Value);
            //Scribe_Collections.Look<Vector3>(ref this.beamVector, "beamVector", LookMode.Value);
            //Scribe_Collections.Look<int>(ref this.beamDuration, "beamDuration", LookMode.Value);
            //Scribe_Collections.Look<int>(ref this.beamAge, "beamAge", LookMode.Value);
            //Scribe_Collections.Look<Vector3>(ref this.beamPos, "beamPos", LookMode.Value);
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

        private void GetSecondTarget()
        {
            Find.Targeter.StopTargeting();
            this.BeginTargetingWithVerb(WizardryDefOf.CompVerb, WizardryDefOf.CompVerb.MainVerb.targetParams, delegate (LocalTargetInfo info)
            {
                CompWizardry comp = caster.GetComp<CompWizardry>();
                comp.SecondTarget = info;
                Log.Message("second target set at " + comp.SecondTarget);
            }, caster, null, null);
            
        }

        protected override void Impact(Thing hitThing)
        {
            Map map = base.Map;
            base.Impact(hitThing);
            ThingDef def = this.def;            
            if (!this.initialized)
            {
                caster = this.launcher as Pawn;
                GetSecondTarget();
                this.initialized = true;
                Log.Message("initialized");
            }

            CompWizardry comp = caster.GetComp<CompWizardry>();
            if(!this.wallActive && comp.SecondTarget != null)
            {
                this.age = 0;
                this.duration = 1200;
                this.wallActive = true;
                this.wallPos = base.Position.ToVector3Shifted();
                this.wallDir = GetVector(base.Position.ToVector3Shifted(), comp.SecondTarget.Cell.ToVector3Shifted());
                this.wallEnd = comp.SecondTarget.Cell;
                comp.SecondTarget = null;
                Log.Message("building wall in direction " + this.wallDir);
            }

            if (!wallActive)
            {
                Log.Message("wall inactive");
                if (Find.TickManager.TicksGame % 6 == 0)
                {
                    MoteMaker.ThrowDustPuff(base.Position, caster.Map, Rand.Range(.6f, .9f));                    
                }
            }
            else
            {
                if (Find.TickManager.TicksGame % 3 == 0)
                {
                    if (wallLength <= wallLengthMax)
                    {
                        List<Thing> cellList = this.wallPos.ToIntVec3().GetThingList(caster.Map);
                        bool hasWall = false;
                        for (int i = 0; i < cellList.Count(); i++)
                        {
                            if (cellList[i].def.defName == "LotRW_WindWall")
                            {
                                hasWall = true;
                            }
                        }

                        if (!hasWall)
                        {
                            bool spawnWall = true;
                            for (int i = 0; i < cellList.Count(); i++)
                            {
                                if (!cellList[i].def.EverHaulable)
                                {
                                    if (cellList[i].def.altitudeLayer == AltitudeLayer.Building || cellList[i].def.altitudeLayer == AltitudeLayer.Item || cellList[i].def.altitudeLayer == AltitudeLayer.ItemImportant)
                                    {
                                        Log.Message("bypassing object and setting wall spawn to false");
                                        spawnWall = false;
                                    }
                                    else
                                    {
                                        if (cellList[i].def.defName.Contains("Mote") || (cellList[i].def.defName == "LotRW_Projectile_AirWall"))
                                        {
                                            Log.Message("avoided storing " + cellList[i].def.defName);
                                        }
                                        else
                                        {
                                            Log.Message("storing object " + cellList[i].def.defName);
                                            this.despawnedThingList.Add(cellList[i]);
                                            cellList[i].DeSpawn();                                            
                                        }
                                    }
                                }
                                else
                                {
                                    int launchDir = -90;
                                    if (Rand.Chance(.5f)) { launchDir = 90; }
                                    LaunchFlyingObect(cellList[i].Position + (Quaternion.AngleAxis(launchDir, Vector3.up) * wallDir).ToIntVec3(), cellList[i]);
                                    Log.Message("launching object in way of wall");
                                }
                            }
                            if (spawnWall)
                            {
                                AbilityUser.SpawnThings tempSpawn = new SpawnThings();
                                tempSpawn.def = ThingDef.Named("LotRW_WindWall");
                                tempSpawn.spawnCount = 1;
                                SingleSpawnLoop(tempSpawn, wallPos.ToIntVec3(), caster.Map);
                                this.wallLength++;
                                this.wallPositions.Add(wallPos.ToIntVec3());
                            }
                        }

                        this.wallPos += this.wallDir;

                        if (!this.wallPos.ToIntVec3().Walkable(caster.Map) || this.wallPos.ToIntVec3() == this.wallEnd)
                        {
                            Log.Message("ending wall creation prematurely  at length " + this.wallLength);
                            this.wallPos -= this.wallDir;
                            this.wallLength = this.wallLengthMax;
                        }
                    }

                    for (int j = 0; j < this.wallPositions.Count(); j++)
                    {
                        int launchDir = Rand.Range(-100, -80);
                        if (Rand.Chance(.5f)) { launchDir = Rand.Range(80, 100); }
                        EffectMaker.MakeEffect(ThingDef.Named("Mote_DustPuff"), this.wallPositions.RandomElement().ToVector3Shifted(), caster.Map, Rand.Range(.6f, .8f), (Quaternion.AngleAxis(launchDir, Vector3.up) * wallDir).ToAngleFlat(), Rand.Range(2f, 5f), Rand.Range(100, 200), .04f, .03f, .8f, false);
                    }
                }
            }
        }

        public void LaunchFlyingObect(IntVec3 targetCell, Thing thing)
        {
            bool flag = targetCell != null && targetCell != default(IntVec3);
            if (flag)
            {
                if (thing != null && thing.Position.IsValid && !this.Destroyed && thing.Spawned && thing.Map != null)
                {
                    FlyingObject_Spinning flyingObject = (FlyingObject_Spinning)GenSpawn.Spawn(ThingDef.Named("FlyingObject_Spinning"), thing.Position, thing.Map);
                    flyingObject.speed = 22;
                    flyingObject.Launch(caster, targetCell, thing);
                }
            }
        }    

        public void SingleSpawnLoop(SpawnThings spawnables, IntVec3 position, Map map)
        {
            bool flag = spawnables.def != null;
            if (flag)
            {
                Faction faction = this.caster.Faction;
                ThingDef def = spawnables.def;
                ThingDef stuff = null;
                bool madeFromStuff = def.MadeFromStuff;
                if (madeFromStuff)
                {
                    stuff = ThingDefOf.BlocksGranite;
                }
                Thing thing = ThingMaker.MakeThing(def, stuff);
                GenSpawn.Spawn(thing, position, map, Rot4.North, WipeMode.Vanish);
            }
        }

        public Vector3 GetVector(Vector3 casterPos, Vector3 targetPos)
        {
            Vector3 heading = (targetPos - casterPos);
            float distance = heading.magnitude;
            Vector3 direction = heading / distance;
            return direction;
        }

        public override void Tick()
        {
            base.Tick();
            this.age++;
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            bool flag = this.age <= this.duration;
            if (!flag)
            {
                Log.Message("destroying wind wall at age " + this.age);
                for(int i =0; i < this.despawnedThingList.Count(); i++)
                {
                    GenSpawn.Spawn(this.despawnedThingList[i], this.despawnedThingList[i].Position, this.Map);
                    Log.Message("spawning " + this.despawnedThingList[i].def.defName + " at " + this.despawnedThingList[i].Position);
                }
                base.Destroy(mode);
            }
        }
    }
}
