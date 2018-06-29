using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using AbilityUser;
using UnityEngine;

namespace Wizardry
{
    [StaticConstructorOnStartup]
    public class FlyingObject_Spinning : ThingWithComps
    {
        protected Vector3 origin;
        protected Vector3 destination;
        private int age = -1;
        protected int ticksToImpact;
        private Vector3 flyingDirection = default(Vector3);
        protected Thing launcher;
        protected Thing assignedTarget;
        protected Thing flyingThing;
        public bool damageLaunched = true;
        Pawn pawn;

        public DamageInfo? impactDamage;
        public int duration = 600;
        public float speed = 25f;
        private int rotation = 0;
        private float impactForce = 0;
        private bool earlyImpact = false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<Vector3>(ref this.origin, "origin", default(Vector3), false);
            Scribe_Values.Look<Vector3>(ref this.destination, "destination", default(Vector3), false);
            Scribe_Values.Look<Vector3>(ref this.flyingDirection, "flyingDirection", default(Vector3), false);
            Scribe_Values.Look<int>(ref this.ticksToImpact, "ticksToImpact", 0, false);
            Scribe_Values.Look<int>(ref this.age, "age", 0, false);
            Scribe_Values.Look<bool>(ref this.damageLaunched, "damageLaunched", true, false);
            Scribe_References.Look<Thing>(ref this.assignedTarget, "assignedTarget", false);
            Scribe_References.Look<Thing>(ref this.launcher, "launcher", false);
            Scribe_References.Look<Thing>(ref this.flyingThing, "flyingThing", false);
            Scribe_References.Look<Pawn>(ref this.pawn, "pawn", false);
        }

        protected int StartingTicksToImpact
        {
            get
            {
                int num = Mathf.RoundToInt((this.origin - this.destination).magnitude / (this.speed / 100f));
                bool flag = num < 1;
                if (flag)
                {
                    num = 1;
                }
                return num;
            }
        }

        protected IntVec3 DestinationCell
        {
            get
            {
                return new IntVec3(this.destination);
            }
        }

        public virtual Vector3 ExactPosition
        {
            get
            {
                Vector3 b = (this.destination - this.origin) * (1f - (float)this.ticksToImpact / (float)this.StartingTicksToImpact);
                return this.origin + b + Vector3.up * this.def.Altitude;
            }
        }

        public virtual Quaternion ExactRotation
        {
            get
            {
                return Quaternion.LookRotation(this.destination - this.origin);
            }
        }

        public override Vector3 DrawPos
        {
            get
            {
                return this.ExactPosition;
            }
        }

        private void Initialize()
        {
            if (pawn != null)
            {
                MoteMaker.ThrowDustPuff(pawn.Position, pawn.Map, Rand.Range(1.2f, 1.8f));
            }
            //this.speed;
        }

        public void Launch(Thing launcher, LocalTargetInfo targ, Thing flyingThing, DamageInfo? impactDamage)
        {
            this.Launch(launcher, base.Position.ToVector3Shifted(), targ, flyingThing, impactDamage);
        }

        public void Launch(Thing launcher, LocalTargetInfo targ, Thing flyingThing)
        {
            this.Launch(launcher, base.Position.ToVector3Shifted(), targ, flyingThing, null);
        }

        public void Launch(Thing launcher, Vector3 origin, LocalTargetInfo targ, Thing flyingThing, DamageInfo? newDamageInfo = null)
        {
            this.Initialize();
            bool spawned = flyingThing.Spawned;
            pawn = launcher as Pawn;
            if (spawned)
            {
                flyingThing.DeSpawn();
            }
            
            this.launcher = launcher;
            this.origin = origin;
            this.impactDamage = newDamageInfo;
            this.flyingThing = flyingThing;


            if(targ.Thing != null)
            {
                this.assignedTarget = targ.Thing;
            }
            this.destination = targ.Cell.ToVector3Shifted();
            this.flyingDirection = GetVector(this.origin.ToIntVec3(), this.destination.ToIntVec3());
            this.ticksToImpact = this.StartingTicksToImpact;
        }

        public override void Tick()
        {
            base.Tick();
            this.age++;
            Vector3 exactPosition = this.ExactPosition;
            this.ticksToImpact--;
            bool flag = !this.ExactPosition.InBounds(base.Map);
            if (flag)
            {
                this.ticksToImpact++;
                base.Position = this.ExactPosition.ToIntVec3();
                this.Destroy(DestroyMode.Vanish);
            }
            else if (!this.ExactPosition.ToIntVec3().Walkable(base.Map))
            {
                this.earlyImpact = true;
                this.impactForce = (this.DestinationCell - this.ExactPosition.ToIntVec3()).LengthHorizontal + (this.speed * .2f);
                this.ImpactSomething();
            }
            else
            {
                base.Position = this.ExactPosition.ToIntVec3();
                if (Find.TickManager.TicksGame % 3 == 0)
                {
                    MoteMaker.ThrowDustPuff(base.Position, base.Map, Rand.Range(0.6f, .8f));
                    this.rotation++;
                    if (this.rotation >= 4)
                    {
                        this.rotation = 0;
                    }
                }

                if (Find.TickManager.TicksGame % 12 == 0)
                {
                    DoFlyingObjectDamage();
                }

                bool flag2 = this.ticksToImpact <= 0;
                if (flag2)
                {
                    bool flag3 = this.DestinationCell.InBounds(base.Map);
                    if (flag3)
                    {
                        base.Position = this.DestinationCell;
                    }
                    this.ImpactSomething();         
                }
            }
        }

        public override void Draw()
        {
            bool flag = this.flyingThing != null;
            if (flag)
            {
                if (rotation == 0)
                {
                    this.flyingThing.Rotation = Rot4.West;
                }
                else if (rotation == 1)
                {
                    this.flyingThing.Rotation = Rot4.North;
                }
                else if (rotation == 2)
                {
                    this.flyingThing.Rotation = Rot4.East;
                }
                else
                {
                    this.flyingThing.Rotation = Rot4.South;
                }

                bool flag2 = this.flyingThing is Pawn;
                if (flag2)
                {
                    Vector3 arg_2B_0 = this.DrawPos;
                    bool flag4 = !this.DrawPos.ToIntVec3().IsValid;
                    if (flag4)
                    {
                        return;
                    }
                    Pawn pawn = this.flyingThing as Pawn;
                    pawn.Drawer.DrawAt(this.DrawPos);

                }
                else
                {
                    Graphics.DrawMesh(MeshPool.plane10, this.DrawPos, this.ExactRotation, this.flyingThing.def.DrawMatSingle, 0);
                }
            }
            else
            {
                Graphics.DrawMesh(MeshPool.plane10, this.DrawPos, this.ExactRotation, this.flyingThing.def.DrawMatSingle, 0);
            }
            base.Comps_PostDraw();
        }

        private void DrawEffects(Vector3 pawnVec, Pawn flyingPawn, int magnitude)
        {
            bool flag = !pawn.Dead && !pawn.Downed;
            if (flag)
            {

            }
        }

        private void ImpactSomething()
        {
            bool flag = this.assignedTarget != null;
            if (flag)
            {
                Pawn pawn = this.assignedTarget as Pawn;
                bool flag2 = pawn != null && pawn.GetPosture() != PawnPosture.Standing && (this.origin - this.destination).MagnitudeHorizontalSquared() >= 20.25f && Rand.Value > 0.2f;
                if (flag2)
                {
                    this.Impact(null);
                }
                else
                {
                    this.Impact(this.assignedTarget);
                }
            }
            else
            {
                this.Impact(null);
            }
        }

        protected virtual void Impact(Thing hitThing)
        {
            bool flag = hitThing == null;
            if (flag)
            {
                Pawn pawn;
                bool flag2 = (pawn = (base.Position.GetThingList(base.Map).FirstOrDefault((Thing x) => x == this.assignedTarget) as Pawn)) != null;
                if (flag2)
                {
                    hitThing = pawn;
                }
            }
            bool hasValue = this.impactDamage.HasValue;
            if (hasValue)
            {
                hitThing.TakeDamage(this.impactDamage.Value);
            }
            try
            {
                SoundDefOf.AmbientAltitudeWind.sustainFadeoutTime.Equals(30.0f);

                GenSpawn.Spawn(this.flyingThing, base.Position, base.Map);
                Pawn p = this.flyingThing as Pawn;
                if (this.earlyImpact)
                {
                    damageEntities(p, this.impactForce, DamageDefOf.Blunt);
                    damageEntities(p, 2 * this.impactForce, DamageDefOf.Stun);
                }
                this.Destroy(DestroyMode.Vanish);
            }
            catch
            {
                GenSpawn.Spawn(this.flyingThing, base.Position, base.Map);
                Pawn p = this.flyingThing as Pawn;

                this.Destroy(DestroyMode.Vanish);
            }
        }

        public void DoFlyingObjectDamage()
        {
            float radius = 1f;
            IntVec3 center = this.ExactPosition.ToIntVec3();
            int num = GenRadial.NumCellsInRadius(radius);
            for (int i = 0; i < num; i++)
            {
                IntVec3 intVec = center + GenRadial.RadialPattern[i];
                if (intVec.IsValid && intVec.InBounds(base.Map))
                {
                    List<Thing> hitList = intVec.GetThingList(base.Map);
                    for (int j = 0; j < hitList.Count(); j++)
                    {
                        if (hitList[j] is Pawn)
                        {
                            damageEntities(hitList[j], Rand.Range(6, 9), DamageDefOf.Crush);
                            MoteMaker.ThrowMicroSparks(hitList[j].DrawPos, hitList[j].Map);
                        }
                        else if (hitList[j] is Building)
                        {
                            damageEntities(hitList[j], Rand.Range(9, 16), DamageDefOf.Crush);
                            MoteMaker.ThrowMicroSparks(hitList[j].DrawPos, hitList[j].Map);
                        }

                    }
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
    }
}
