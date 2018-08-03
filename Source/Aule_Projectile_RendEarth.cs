using System;
using System.Collections.Generic;
using System.Linq;
using AbilityUser;
using UnityEngine;
using Verse;
using RimWorld;

namespace Wizardry
{
    [StaticConstructorOnStartup]
    class Aule_Projectile_RendEarth : Projectile_AbilityBase
    {
        private int age = -1;
        private bool initialized = false;
        private float distance = 0;
        private float angle = 0;               
        private int strikeCount = 0;
        private float maxRange = 0;
        private int approximateDuration = 0;
        private bool wallImpact = false;
        IntVec3 boltPosition = default(IntVec3);
        IntVec3 boltOrigin = default(IntVec3);
        Vector3 direction = default(Vector3);        

        //local, unsaved variables
        private int duration = 600;  //maximum duration as a failsafe
        private int fadeTicks = 300;
        private int ticksPerStrike = 5;
        private int nextStrike = 0;
        private float boltTravelRate = 2f;
        Pawn caster = null;

        private int boltMaxCount = 20;
        private float boltRange = 100;

        private Mesh boltMesh = null;
        private static List<Mesh> boltMeshes = new List<Mesh>();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.initialized, "initialized", false, false);
            Scribe_Values.Look<bool>(ref this.wallImpact, "wallImpact", false, false);
            Scribe_Values.Look<int>(ref this.age, "age", -1, false);
            Scribe_Values.Look<int>(ref this.duration, "duration", 0, false);
            Scribe_Values.Look<int>(ref this.approximateDuration, "approximateDuration", 0, false);
            Scribe_Values.Look<int>(ref this.strikeCount, "strikeCount", 0, false);
            Scribe_Values.Look<float>(ref this.distance, "distance", 0, false);
            Scribe_Values.Look<float>(ref this.angle, "angle", 0, false);
            Scribe_Values.Look<IntVec3>(ref this.boltOrigin, "boltOrigin", default(IntVec3), false);
            Scribe_Values.Look<IntVec3>(ref this.boltPosition, "boltPosition", default(IntVec3), false);
            Scribe_Values.Look<Vector3>(ref this.direction, "direction", default(Vector3), false);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            bool flag = this.age < (duration + fadeTicks);
            if (!flag)
            {
                Aule_Projectile_RendEarth.boltMeshes.Clear();
                base.Destroy(mode);
            }
        }

        protected override void Impact(Thing hitThing)
        {
            base.Impact(hitThing);
            ThingDef def = this.def;
            this.caster = this.launcher as Pawn;
            if (this.Map != null)
            {
                if (!this.initialized)
                {
                    this.direction = GetVector(this.caster.Position, base.Position);
                    this.angle = (Quaternion.AngleAxis(90, Vector3.up) * this.direction).ToAngleFlat();
                    this.boltOrigin = this.caster.Position + (2f * this.direction).ToIntVec3();                    
                    this.maxRange = (this.boltOrigin - base.Position).LengthHorizontal;
                    this.boltMaxCount = Mathf.RoundToInt(this.maxRange);
                    this.approximateDuration = (int)((this.maxRange / this.boltTravelRate) * this.ticksPerStrike * 1.25f);                    
                    this.boltMesh = this.RandomBoltMesh;
                    this.initialized = true;                    
                }                

                if (this.nextStrike < this.age && this.age < this.duration && !this.wallImpact)
                {
                    this.strikeCount++;
                    this.boltMesh = null;
                    this.boltPosition = (this.boltOrigin + (this.boltTravelRate * this.strikeCount * this.direction).ToIntVec3());
                    this.boltRange = (this.boltOrigin - this.boltPosition).LengthHorizontal;
                    this.boltMaxCount = Mathf.RoundToInt(this.boltRange);
                    this.boltMesh = this.RandomBoltMesh;                    
                    this.nextStrike = this.age + this.ticksPerStrike;
                    DoQuakeDamages(2, this.boltPosition);
                    
                    if (Rand.Chance(.6f))
                    {
                        IntVec3 smallMeshDestination;
                        if (Rand.Chance(.5f))
                        {
                            smallMeshDestination = (this.boltPosition + ((Quaternion.AngleAxis(Rand.Range(30, 60), Vector3.up) * direction) * Rand.Range(2f, 5f)).ToIntVec3());
                        }
                        else
                        {
                            smallMeshDestination = (this.boltPosition + ((Quaternion.AngleAxis(Rand.Range(-30, -60), Vector3.up) * direction) * Rand.Range(2f, 5f)).ToIntVec3());
                        }

                        Map.weatherManager.eventHandler.AddEvent(new MeshMaker(this.Map, MatPool.rendEarthMat7, this.boltPosition, smallMeshDestination, Rand.Range(2f, 6f), AltitudeLayer.Floor, this.approximateDuration - this.age, this.fadeTicks, 10));
                        DoQuakeDamages(1.4f, smallMeshDestination);                       
                    }

                    if (this.maxRange < (this.boltOrigin - this.boltPosition).LengthHorizontal)
                    {
                        this.wallImpact = true;
                        this.duration = this.approximateDuration;
                    }
                }
                DrawStrike(this.boltOrigin, this.boltPosition.ToVector3());
            }
        }

        private void DoQuakeDamages(float radius, IntVec3 location)
        {
            int num = GenRadial.NumCellsInRadius(radius);
            for (int i = 0; i < num; i++)
            {
                Building structure = null;
                Pawn pawn = null;
                IntVec3 intVec = location + GenRadial.RadialPattern[i];
                structure = null;
                if (intVec.IsValid && intVec.InBounds(this.Map))
                {
                    if (Rand.Chance(.4f))
                    {
                        EffectMaker.MakeEffect(ThingDef.Named("Mote_ThickDust"), intVec.ToVector3Shifted(), this.Map, Rand.Range(.2f, 2f), Rand.Range(0, 360), Rand.Range(.5f, 1f), Rand.Range(10, 250), 0, Rand.Range(.3f, .9f), Rand.Range(.05f, .3f), Rand.Range(.6f, 2.4f), true);
                    }

                    structure = intVec.GetFirstBuilding(this.Map);
                    if (structure != null)
                    {
                        if (structure.def.designationCategory == DesignationCategoryDefOf.Structure)
                        {
                            DamageEntities(structure, structure.def.BaseMaxHitPoints, DamageDefOf.Crush);
                            Vector3 moteDirection = GetVector(this.origin.ToIntVec3(), intVec);
                            EffectMaker.MakeEffect(ThingDef.Named("Mote_Rubble"), intVec.ToVector3Shifted(), base.Map, Rand.Range(.3f, .5f), (Quaternion.AngleAxis(90, Vector3.up) * moteDirection).ToAngleFlat(), 8f, 0);
                            EffectMaker.MakeEffect(ThingDef.Named("Mote_Rubble"), intVec.ToVector3Shifted(), base.Map, Rand.Range(.3f, .6f), (Quaternion.AngleAxis(Rand.Range(-70, -110), Vector3.up) * moteDirection).ToAngleFlat(), 6f, 0);
                            GenExplosion.DoExplosion(intVec, base.Map, .4f, WizardryDefOf.LotRW_RockFragments, this.launcher, Rand.Range(6, 16), 0, SoundDefOf.Pawn_Melee_Punch_HitBuilding, null, null, null, ThingDef.Named("Filth_RockRubble"), .4f, 1, false, null, 0f, 1, 0, false);
                            MoteMaker.ThrowSmoke(intVec.ToVector3Shifted(), base.Map, Rand.Range(.6f, 1f));
                            if (intVec == this.boltPosition)
                            {
                                this.wallImpact = true;
                                this.duration = this.approximateDuration;
                            }
                        }
                        else if (structure.def.building.isResourceRock)
                        {
                            ThingDef yieldThing = structure.def.building.mineableThing;
                            int yieldAmount = (int)(structure.def.building.mineableYield * Rand.Range(.7f, .9f));
                            DamageEntities(structure, structure.def.BaseMaxHitPoints, DamageDefOf.Crush);
                            Vector3 moteDirection = GetVector(this.origin.ToIntVec3(), intVec);
                            EffectMaker.MakeEffect(ThingDef.Named("Mote_Rubble"), intVec.ToVector3Shifted(), base.Map, Rand.Range(.3f, .5f), (Quaternion.AngleAxis(90, Vector3.up) * moteDirection).ToAngleFlat(), 8f, 0);
                            EffectMaker.MakeEffect(ThingDef.Named("Mote_Rubble"), intVec.ToVector3Shifted(), base.Map, Rand.Range(.3f, .6f), (Quaternion.AngleAxis(Rand.Range(-70, -110), Vector3.up) * moteDirection).ToAngleFlat(), 6f, 0);
                            GenExplosion.DoExplosion(intVec, base.Map, .4f, WizardryDefOf.LotRW_RockFragments, this.launcher, Rand.Range(6, 16), 0, SoundDefOf.Crunch, null, null, null, yieldThing, 1f, yieldAmount, false, null, 0f, 1, 0, false);
                            MoteMaker.ThrowSmoke(intVec.ToVector3Shifted(), base.Map, Rand.Range(.6f, 1f));
                            if (intVec == this.boltPosition)
                            {
                                this.wallImpact = true;
                                this.duration = this.approximateDuration;
                            }
                        }
                        else if (structure.def.building.isNaturalRock)
                        {
                            ThingDef yieldThing = structure.def.building.mineableThing;
                            DamageEntities(structure, structure.def.BaseMaxHitPoints, DamageDefOf.Crush);
                            Vector3 moteDirection = GetVector(this.origin.ToIntVec3(), intVec);
                            EffectMaker.MakeEffect(ThingDef.Named("Mote_Rubble"), intVec.ToVector3Shifted(), base.Map, Rand.Range(.3f, .5f), (Quaternion.AngleAxis(90, Vector3.up) * moteDirection).ToAngleFlat(), 8f, 0);
                            EffectMaker.MakeEffect(ThingDef.Named("Mote_Rubble"), intVec.ToVector3Shifted(), base.Map, Rand.Range(.3f, .6f), (Quaternion.AngleAxis(Rand.Range(-70, -110), Vector3.up) * moteDirection).ToAngleFlat(), 6f, 0);
                            GenExplosion.DoExplosion(intVec, base.Map, .4f, WizardryDefOf.LotRW_RockFragments, this.launcher, Rand.Range(6, 16), 0, SoundDefOf.Crunch, null, null, null, yieldThing, .2f, 1, false, null, 0f, 1, 0, false);
                            MoteMaker.ThrowSmoke(intVec.ToVector3Shifted(), base.Map, Rand.Range(.6f, 1f));
                            if (intVec == this.boltPosition)
                            {
                                this.wallImpact = true;
                                this.duration = this.approximateDuration;
                            }
                        }
                        else
                        {
                            DamageEntities(structure, Rand.Range(40, 50), DamageDefOf.Crush);
                            EffectMaker.MakeEffect(ThingDef.Named("Mote_Rubble"), intVec.ToVector3Shifted(), base.Map, Rand.Range(.3f, .6f), Rand.Range(0, 359), 6f, 0);
                            EffectMaker.MakeEffect(ThingDef.Named("Mote_Rubble"), intVec.ToVector3Shifted(), base.Map, Rand.Range(.3f, .6f), Rand.Range(0, 359), 4f, 0);
                            MoteMaker.ThrowSmoke(intVec.ToVector3Shifted(), base.Map, Rand.Range(.6f, 1f));
                        }
                    }

                    pawn = intVec.GetFirstPawn(this.Map);
                    if (pawn != null && pawn != this.caster)
                    {
                        if (Rand.Chance(.2f))
                        {
                            DamageEntities(pawn, Rand.Range(4, 6), DamageDefOf.Crush);
                        }
                        HealthUtility.AdjustSeverity(pawn, HediffDef.Named("LotRW_Quake"), Rand.Range(1f, 2f));
                    }
                }
            }            
        }

        public void DrawStrike(IntVec3 start, Vector3 dest)
        {
            if (this.boltOrigin != default(IntVec3))
            {
                float magnitude = (this.boltPosition.ToVector3Shifted() - Find.Camera.transform.position).magnitude;
                if (this.age <= this.duration)
                {                    
                    Find.CameraDriver.shaker.DoShake(20 / magnitude);
                    Graphics.DrawMesh(this.boltMesh, this.boltOrigin.ToVector3ShiftedWithAltitude(AltitudeLayer.Floor), Quaternion.Euler(0f, this.angle, 0f), FadedMaterialPool.FadedVersionOf(MatPool.rendEarthMat7, 1), 0);
                }
                else
                {
                    Graphics.DrawMesh(this.boltMesh, this.boltOrigin.ToVector3ShiftedWithAltitude(AltitudeLayer.Floor), Quaternion.Euler(0f, this.angle, 0f), FadedMaterialPool.FadedVersionOf(MatPool.rendEarthMat7, this.MeshBrightness), 0);
                }                
            }
        }

        protected float MeshBrightness
        {
            get
            {
                return 1f - ((float)(this.age - this.duration) / this.fadeTicks);
            }
        }

        public Vector3 GetVector(IntVec3 center, IntVec3 objectPos)
        {
            Vector3 heading = (objectPos - center).ToVector3();
            float distance = heading.magnitude;
            Vector3 direction = heading / distance;            
            return direction;
        }

        public void DamageEntities(Thing e, int amt, DamageDef damageType)
        {            
            DamageInfo dinfo = new DamageInfo(damageType, amt, 0, (float)-1, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown);
            
            bool flag = e != null;
            if (flag)
            {
                e.TakeDamage(dinfo);
            }
        }

        public override void Tick()
        {
            base.Tick();
            this.age++;
        }

        public Mesh RandomBoltMesh
        {
            get
            {
                Mesh result;
                Aule_Projectile_RendEarth.boltMeshes.Clear();
                if (Aule_Projectile_RendEarth.boltMeshes.Count < this.boltMaxCount)
                {
                    Mesh mesh = Effect_MeshMaker.NewBoltMesh(this.boltRange, 0);
                    Aule_Projectile_RendEarth.boltMeshes.Add(mesh);
                    result = mesh;
                }
                else
                {
                    result = Aule_Projectile_RendEarth.boltMeshes.RandomElement<Mesh>();
                }
                return result;
            }
        }
    }
}