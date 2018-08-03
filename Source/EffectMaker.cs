using System;
using UnityEngine;
using Verse;

namespace Wizardry
{
    public static class EffectMaker
    {
        public static void MakeEffect(ThingDef mote, Vector3 loc, Map map, float scale)
        {
            MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(mote, null);
            MakeEffect(mote, loc, map, scale, Rand.Range(0, 360), Rand.Range(.5f, 1f), Rand.Range(50, 100), moteThrown.def.mote.solidTime, moteThrown.def.mote.fadeInTime, moteThrown.def.mote.fadeOutTime, false);
        }

        public static void MakeEffect(ThingDef mote, Vector3 loc, Map map, float scale, float directionAngle, float velocity, float rotationRate)
        {
            MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(mote, null);
            MakeEffect(mote, loc, map, scale, directionAngle, velocity, rotationRate, moteThrown.def.mote.solidTime, moteThrown.def.mote.fadeInTime, moteThrown.def.mote.fadeOutTime, false);
        }

        public static void MakeEffect(ThingDef mote, Vector3 loc, Map map, float scale, float directionAngle, float velocity, float rotationRate, float solidTime, float fadeIn, float fadeOut, bool colorShift)
        {
            MakeEffect(mote, loc, map, scale, directionAngle, velocity, rotationRate, 0, solidTime, fadeIn, fadeOut, colorShift);
        }

        public static void MakeEffect(ThingDef mote, Vector3 loc, Map map, float scale, float directionAngle, float velocity, float rotationRate, float lookAngle, float solidTime, float fadeIn, float fadeOut, bool colorShift)
        {
            
            MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(mote, null);
            moteThrown.Scale = 1.9f * scale;
            moteThrown.rotationRate = rotationRate;
            moteThrown.exactPosition = loc;
            moteThrown.exactRotation = lookAngle;
            moteThrown.SetVelocity(directionAngle, velocity);
            moteThrown.def.mote.solidTime = solidTime;
            moteThrown.def.mote.fadeInTime = fadeIn;
            moteThrown.def.mote.fadeOutTime = fadeOut;

            if (colorShift)
            {
                Color color = moteThrown.instanceColor;
                color = new Color(color.r, color.g, color.b, color.a * Rand.Range(0, 1f));
                moteThrown.instanceColor = color;
            }
            GenSpawn.Spawn(moteThrown, loc.ToIntVec3(), map);
        }

        public static void MakeRadialEffects(float radius, ThingDef mote, Vector3 loc, Map map, float scale, float directionAngle, float velocity, float rotationRate, float lookAngle, float solidTime, float fadeIn, float fadeOut, bool colorShift)
        {
            int num = GenRadial.NumCellsInRadius(radius);
            for (int i = 0; i < num; i++)
            {
                IntVec3 intVec = loc.ToIntVec3() + GenRadial.RadialPattern[i];
                if (intVec.IsValid && intVec.InBounds(map))
                {
                    //-1 denotes "outward" from center
                    if (directionAngle == -1)
                    {
                        directionAngle = (Quaternion.AngleAxis(90, Vector3.up) * GetVector(loc.ToIntVec3(), intVec)).ToAngleFlat();
                    }
                    if (lookAngle == -1)
                    {
                        lookAngle = (Quaternion.AngleAxis(90, Vector3.up) * GetVector(loc.ToIntVec3(), intVec)).ToAngleFlat();
                    }
                    EffectMaker.MakeEffect(mote, intVec.ToVector3Shifted(), map, scale, directionAngle, velocity, rotationRate, lookAngle, solidTime, fadeIn, fadeOut, false);
                }
            }
        }

        public static void MakeRadialEffects(float radius, ThingDef mote, Vector3 loc, Map map, float scale, float directionAngle, float velocity, float rotationRate, float lookAngle)
        {
            MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(mote, null);
            MakeRadialEffects(radius, mote, loc, map, scale, directionAngle, velocity, rotationRate, lookAngle, moteThrown.def.mote.solidTime, moteThrown.def.mote.fadeInTime, moteThrown.def.mote.fadeOutTime, false);
        }

        private static Vector3 GetVector(IntVec3 center, IntVec3 objectPos)
        {
            Vector3 heading = (objectPos - center).ToVector3();
            float distance = heading.magnitude;
            Vector3 direction = heading / distance;
            return direction;
        }
    }
}
