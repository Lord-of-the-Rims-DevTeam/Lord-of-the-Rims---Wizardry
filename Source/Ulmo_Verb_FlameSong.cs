using System;
using AbilityUser;
using Verse;
using RimWorld;
using Verse.AI;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Wizardry
{
    public class Ulmo_Verb_FlameSong : Verb_UseAbility
    {

        float fireAmount = 0;

        protected override bool TryCastShot()
        {
            Map map = base.CasterPawn.Map;
            Pawn pawn = base.CasterPawn;
            IntVec3 centerCell = this.currentTarget.Cell;
            this.fireAmount = CalculateFireAmountInArea(centerCell, this.Projectile.projectile.explosionRadius, map);
            if ((centerCell.IsValid && centerCell.InBounds(map)))
            {
                Thing thing = null;
                if (this.fireAmount > 8f)
                {
                    thing = ThingMaker.MakeThing(ThingDef.Named("LotRW_Flamesong_Orb"), null);
                }
                else if (this.fireAmount > 1f)
                {
                    thing = ThingMaker.MakeThing(ThingDef.Named("LotRW_Flamesong_Orb_Small"), null);
                }                
                
                if (thing != null)
                {
                    GenPlace.TryPlaceThing(thing, centerCell, map, ThingPlaceMode.Near, null);
                }
            }
            else
            {
                Messages.Message("failed to spawn orb of flamesong", MessageTypeDefOf.RejectInput);
            }
            this.Ability.PostAbilityAttempt();
            this.burstShotsLeft = 0;
            return false;
        }

        public float CalculateFireAmountInArea(IntVec3 center, float radius, Map map)
        {
            float result = 0;
            IntVec3 curCell;
            List<Thing> fireList = map.listerThings.ThingsOfDef(ThingDefOf.Fire);
            IEnumerable<IntVec3> targetCells = GenRadial.RadialCellsAround(center, radius, true);
            for (int i = 0; i < targetCells.Count(); i++)
            {
                curCell = targetCells.ToArray<IntVec3>()[i];
                if (curCell.InBounds(map) && curCell.IsValid)
                {
                    for (int j = 0; j < fireList.Count; j++)
                    {
                        if (fireList[j].Position == curCell)
                        {
                            Fire fire = fireList[j] as Fire;
                            result += fire.fireSize;
                            RemoveFireAtPosition(curCell, map);
                        }
                    }
                }
            }
            return result;
        }

        public void RemoveFireAtPosition(IntVec3 pos, Map map)
        {
            GenExplosion.DoExplosion(pos, map, 1, DamageDefOf.Extinguish, this.CasterPawn, 100, 0, SoundDef.Named("ExpandingFlames"), null, null, null, null, 0f, 1, false, null, 0f, 1, 0f, false);
        }
    }
}
