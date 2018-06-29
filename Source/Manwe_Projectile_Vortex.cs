using System;
using System.Collections.Generic;
using System.Linq;
using AbilityUser;
using UnityEngine;
using Verse;
using Verse.Noise;
using Verse.Sound;
using RimWorld;

namespace Wizardry
{
    [StaticConstructorOnStartup]
    class Manwe_Projectile_Vortex : Projectile_AbilityBase
    {
        Pawn pawn;
        private int age = 0;
        private int duration = 1200;
        private int strikeNum = 4;
        private int strikeDelay = 20; //random 45-90 within class
        private int fireDelay = 10;
        private Vector3 direction = default(Vector3);
        private bool initialized = false;
        private int leftFadeOutTicks = -1;
        private Vector3 realPosition;
        private Sustainer sustainer;
        private float fireVortexValue = 0;

        private static MaterialPropertyBlock matPropertyBlock = new MaterialPropertyBlock();
        private static readonly Material TornadoMaterial = MaterialPool.MatFrom("Things/Ethereal/Tornado", ShaderDatabase.Transparent, MapMaterialRenderQueues.Tornado);

        float radius = 5;
        List<IntVec3> cellList;
        
        IEnumerable<IntVec3> targets;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.initialized, "initialized", true, false);
            Scribe_Values.Look<int>(ref this.age, "age", -1, false);
            Scribe_Values.Look<int>(ref this.duration, "duration", 900, false);
            Scribe_Values.Look<int>(ref this.strikeDelay, "strikeDelay", 0, false);
            Scribe_Values.Look<float>(ref this.fireVortexValue, "fireVortexValue", 0, false);
            Scribe_References.Look<Pawn>(ref this.pawn, "pawn", false);
            Scribe_Collections.Look<IntVec3>(ref this.cellList, "cellList", LookMode.Value);
            Scribe_Values.Look<Vector3>(ref this.direction, "direction", default(Vector3), false);
            Scribe_Values.Look<Vector3>(ref this.realPosition, "realPosition", default(Vector3), false);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            bool flag = this.age < duration;
            if (!flag)
            {
                base.Destroy(mode);
            }
        }

        public override void Tick()
        {
            base.Tick();
            this.age++;
        }

        protected override void Impact(Thing hitThing)
        {
            base.Impact(hitThing);

            ThingDef def = this.def;
            Pawn victim = null;

            if (!this.initialized)
            {
                this.pawn = this.launcher as Pawn;
                this.radius = this.def.projectile.explosionRadius;
                this.initialized = true;
                this.direction = GetVector(pawn.Position, base.Position);
                this.realPosition = base.Position.ToVector3();
                this.targets = GenRadial.RadialCellsAround(base.Position, strikeNum, false);
                cellList = targets.ToList<IntVec3>();
                this.CreateSustainer();
            }

            if (this.sustainer == null)
            {
                Log.Error("Vortex sustainer is null.");
                this.CreateSustainer();
            }
            this.sustainer.Maintain();
            this.UpdateSustainerVolume();

            this.realPosition += this.direction * .1f;
            if (this.Map != null)
            {
                if (!this.realPosition.ToIntVec3().Walkable(this.Map))
                {
                    this.age = this.duration;
                }
                EffectMaker.MakeEffect(ThingDefOf.Mote_TornadoDustPuff, this.realPosition, this.Map, Rand.Range(.6f, .9f), Rand.Range(0, 360), Rand.Range(4f, 5f), Rand.Range(100, 200));
                IntVec3 curCell;
                for (int i = 0; i < 5; i++)
                {
                    curCell = cellList.RandomElement();
                    if (curCell.IsValid && curCell.InBounds(this.Map))
                    {
                        Vector3 moteVector = GetVector(this.realPosition.ToIntVec3(), curCell);
                        EffectMaker.MakeEffect(ThingDef.Named("Mote_Tornado"), curCell.ToVector3(), this.Map, Rand.Range(.4f, .8f), (Quaternion.AngleAxis(Rand.Range(-35, -55), Vector3.up) * moteVector).ToAngleFlat(), Rand.Range(1f, 3f), Rand.Range(-200, -500), (Quaternion.AngleAxis(Rand.Range(-35, -55), Vector3.up) * moteVector).ToAngleFlat(), Rand.Range(.2f, .3f), .1f, Rand.Range(.05f, .2f), true);
                    }
                }

                if (Find.TickManager.TicksGame % 50 == 0)
                {
                    this.direction.x = (Rand.Range(-.6f, .6f));
                    this.direction.z = (Rand.Range(-.6f, .6f));
                }

                if (Find.TickManager.TicksGame % this.strikeDelay == 0)
                {
                    for (int i = 0; i < cellList.Count(); i++)
                    {
                        curCell = cellList[i];
                        if (curCell.IsValid && curCell.InBounds(this.Map))
                        {
                            this.fireVortexValue += this.CalculateFireAmountInArea(curCell, .4f);
                            float force = (10f / (curCell.ToVector3() - this.realPosition).magnitude + 10f);
                            Thing dmgThing;
                            List<Thing> hitList = curCell.GetThingList(this.Map);
                            for (int j = 0; j < hitList.Count; j++)
                            {
                                dmgThing = hitList[j];                                
                                Vector3 launchVector = GetVector(dmgThing.Position, this.realPosition.ToIntVec3());
                                IntVec3 projectedPosition = dmgThing.Position + (force * launchVector).ToIntVec3();
                                if (dmgThing is Pawn)
                                {
                                    victim = dmgThing as Pawn;
                                    int mass = 10; // victim.mass possible calculation, currently not used
                                    if (projectedPosition.IsValid && projectedPosition.InBounds(this.Map) && !victim.Dead)
                                    {
                                        if (this.fireVortexValue > 0)
                                        {
                                            damageEntities(dmgThing, Mathf.RoundToInt(this.def.projectile.damageAmountBase * force), DamageDefOf.Flame);
                                            this.fireVortexValue -= .2f;
                                        }
                                        LaunchFlyingObect(projectedPosition, victim);
                                    }
                                }
                                else if(dmgThing is Building)
                                {
                                    if (this.fireVortexValue > 0)
                                    {
                                        damageEntities(dmgThing, Mathf.RoundToInt(this.def.projectile.damageAmountBase * force * 2), DamageDefOf.Flame);
                                        this.fireVortexValue -= .2f;
                                    }
                                }
                                else if (dmgThing.def.EverHaulable && !(dmgThing is Corpse))
                                {
                                    if (projectedPosition.IsValid && projectedPosition.InBounds(this.Map))
                                    {
                                        LaunchFlyingObect(projectedPosition, dmgThing);
                                    }
                                }
                            }
                        }
                    }
                    targets = GenRadial.RadialCellsAround(this.realPosition.ToIntVec3(), strikeNum, true);
                    cellList = targets.ToList<IntVec3>();

                }
                if (this.fireVortexValue > 0)
                {
                    if (Find.TickManager.TicksGame % this.fireDelay == 0)
                    {
                        curCell = this.cellList.RandomElement();
                        Vector3 moteVector = GetVector(this.realPosition.ToIntVec3(), curCell);
                        EffectMaker.MakeEffect(ThingDef.Named("Mote_MicroSparks"), this.cellList.RandomElement().ToVector3Shifted(), this.Map, Rand.Range(.5f, 1f), (Quaternion.AngleAxis(Rand.Range(-35, -50), Vector3.up) * moteVector).ToAngleFlat(), Rand.Range(2, 3), Rand.Range(50, 200));
                        EffectMaker.MakeEffect(ThingDef.Named("Mote_MicroSparks"), this.cellList.RandomElement().ToVector3Shifted(), this.Map, Rand.Range(.5f, 1f), (Quaternion.AngleAxis(Rand.Range(35, 50), Vector3.up) * moteVector).ToAngleFlat(), Rand.Range(1, 2), Rand.Range(50, 200));
                        DoFireVortex();
                    }
                }
            }
        }

        private void DoFireVortex()
        {
            IEnumerable<IntVec3> targetsCellsSmall = GenRadial.RadialCellsAround(this.realPosition.ToIntVec3(), 3, true);
            IEnumerable<IntVec3> targetsCells = GenRadial.RadialCellsAround(this.realPosition.ToIntVec3(), 4, true).Except(targetsCellsSmall);
            IntVec3 startCell = targetsCells.RandomElement();
            Vector3 moteVector = GetVector(this.realPosition.ToIntVec3(), startCell);
            Thing launchedThing = new Thing();
            launchedThing.def = WizardryDefOf.FlyingObject_StreamingFlame;
            LaunchFlames(targetsCellsSmall.RandomElement(), startCell + (moteVector * 4).ToIntVec3(), launchedThing);
            this.fireVortexValue -= .4f;
        }

        public void LaunchFlames(IntVec3 startCell, IntVec3 targetCell, Thing thing)
        {
            bool flag = targetCell != null && targetCell != default(IntVec3);
            if (flag)
            {
                if (thing != null)
                {
                    Varda_FlyingObject_StreamingFlame flyingObject = (Varda_FlyingObject_StreamingFlame)GenSpawn.Spawn(ThingDef.Named("FlyingObject_StreamingFlame"), startCell, this.Map);
                    flyingObject.speed = 22;
                    flyingObject.Launch(pawn, targetCell, thing);
                }
            }
        }

        public float CalculateFireAmountInArea(IntVec3 center, float radius)
        {
            float result = 0;
            IntVec3 curCell;
            List<Thing> fireList = this.Map.listerThings.ThingsOfDef(ThingDefOf.Fire);
            IEnumerable<IntVec3> targetCells = GenRadial.RadialCellsAround(center, radius, true);
            for (int i = 0; i < targetCells.Count(); i++)
            {
                curCell = targetCells.ToArray<IntVec3>()[i];
                if (curCell.InBounds(this.Map) && curCell.IsValid)
                {
                    for (int j = 0; j < fireList.Count; j++)
                    {
                        if (fireList[j].Position == curCell)
                        {
                            Fire fire = fireList[j] as Fire;
                            result += fire.fireSize;                            
                            MoteMaker.ThrowSmoke(curCell.ToVector3Shifted(), this.Map, fire.fireSize * 1.5f);
                            fire.Destroy();
                        }
                    }
                }
            }
            return result;
        }

        public void RemoveFireAtPosition(IntVec3 pos)
        {
            GenExplosion.DoExplosion(pos, this.Map, 1, DamageDefOf.Extinguish, this.launcher, 100, SoundDef.Named("ExpandingFlames"), def, this.equipmentDef, null, 0f, 1, false, null, 0f, 1, 0f, false);
        }

        private float AdjustedDistanceFromCenter(float distanceFromCenter)
        {
            float num = Mathf.Min(distanceFromCenter / 8f, 1f);
            num *= num;
            return distanceFromCenter * num;
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
                    flyingObject.Launch(pawn, targetCell, thing);
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

        public void damageEntities(Thing e, float d, DamageDef type)
        {
            int amt = Mathf.RoundToInt(Rand.Range(.75f, 1.25f) * d);
            DamageInfo dinfo = new DamageInfo(type, amt, (float)-1, this.pawn, null, null, DamageInfo.SourceCategory.ThingOrUnknown);
            bool flag = e != null;
            if (flag)
            {
                e.TakeDamage(dinfo);
            }
        }

        private float FadeInOutFactor
        {
            get
            {
                float a = Mathf.Clamp01((float)(this.age) / 120f);
                float b = (this.leftFadeOutTicks >= 0) ? Mathf.Min((float)this.leftFadeOutTicks / 120f, 1f) : 1f;
                return Mathf.Min(a, b);
            }
        }

        private void UpdateSustainerVolume()
        {
            this.sustainer.info.volumeFactor = this.FadeInOutFactor;
        }

        private void CreateSustainer()
        {
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                SoundDef soundDef = SoundDef.Named("Tornado");
                this.sustainer = soundDef.TrySpawnSustainer(SoundInfo.InMap(this, MaintenanceType.PerTick));
                this.UpdateSustainerVolume();
            });
        }
    }
}