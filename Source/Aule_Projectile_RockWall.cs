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
    public class Aule_Projectile_RockWall : Projectile_AbilityBase
    {
        private bool initialized = false;
        private bool wallActive = false;
        private int wallLength = 0;        
        private Vector3 wallPos = default(Vector3);
        private int age = -1;
        private int duration = 300;
        Vector3 wallDir = default(Vector3);
        IntVec3 wallEnd = default(IntVec3);
        List<IntVec3> wallPositions = new List<IntVec3>();
        List<Thing> despawnedThingList = new List<Thing>();
        Pawn caster;

        //unsaved variables
        private int wallLengthMax = 20;
        private float wallSpawnChance = .9f;
        ThingDef spawnDef = ThingDef.Named("Sandstone");

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.initialized, "initialized", false, false);
            Scribe_Values.Look<bool>(ref this.wallActive, "wallActive", false, false);
            Scribe_Values.Look<int>(ref this.age, "age", -1, false);
            Scribe_Values.Look<int>(ref this.duration, "duration", 300, false);
            Scribe_Values.Look<int>(ref this.wallLength, "wallLength", 0, false);
            Scribe_Values.Look<Vector3>(ref this.wallPos, "wallPos", default(Vector3), false);
            Scribe_Values.Look<Vector3>(ref this.wallDir, "wallDir", default(Vector3), false);
            Scribe_Values.Look<IntVec3>(ref this.wallEnd, "wallEnd", default(IntVec3), false);
            Scribe_References.Look<Pawn>(ref this.caster, "caster", false);
            Scribe_Collections.Look<IntVec3>(ref this.wallPositions, "wallPositions", LookMode.Value);
            Scribe_Collections.Look<Thing>(ref this.despawnedThingList, "despawnedThingList", LookMode.Value);
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
                //todo: determine thingdef based on ground type, not random
                float rnd = Rand.Range(0f, 3f);
                if (rnd < 1)
                {
                    spawnDef = ThingDef.Named("Sandstone");
                }
                else if (rnd < 2)
                {
                    spawnDef = ThingDef.Named("Granite");
                }
                else if (rnd < 3)
                {
                    spawnDef = ThingDef.Named("Slate");  
                }
                else if (rnd < 4)
                {
                    spawnDef = ThingDef.Named("Limestone");
                }
                else
                {
                    spawnDef = ThingDef.Named("Marble");
                }
                this.initialized = true;
            }

            CompWizardry comp = caster.GetComp<CompWizardry>();
            if(!this.wallActive && comp.SecondTarget != null)
            {
                this.age = 0;
                this.duration = 2400;
                this.wallActive = true;
                this.wallPos = base.Position.ToVector3Shifted();
                this.wallDir = GetVector(base.Position.ToVector3Shifted(), comp.SecondTarget.Cell.ToVector3Shifted());
                this.wallEnd = comp.SecondTarget.Cell;
                comp.SecondTarget = null;
            }

            if (!wallActive)
            {
                if (Find.TickManager.TicksGame % 6 == 0)
                {
                    EffectMaker.MakeEffect(ThingDef.Named("Mote_ThickDust"), base.Position.ToVector3Shifted(), this.Map, Rand.Range(.4f, .6f), Rand.Range(0, 360), Rand.Range(.8f, 1.6f), Rand.Range(-20, 20), 0, Rand.Range(.2f, .3f), .05f, Rand.Range(.4f, .6f), false);   
                }
            }
            else
            {                
                if (wallLength < wallLengthMax)
                {
                    float magnitude = (base.Position.ToVector3Shifted() - Find.Camera.transform.position).magnitude;
                    Find.CameraDriver.shaker.DoShake(10 / magnitude);
                    for (int k = 0; k < wallLengthMax; k++)
                    {
                        List<Thing> cellList = this.wallPos.ToIntVec3().GetThingList(caster.Map);
                        bool hasWall = false;
                        for (int i = 0; i < cellList.Count(); i++)
                        {
                            if (cellList[i].def.designationCategory == DesignationCategoryDefOf.Structure)
                            {
                                hasWall = true;
                            }
                        }

                        if (!hasWall)
                        {
                            bool spawnWall = true;
                            for (int i = 0; i < cellList.Count(); i++)
                            {
                                if (!cellList[i].def.EverHaulable && !(cellList[i] is Pawn))
                                {
                                    if (cellList[i].def.altitudeLayer == AltitudeLayer.Building || cellList[i].def.altitudeLayer == AltitudeLayer.Item || cellList[i].def.altitudeLayer == AltitudeLayer.ItemImportant)
                                    {
                                        //Log.Message("bypassing object and setting wall spawn to false");
                                        spawnWall = false;
                                    }
                                    else
                                    {
                                        if (cellList[i].def.defName.Contains("Mote") || (cellList[i].def.defName == "LotRW_Projectile_RockWall"))
                                        {
                                            //Log.Message("avoided storing " + cellList[i].def.defName);
                                        }
                                        else
                                        {
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
                                }
                            }
                            if (spawnWall && Rand.Chance(this.wallSpawnChance))
                            {
                                AbilityUser.SpawnThings tempSpawn = new SpawnThings()
                                {
                                    def = spawnDef,
                                    spawnCount = 1
                                };
                                SingleSpawnLoop(tempSpawn, wallPos.ToIntVec3(), caster.Map);
                                this.wallLength++;
                                this.wallPositions.Add(wallPos.ToIntVec3());
                                EffectMaker.MakeEffect(ThingDef.Named("Mote_ThickDust"), this.wallPos, this.Map, Rand.Range(.6f, .8f), Rand.Range(0, 360), Rand.Range(1f, 2f), Rand.Range(-20, 20), 0, Rand.Range(.2f, .3f), .05f, Rand.Range(.4f, .6f), false);
                            }
                        }

                        this.wallPos += this.wallDir;
                        EffectMaker.MakeEffect(ThingDef.Named("Mote_ThickDust"), this.wallPos, this.Map, Rand.Range(.6f, 1f), Rand.Range(0, 360), Rand.Range(.6f, 1f), Rand.Range(-20, 20), 0, Rand.Range(.4f, .6f), Rand.Range(.05f,.3f), Rand.Range(.4f, 1f), false);

                        if (this.wallPos.ToIntVec3() == this.wallEnd)
                        {
                            this.wallPos -= this.wallDir;
                            this.wallLength = this.wallLengthMax;
                        }
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
                Building structure = null;
                for (int j = 0; j < this.wallPositions.Count(); j++)
                {                    
                    structure = this.wallPositions[j].GetFirstBuilding(this.Map);
                    if(structure != null)
                    {
                        structure.Destroy();
                        EffectMaker.MakeEffect(ThingDef.Named("Mote_ThickDust"), this.wallPositions[j].ToVector3Shifted(), this.Map, Rand.Range(.6f, .8f), Rand.Range(0, 360), Rand.Range(.8f, 1.6f), Rand.Range(-20, 20), 0, Rand.Range(.2f, .3f), .05f, Rand.Range(.4f, .6f), false);
                    }
                    structure = null;
                }

                for (int i =0; i < this.despawnedThingList.Count(); i++)
                {
                    GenSpawn.Spawn(this.despawnedThingList[i], this.despawnedThingList[i].Position, this.Map);
                }
                base.Destroy(mode);
            }
        }
    }
}
