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

        public static void MakeEffect(ThingDef mote, Vector3 loc, Map map, float scale, float directionAngle, float velocity, float rotationRate, float solidTime, float fadeIn, float fadeOut, bool priority)
        {
            //if (!loc.ShouldSpawnMotesAt(map) || (map.moteCounter.Saturated && !priority))
            //{
            //    return;
            //}
            MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(mote, null);
            moteThrown.Scale = 1.9f * scale;
            moteThrown.rotationRate = rotationRate;
            moteThrown.exactPosition = loc;
            moteThrown.SetVelocity(directionAngle, velocity);
            moteThrown.def.mote.solidTime = solidTime;
            moteThrown.def.mote.fadeInTime = fadeIn;
            moteThrown.def.mote.fadeOutTime = fadeOut;
            GenSpawn.Spawn(moteThrown, loc.ToIntVec3(), map);
        }
    }
}
