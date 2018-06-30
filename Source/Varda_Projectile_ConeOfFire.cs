using System;
using System.Collections.Generic;
using System.Linq;
using AbilityUser;
using UnityEngine;
using Verse;
using RimWorld;

namespace Wizardry
{
    class Varda_Projectile_ConeOfFire : Projectile_AbilityBase
    {

        private int age = -1;
        private bool initialized = false;        
        private float distance = 0;        
        Vector3 direction = default(Vector3);
        Vector3 directionP = default(Vector3);
        Vector3 currentPos = default(Vector3);
        IntVec3 centerCell = default(IntVec3);                     
        private int strikeInt = 0;
        private float fireAmount = 12; //amount of fire to expend, subtracts dropoff amounts for each cell traversed

        //local, unsaved variables
        private int nextStrike = 0;
        private int duration = 180;  //maximum duration, should expend fireAmount before this occurs; this is a backstop/failsafe
        private float branchingFlameDropoff = 0.14f;
        private float mainFlameDropoff = 0.2f;
        private float fireStartChance = .25f;
        private int ticksPerStrike = 1; //how fast flames propogate, lower is faster
        Pawn caster = null;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.initialized, "initialized", false, false);
            Scribe_Values.Look<int>(ref this.age, "age", -1, false);
            Scribe_Values.Look<int>(ref this.strikeInt, "strikeInt", 0, false);
            Scribe_Values.Look<float>(ref this.fireAmount, "fireAmount", 20, false);
            Scribe_Values.Look<float>(ref this.distance, "distance", 0, false);
            Scribe_Values.Look<IntVec3>(ref this.centerCell, "centerCell", default(IntVec3),false);
            Scribe_Values.Look<Vector3>(ref this.direction, "direction", default(Vector3), false);
            Scribe_Values.Look<Vector3>(ref this.directionP, "directionP", default(Vector3), false);
            Scribe_Values.Look<Vector3>(ref this.currentPos, "currentPos", default(Vector3), false);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            bool flag = this.age < duration;
            if (!flag)
            {
                base.Destroy(mode);
            }
        }

        protected override void Impact(Thing hitThing)
        {            
            base.Impact(hitThing);
            ThingDef def = this.def;
            this.caster = this.launcher as Pawn;
            Map map = caster.Map;

            if (!this.initialized)
            {
                this.centerCell = caster.Position;
                this.direction = GetVector(base.Position, false);
                this.nextStrike = this.age + this.ticksPerStrike;
                this.currentPos = this.caster.Position.ToVector3();
                this.currentPos.y = 0;
                this.initialized = true;
            }
            if(this.age > this.nextStrike && fireAmount > 0)
            {
                this.currentPos += this.direction;
                this.nextStrike = this.age + this.ticksPerStrike;
                if (!(currentPos.ToIntVec3().GetTerrain(map).passability == Traversability.Impassable) && currentPos.ToIntVec3().Walkable(map))
                {
                    if (currentPos.ToIntVec3() != this.caster.Position && this.Map != null)
                    {
                        EffectMaker.MakeEffect(WizardryDefOf.Mote_ExpandingFlame, currentPos, this.Map, 1f, (Quaternion.AngleAxis(90, Vector3.up) * direction).ToAngleFlat(), 3f, Rand.Range(200, 500));
                        EffectMaker.MakeEffect(WizardryDefOf.Mote_RecedingFlame, currentPos, this.Map, .8f, (Quaternion.AngleAxis(90, Vector3.up) * direction).ToAngleFlat(), 1f, 0);
                        List<Thing> hitList = currentPos.ToIntVec3().GetThingList(map);
                        Thing burnThing = null;
                        for (int j = 0; j < hitList.Count; j++)
                        {
                            burnThing = hitList[j];
                            damageEntities(burnThing);
                        }
                        //GenExplosion.DoExplosion(this.currentPos.ToIntVec3(), this.Map, .4f, DamageDefOf.Flame, this.launcher, 10, SoundDefOf.ArtilleryShellLoaded, def, this.equipmentDef, null, 0f, 1, false, null, 0f, 1, 0f, false);
                        if (Rand.Chance(this.fireStartChance))
                        {
                            FireUtility.TryStartFireIn(currentPos.ToIntVec3(), map, .2f);
                        }
                        this.fireAmount -= this.mainFlameDropoff;
                        this.strikeInt++;
                        Vector3 tempVec1 = this.currentPos;
                        IntVec3 lastVec1Pos = default(IntVec3);
                        Vector3 tempVec2 = this.currentPos;
                        IntVec3 lastVec2Pos = default(IntVec3);
                        this.distance = Mathf.Max(5f, this.distance);
                        for (float i = (float)strikeInt / this.distance; i > .3f; i -= .5f)
                        {
                            tempVec1 += this.directionP;
                            if (tempVec1.ToIntVec3() != this.currentPos.ToIntVec3() && tempVec1.ToIntVec3() != lastVec1Pos)
                            {
                                lastVec1Pos = tempVec1.ToIntVec3();
                                EffectMaker.MakeEffect(WizardryDefOf.Mote_ExpandingFlame, tempVec1, this.Map, .8f, (Quaternion.AngleAxis(90, Vector3.up) * direction).ToAngleFlat(), 2f, Rand.Range(200, 500));
                                EffectMaker.MakeEffect(WizardryDefOf.Mote_RecedingFlame, tempVec1, this.Map, .7f, (Quaternion.AngleAxis(90, Vector3.up) * direction).ToAngleFlat(), 1f, 0);
                                hitList = lastVec1Pos.GetThingList(map);
                                for (int j = 0; j < hitList.Count; j++)
                                {
                                    burnThing = hitList[j];
                                    damageEntities(burnThing);
                                }
                                if (Rand.Chance(this.fireStartChance))
                                {
                                    FireUtility.TryStartFireIn(lastVec1Pos, map, .2f);
                                }
                                this.fireAmount -= this.branchingFlameDropoff;
                                //GenExplosion.DoExplosion(lastVec1Pos, this.Map, .4f, DamageDefOf.Flame, this.launcher, 10, SoundDefOf.ArtilleryShellLoaded, def, this.equipmentDef, null, 0f, 1, false, null, 0f, 1, 0f, false);
                            }
                            tempVec2 -= this.directionP;
                            if (tempVec2.ToIntVec3() != this.currentPos.ToIntVec3() && tempVec2.ToIntVec3() != lastVec2Pos)
                            {
                                lastVec2Pos = tempVec2.ToIntVec3();
                                EffectMaker.MakeEffect(WizardryDefOf.Mote_ExpandingFlame, tempVec2, this.Map, .8f, (Quaternion.AngleAxis(90, Vector3.up) * direction).ToAngleFlat(), 2f, Rand.Range(200, 500));
                                EffectMaker.MakeEffect(WizardryDefOf.Mote_RecedingFlame, tempVec2, this.Map, .7f, (Quaternion.AngleAxis(90, Vector3.up) * direction).ToAngleFlat(), 1f, 0);
                                hitList = lastVec2Pos.GetThingList(map);
                                for (int j = 0; j < hitList.Count; j++)
                                {
                                    burnThing = hitList[j];
                                    damageEntities(burnThing);
                                }
                                if (Rand.Chance(this.fireStartChance))
                                {
                                    FireUtility.TryStartFireIn(lastVec2Pos, map, .2f);
                                }
                                this.fireAmount -= this.branchingFlameDropoff;
                                //GenExplosion.DoExplosion(lastVec2Pos, this.Map, .4f, DamageDefOf.Flame, this.launcher, 10, SoundDefOf.ArtilleryShellLoaded, def, this.equipmentDef, null, 0f, 1, false, null, 0f, 1, 0f, false);
                            }
                            if (this.fireAmount < 0)
                            {
                                i = 0;
                            }
                        }
                    }
                }
                else
                {
                    //main branch of fire cone hit impassable or unwalkable terrain
                    this.age = this.duration;
                }
            }
        }

        public Vector3 GetVector(IntVec3 closestTarget, bool reverseDirection)
        {
            Vector3 heading = (closestTarget - this.caster.Position).ToVector3();
            if(reverseDirection)
            {
                heading = Quaternion.AngleAxis(180, Vector3.up) * heading;
            }
            this.distance = heading.magnitude;
            Vector3 dirVec = heading / distance;
            this.directionP = Quaternion.AngleAxis(90, Vector3.up) * dirVec;
            return dirVec;
        }

        public void damageEntities(Thing e)
        {            
            int amt = Mathf.RoundToInt(Rand.Range(this.def.projectile.DamageAmount * .75f, this.def.projectile.DamageAmount * 1.25f) + this.fireAmount);
            DamageInfo dinfo = new DamageInfo(DamageDefOf.Flame, amt, 0, (float)-1, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown);
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
    }
}