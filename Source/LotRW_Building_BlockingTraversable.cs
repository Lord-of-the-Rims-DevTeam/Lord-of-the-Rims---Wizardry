using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;
using RimWorld;

namespace Wizardry
{
    public class LotRW_Building_BlockingTraversable : Building
    {
        public override void Tick()
        {
            base.Tick();
            if (this.Map != null)
            {                
                if (Find.TickManager.TicksGame % 2 == 0)
                {
                    DestroyProjectiles();
                    if (Find.TickManager.TicksGame % 6 == 0)
                    {
                        SlowNearbyPawns();
                    }
                }
            }
        }

        public void DestroyProjectiles()
        {
            List<Thing> cellList = this.Position.GetThingList(this.Map);
            for (int i = 0; i < cellList.Count; i++)
            {
                if (cellList[i] is Projectile && cellList[i].def.defName != "LotRW_Projectile_AirWall")
                {
                    Vector3 displayEffect = this.DrawPos;
                    displayEffect.x += Rand.Range(-.3f, .3f);
                    displayEffect.y += Rand.Range(-.3f, .3f);
                    displayEffect.z += Rand.Range(-.3f, .3f);
                    EffectMaker.MakeEffect(ThingDef.Named("Mote_LightningGlow"), displayEffect, this.Map, cellList[i].def.projectile.GetDamageAmount(1, null)/8f);
                    cellList[i].Destroy(DestroyMode.Vanish);
                }
            }
        }

        public void SlowNearbyPawns()
        {
            int num = GenRadial.NumCellsInRadius(1);
            Pawn p = null;
            for (int i = 0; i < num; i++)
            {
                IntVec3 intVec = this.Position + GenRadial.RadialPattern[i];
                if (intVec.IsValid && intVec.InBounds(base.Map))
                {
                    p = intVec.GetFirstPawn(this.Map);
                    if(p != null)
                    {
                        HealthUtility.AdjustSeverity(p, HediffDef.Named("LotRW_SlowHD"), .5f);
                    }
                }
            }
        }
    }
}
