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
    public class Ulmo_FlyingObject_RainDance : ThingWithComps
    {
        protected Vector3 trueOrigin;
        protected Vector3 origin;
        protected Vector3 destination;
        protected Vector3 swapVector;

        protected float speed = 4f;
        private int rotationRate = 8;
        private bool midPoint = false;
        private bool drafted = false;
        private int rotation = 0;
        private bool rainStarted = false;

        private int circleIndex =0;
        List<IntVec3> rotationCircle;
        protected int ticksToImpact;

        protected Thing launcher;

        protected Thing assignedTarget;

        protected Thing flyingThing;

        public DamageInfo? impactDamage;

        public bool damageLaunched = true;

        public bool explosion = false;

        public int weaponDmg = 0;

        Pawn pawn;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.circleIndex, "circleIndex", 0, false);
            Scribe_Values.Look<bool>(ref this.midPoint, "midPoint", false, false);
            Scribe_Values.Look<bool>(ref this.rainStarted, "rainStarted", false, false);
            Scribe_Collections.Look<IntVec3>(ref this.rotationCircle, "rotationCircle", LookMode.Value);
            Scribe_Values.Look<Vector3>(ref this.origin, "origin", default(Vector3), false);
            Scribe_Values.Look<Vector3>(ref this.destination, "destination", default(Vector3), false);
            Scribe_Values.Look<Vector3>(ref this.trueOrigin, "trueOrigin", default(Vector3), false);
            Scribe_Values.Look<int>(ref this.ticksToImpact, "ticksToImpact", 0, false);
            Scribe_Values.Look<bool>(ref this.damageLaunched, "damageLaunched", true, false);
            Scribe_Values.Look<bool>(ref this.explosion, "explosion", false, false);
            Scribe_References.Look<Thing>(ref this.assignedTarget, "assignedTarget", false);
            Scribe_References.Look<Thing>(ref this.launcher, "launcher", false);
            Scribe_References.Look<Pawn>(ref this.pawn, "pawn", false);
            Scribe_References.Look<Thing>(ref this.flyingThing, "flyingThing", false);
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
                MoteMaker.MakeStaticMote(pawn.TrueCenter(), pawn.Map, ThingDefOf.Mote_ExplosionFlash, 12f);
                SoundDefOf.AmbientAltitudeWind.sustainFadeoutTime.Equals(30.0f);
                MoteMaker.ThrowDustPuff(pawn.Position, pawn.Map, Rand.Range(1.2f, 1.8f));
            }
            IEnumerable<IntVec3> innerCircle = GenRadial.RadialCellsAround(this.Position, Mathf.RoundToInt((this.destination - this.trueOrigin).MagnitudeHorizontal()+1), true);
            IEnumerable<IntVec3> outerCircle = GenRadial.RadialCellsAround(this.Position, Mathf.RoundToInt((this.destination - this.trueOrigin).MagnitudeHorizontal()+2), true);
            this.rotationCircle = outerCircle.Except(innerCircle).ToList<IntVec3>();
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

            bool spawned = flyingThing.Spawned;
            pawn = launcher as Pawn;
            drafted = pawn.Drafted;
            this.Initialize();
            if (spawned)
            {
                flyingThing.DeSpawn();
            }
            this.launcher = launcher;
            this.trueOrigin = origin;
            this.origin = origin;
            this.impactDamage = newDamageInfo;
            this.flyingThing = flyingThing;

            bool flag = targ.Thing != null;
            if (flag)
            {
                this.assignedTarget = targ.Thing;
            }
            this.destination = targ.Cell.ToVector3Shifted() + new Vector3(Rand.Range(-0.3f, 0.3f), 0f, Rand.Range(-0.3f, 0.3f));
            this.ticksToImpact = this.StartingTicksToImpact;

        }

        public override void Tick()
        {
            base.Tick();
            Vector3 exactPosition = this.ExactPosition;
            this.ticksToImpact--;
            bool flag = !this.ExactPosition.InBounds(base.Map);
            if (flag)
            {
                this.ticksToImpact++;
                base.Position = this.ExactPosition.ToIntVec3();
                this.Destroy(DestroyMode.Vanish);
            }
            else
            {
                base.Position = this.ExactPosition.ToIntVec3();
                if (Find.TickManager.TicksGame % rotationRate == 0)
                {
                    this.rotation++;
                    if (this.rotation >= 4)
                    {
                        this.rotation = 0;
                    }                   
                    EffectMaker.MakeEffect(WizardryDefOf.Mote_Sparks, this.DrawPos, this.Map, Rand.Range(.3f,.5f), (this.rotation *90) + Rand.Range(-45, 45), Rand.Range(2, 3), Rand.Range(100, 200));
                }

                bool flag2 = this.ticksToImpact <= 0;
                if (flag2)
                {
                    bool flag3 = this.DestinationCell.InBounds(base.Map);
                    if (flag3)
                    {
                        base.Position = this.DestinationCell;
                    }

                    if(midPoint)
                    {
                        this.ImpactSomething();
                    }
                    else
                    {
                        this.ChangeDirection();
                        if(!rainStarted)
                        {
                            StartWeatherEffects();
                        }
                    }
                }

            }
        }

        private void StartWeatherEffects()
        {
            Map map = this.Map;
            this.rainStarted = true;
            WeatherDef rainMakerDef = new WeatherDef();
            if (map.mapTemperature.OutdoorTemp < 0)
            {
                if (map.weatherManager.curWeather.defName == "SnowHard" || map.weatherManager.curWeather.defName == "SnowGentle")
                {
                    rainMakerDef = WeatherDef.Named("Clear");
                    map.weatherManager.TransitionTo(rainMakerDef);
                }
                else
                {
                    if (Rand.Chance(.5f))
                    {
                        rainMakerDef = WeatherDef.Named("SnowGentle");
                    }
                    else
                    {
                        rainMakerDef = WeatherDef.Named("SnowHard");
                    }
                    map.weatherDecider.DisableRainFor(0);
                    map.weatherManager.TransitionTo(rainMakerDef);
                }
            }
            else
            {
                if (map.weatherManager.curWeather.defName == "Rain" || map.weatherManager.curWeather.defName == "RainyThunderstorm" || map.weatherManager.curWeather.defName == "FoggyRain")
                {
                    rainMakerDef = WeatherDef.Named("Clear");
                    map.weatherDecider.DisableRainFor(4000);
                    map.weatherManager.TransitionTo(rainMakerDef);
                }
                else
                {
                    int rnd = Rand.RangeInclusive(1, 3);
                    switch (rnd)
                    {
                        case 1:
                            rainMakerDef = WeatherDef.Named("Rain");
                            break;
                        case 2:
                            rainMakerDef = WeatherDef.Named("RainyThunderstorm");
                            break;
                        case 3:
                            rainMakerDef = WeatherDef.Named("FoggyRain");
                            break;
                    }
                    map.weatherDecider.DisableRainFor(0);
                    map.weatherManager.TransitionTo(rainMakerDef);
                }
            }
        }

        private void ChangeDirection()
        {
            circleIndex++;
            if(circleIndex >= rotationCircle.Count())
            {
                this.origin = this.destination;
                this.destination = this.trueOrigin;
                this.ticksToImpact = this.StartingTicksToImpact;
                this.midPoint = true;
            }
            else
            {
                this.swapVector = this.destination;
                this.destination = rotationCircle.ToArray<IntVec3>()[circleIndex].ToVector3Shifted();
                this.origin = this.swapVector;
                this.ticksToImpact = this.StartingTicksToImpact;
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
            SoundDefOf.AmbientAltitudeWind.sustainFadeoutTime.Equals(30.0f);
            GenSpawn.Spawn(this.flyingThing, base.Position, base.Map);
            Pawn p = this.flyingThing as Pawn;
            if (p.IsColonist)
            {                    
                p.drafter.Drafted = this.drafted;
            }
            this.Destroy(DestroyMode.Vanish);
        }
    }
}
