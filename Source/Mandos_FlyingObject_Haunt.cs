using RimWorld;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using AbilityUser;

namespace Wizardry
{
    [StaticConstructorOnStartup]
    public class Mandos_FlyingObject_Haunt : ThingWithComps
    {
        protected Vector3 origin;
        protected Vector3 destination;
        protected Vector3 trueOrigin;
        protected Vector3 trueDestination;

        public float speed = 30f;
        protected int ticksToImpact;
        protected Thing launcher;
        protected Thing assignedTarget;
        protected Pawn assignedTargetPawn = null;
        protected Thing flyingThing;

        public ThingDef moteDef = null;
        public int moteFrequency = 0;

        public bool spinning = false;
        public float curveVariance = 0; // 0 = no curve
        public int variancePoints = 20;
        private List<Vector3> curvePoints = new List<Vector3>();
        public float force = 1f;
        private int destinationCurvePoint = 0;
        private float impactRadius = 0;
        private int explosionDamage;
        private bool isExplosive = false;
        private DamageDef impactDamageType = null;
        private int attackFrequency = 30;
        private float attackRadius = 1f;        

        private bool earlyImpact = false;
        private float impactForce = 0;
        private bool isCircling = false;
        private int duration = 1200;
        private int age = -1;
        private bool shiftRight = false;
        private bool curveDir = false;

        public DamageInfo? impactDamage;

        public bool damageLaunched = true;
        public bool explosion = false;
        public int weaponDmg = 0;

        Pawn pawn;

        //Magic related

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

        public virtual float ExactRotationAngle
        {
            get
            {
                return (Quaternion.AngleAxis(90, Vector3.up) * (this.destination - this.origin)).ToAngleFlat();
            }
        }

        public override Vector3 DrawPos
        {
            get
            {
                return this.ExactPosition;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<Vector3>(ref this.origin, "origin", default(Vector3), false);
            Scribe_Values.Look<Vector3>(ref this.destination, "destination", default(Vector3), false);
            Scribe_Values.Look<int>(ref this.ticksToImpact, "ticksToImpact", 0, false);
            Scribe_Values.Look<int>(ref this.age, "age", -1, false);
            Scribe_Values.Look<bool>(ref this.damageLaunched, "damageLaunched", true, false);
            Scribe_Values.Look<bool>(ref this.explosion, "explosion", false, false);
            Scribe_References.Look<Thing>(ref this.assignedTarget, "assignedTarget", false);
            Scribe_References.Look<Pawn>(ref this.assignedTargetPawn, "assignedTargetPawn", false);
            Scribe_References.Look<Thing>(ref this.launcher, "launcher", false);
            Scribe_Deep.Look<Thing>(ref this.flyingThing, "flyingThing", new object[0]);
            Scribe_References.Look<Pawn>(ref this.pawn, "pawn", false);
        }

        private void Initialize()
        {
            if (pawn != null)
            {
                MoteMaker.ThrowDustPuff(pawn.Position, pawn.Map, Rand.Range(1.2f, 1.8f));
            }
            else
            {
                flyingThing.ThingID += Rand.Range(0, 214).ToString();
            }
        }

        public void Launch(Thing launcher, LocalTargetInfo targ, Thing flyingThing, DamageInfo? impactDamage)
        {
            this.Launch(launcher, base.Position.ToVector3Shifted(), targ, flyingThing, impactDamage);
        }

        public void Launch(Thing launcher, LocalTargetInfo targ, Thing flyingThing)
        {
            this.Launch(launcher, base.Position.ToVector3Shifted(), targ, flyingThing, null);
        }

        public void AdvancedLaunch(Thing launcher, ThingDef effectMote, int moteFrequencyTicks, float curveAmount, int variancePoints, bool shouldSpin, Vector3 origin, LocalTargetInfo targ, Thing flyingThing, int flyingSpeed, bool isExplosion, int attackFrequency, float attackRadius, int _impactDamage, float _impactRadius, DamageDef damageType, DamageInfo? newDamageInfo = null)
        {
            this.explosionDamage = _impactDamage;
            this.isExplosive = isExplosion;
            this.impactRadius = _impactRadius;
            this.impactDamageType = damageType;
            this.attackFrequency = attackFrequency;
            this.attackRadius = attackRadius;
            this.moteFrequency = moteFrequencyTicks;
            this.moteDef = effectMote;
            this.curveVariance = curveAmount;
            this.variancePoints = variancePoints;
            this.spinning = shouldSpin;
            this.speed = flyingSpeed;
            this.curvePoints = new List<Vector3>();
            this.curvePoints.Clear();
            this.Launch(launcher, origin, targ, flyingThing, newDamageInfo);
        }

        public void Launch(Thing launcher, Vector3 origin, LocalTargetInfo targ, Thing flyingThing, DamageInfo? newDamageInfo = null)
        {
            bool spawned = flyingThing.Spawned;
            this.pawn = launcher as Pawn;
            if (spawned)
            {
                flyingThing.DeSpawn();
            }
            this.launcher = launcher;
            this.trueOrigin = origin;
            this.trueDestination = targ.Cell.ToVector3();
            this.impactDamage = newDamageInfo;
            this.flyingThing = flyingThing;
            bool flag = targ.Thing != null;
            if (flag)
            {
                this.assignedTarget = targ.Thing;
                if(this.assignedTarget is Pawn)
                {
                    this.assignedTargetPawn = targ.Thing as Pawn;
                }                       
            }
            if(targ.Cell.x > launcher.Position.x)
            {
                shiftRight = true;
            }
            this.speed = this.speed * this.force;
            if(Rand.Chance(.5f))
            {
                this.curveDir = true;
            }
            this.origin = origin;
            if (this.curveVariance > 0)
            {
                CalculateCurvePoints(this.trueOrigin, this.trueDestination, this.curveVariance);
                this.destinationCurvePoint++;
                this.destination = this.curvePoints[this.destinationCurvePoint];
            }
            else
            {
                this.destination = this.trueDestination;
            }
            this.ticksToImpact = this.StartingTicksToImpact;
            this.Initialize();
        }

        public void CalculateCurvePoints(Vector3 start, Vector3 end, float variance)
        {            
            Vector3 initialVector = GetVector(start, end);
            initialVector.y = 0;
            float initialAngle = (initialVector).ToAngleFlat();
            float curveAngle = 0;
            this.destinationCurvePoint = 0;
            this.curvePoints.Clear();            
            if (this.curveDir)
            {
                curveAngle = variance;
            }
            else
            {
                curveAngle = (-1) * variance;
            }
            //calculate extra distance bolt travels around the ellipse
            float a = .5f * Vector3.Distance(start, end);
            float b = a * Mathf.Sin(.5f * Mathf.Deg2Rad * variance);
            float p = .5f * Mathf.PI * (3 * (a + b) - (Mathf.Sqrt((3 * a + b) * (a + 3 * b))));

            float incrementalDistance = p / variancePoints;
            float incrementalAngle = (curveAngle / variancePoints) * 2;
            this.curvePoints.Add(start);
            for (int i = 1; i < variancePoints; i++)
            {
                this.curvePoints.Add(this.curvePoints[i - 1] + ((Quaternion.AngleAxis(curveAngle, Vector3.up) * initialVector) * incrementalDistance));
                curveAngle -= incrementalAngle;
            }
        }

        public Vector3 GetVector(Vector3 center, Vector3 objectPos)
        {
            Vector3 heading = (objectPos - center);
            float distance = heading.magnitude;
            Vector3 direction = heading / distance;
            return direction;
        }

        public override void Tick()
        {
            base.Tick();
            Vector3 exactPosition = this.ExactPosition;
            if (this.ticksToImpact >= 0 && this.moteDef != null && Find.TickManager.TicksGame % this.moteFrequency == 0)
            {
                DrawEffects(exactPosition);
            }
            if(this.isCircling && this.attackFrequency != 0 && Find.TickManager.TicksGame % this.attackFrequency == 0)
            {
                if (this.pawn.Destroyed || this.pawn.Dead)
                {
                    this.age = this.duration;
                }
                else
                {
                    DoFlyingObjectDamage();
                    if (this.assignedTargetPawn.Dead)
                    {
                        this.age = this.duration;
                    }
                }
            }
            this.ticksToImpact--;
            this.age++;
            bool flag = !this.ExactPosition.InBounds(base.Map);
            if (flag)
            {
                this.ticksToImpact++;
                base.Position = this.ExactPosition.ToIntVec3();
                this.Destroy(DestroyMode.Vanish);
            }
            else if (!this.ExactPosition.ToIntVec3().Walkable(base.Map) && !this.isCircling)
            {
                this.earlyImpact = true;
                this.impactForce = (this.DestinationCell - this.ExactPosition.ToIntVec3()).LengthHorizontal + (this.speed * .2f);
                this.ImpactSomething();
            }
            else
            {
                base.Position = this.ExactPosition.ToIntVec3();

                bool flag2 = this.ticksToImpact <= 0;
                if (flag2)
                {
                    if (this.curveVariance > 0)
                    {
                        if ((this.curvePoints.Count() - 1) > this.destinationCurvePoint)
                        {
                            this.origin = curvePoints[destinationCurvePoint];
                            this.destinationCurvePoint++;
                            this.destination = this.curvePoints[this.destinationCurvePoint];
                            this.ticksToImpact = this.StartingTicksToImpact;
                        }
                        else
                        {
                            bool flag3 = this.DestinationCell.InBounds(base.Map);
                            if (flag3)
                            {
                                base.Position = this.DestinationCell;
                            }                            
                            this.isCircling = true;
                            this.variancePoints = 10;
                            this.curveVariance = 60;
                            this.speed = 10;
                            this.moteFrequency = 4;
                            NewSemiCircle();                            
                        }
                    }
                    else
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
            if(this.age > this.duration)
            {
                this.Destroy(DestroyMode.Vanish);
            }
        }

        public void NewSemiCircle()
        {
            this.origin = this.destination;
            Vector3 targetShifted = this.assignedTarget.DrawPos;
            if(shiftRight)
            {
                targetShifted.x += 1f;
            }
            else
            {
                targetShifted.x -= 1f;
            }
            this.shiftRight = !this.shiftRight;
            CalculateCurvePoints(this.origin, targetShifted, this.curveVariance);
            this.destinationCurvePoint++;
            this.destination = this.curvePoints[this.destinationCurvePoint];
            this.ticksToImpact = this.StartingTicksToImpact;
        }

        public void DoFlyingObjectDamage()
        {
            IntVec3 center = this.ExactPosition.ToIntVec3();
            int num = GenRadial.NumCellsInRadius(this.attackRadius);
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
                            if (hitList[j].Faction != this.pawn.Faction)
                            {
                                DamageEntities(hitList[j], WizardryDefOf.LotRW_HauntDD.defaultDamage, WizardryDefOf.LotRW_HauntDD);
                            }
                        }
                    }
                }
            }
        }

        public override void Draw()
        {
            bool flag = this.flyingThing != null;
            if (flag)
            {
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

        private void DrawEffects(Vector3 effectVec)
        {
            effectVec.x += Rand.Range(-0.4f, 0.4f);
            effectVec.z += Rand.Range(-0.4f, 0.4f);
            if (isCircling)
            {
                if (this.shiftRight)
                {
                    EffectMaker.MakeEffect(this.moteDef, effectVec, this.Map, Rand.Range(.2f, .3f), this.ExactRotationAngle + 90, Rand.Range(1, 1.5f), Rand.Range(-200, 200), .1f, 0f, Rand.Range(.2f, .25f), false);
                }
                else
                {
                    EffectMaker.MakeEffect(this.moteDef, effectVec, this.Map, Rand.Range(.2f, .3f), this.ExactRotationAngle - 90, Rand.Range(1, 1.5f), Rand.Range(-200, 200), .1f, 0f, Rand.Range(.2f, .25f), false);
                }
            }
            else
            {
                EffectMaker.MakeEffect(this.moteDef, effectVec, this.Map, Rand.Range(.1f, .2f), this.ExactRotationAngle, Rand.Range(10, 15), Rand.Range(-100, 100), .15f, 0f, Rand.Range(.2f, .3f), false);
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
            if (this.flyingThing is Pawn)
            {
                try
                {
                    SoundDefOf.Ambient_AltitudeWind.sustainFadeoutTime.Equals(30.0f);

                    GenSpawn.Spawn(this.flyingThing, base.Position, base.Map);
                    Pawn p = this.flyingThing as Pawn;
                    if (this.earlyImpact)
                    {
                        DamageEntities(p, this.impactForce, DamageDefOf.Blunt);
                        DamageEntities(p, 2 * this.impactForce, DamageDefOf.Stun);
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
            else
            {
                if (this.impactRadius > 0)
                {
                    if (this.isExplosive)
                    {
                        GenExplosion.DoExplosion(this.ExactPosition.ToIntVec3(), this.Map, this.impactRadius, this.impactDamageType, this.launcher as Pawn, this.explosionDamage, -1, this.impactDamageType.soundExplosion, def, null, null, null, 0f, 1, false, null, 0f, 0, 0.0f, true);
                    }
                    else
                    {
                        int num = GenRadial.NumCellsInRadius(this.impactRadius);
                        IntVec3 curCell;
                        for (int i = 0; i < num; i++)
                        {
                            curCell = this.ExactPosition.ToIntVec3() + GenRadial.RadialPattern[i];
                            List<Thing> hitList = new List<Thing>();
                            hitList = curCell.GetThingList(this.Map);
                            for (int j = 0; j < hitList.Count; j++)
                            {
                                if (hitList[j] is Pawn && hitList[j] != this.pawn)
                                {
                                    DamageEntities(hitList[j], this.explosionDamage, this.impactDamageType);
                                }
                            }
                        }
                    }
                }
                this.Destroy(DestroyMode.Vanish);
            }
        }

        public void DamageEntities(Thing e, float d, DamageDef type)
        {
            int amt = Mathf.RoundToInt(Rand.Range(.75f, 1.25f) * d);
            DamageInfo dinfo = new DamageInfo(type, amt, 0, (float)-1, this.pawn, null, null, DamageInfo.SourceCategory.ThingOrUnknown);
            bool flag = e != null;
            if (flag)
            {
                e.TakeDamage(dinfo);
            }
        }
    }
}