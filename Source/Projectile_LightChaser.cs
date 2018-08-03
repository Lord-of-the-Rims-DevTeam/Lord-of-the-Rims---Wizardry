using Verse;
using Verse.Sound;
using RimWorld;
using AbilityUser;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Verse.AI;

namespace Wizardry
{
    [StaticConstructorOnStartup]
    public class Projectile_LightChaser : Projectile_AbilityBase
    {
        private bool initialized = false;
        private int beamNum;
        private int age = -1;
        List<float> beamMaxSize = new List<float>();
        List<float> beamSize = new List<float>();
        List<int> beamDuration = new List<int>();
        List<int> beamAge = new List<int>();
        List<int> beamStartTick = new List<int>();
        List<Vector3> beamPos = new List<Vector3>();
        List<Vector3> beamVector = new List<Vector3>();
        IntVec3 anglePos;
        Pawn caster;

        ColorInt colorInt = new ColorInt(255, 255, 140);

        private float angle = 0;

        private static readonly Material BeamMat = MaterialPool.MatFrom("Other/OrbitalBeam", ShaderDatabase.MoteGlow, MapMaterialRenderQueues.OrbitalBeam);
        private static readonly Material BeamEndMat = MaterialPool.MatFrom("Other/OrbitalBeamEnd", ShaderDatabase.MoteGlow, MapMaterialRenderQueues.OrbitalBeam);
        private static readonly Material BombardMat = MaterialPool.MatFrom("Effects/Bombardment", ShaderDatabase.MoteGlow, MapMaterialRenderQueues.OrbitalBeam);
        private static readonly MaterialPropertyBlock MatPropertyBlock = new MaterialPropertyBlock();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.initialized, "initialized", false, false);
            Scribe_Values.Look<int>(ref this.age, "age", -1, false);
            Scribe_Values.Look<IntVec3>(ref this.anglePos, "anglePos", default(IntVec3), false);
            Scribe_Collections.Look<float>(ref this.beamMaxSize, "beamMaxSize", LookMode.Value);
            Scribe_Collections.Look<Vector3>(ref this.beamVector, "beamVector", LookMode.Value);
            Scribe_Collections.Look<int>(ref this.beamDuration, "beamDuration", LookMode.Value);
            Scribe_Collections.Look<int>(ref this.beamAge, "beamAge", LookMode.Value);
            Scribe_Collections.Look<Vector3>(ref this.beamPos, "beamPos", LookMode.Value);
        }

        protected override void Impact(Thing hitThing)
        {
            Map map = base.Map;
            base.Impact(hitThing);
            ThingDef def = this.def;
            if (!this.initialized)
            {
                caster = this.launcher as Pawn;
                anglePos = caster.Position;
                IntVec3 targetCell = base.Position;
                this.GenerateBeams(targetCell, caster.Position, map);
                this.initialized = true;
            }

            AdjustBeams();
            if (Find.TickManager.TicksGame % 30 == 0)
            {
                DoEffectsNearPosition();
            }
            RemoveExpiredBeams();
        }    
        
        public void DoEffectsNearPosition()
        {
            Map map = base.Map;
            for (int i = 0; i < this.beamNum; i++)
            {
                if (this.beamAge[i] > this.beamStartTick[i])
                {
                    Pawn victim = null;
                    IntVec3 curCell;
                    IEnumerable<IntVec3> cellList = GenRadial.RadialCellsAround(this.beamPos[i].ToIntVec3(), this.beamSize[i] + 3, true);
                    for (int j = 0; j < cellList.Count(); j++)
                    {
                        curCell = cellList.ToArray<IntVec3>()[j];
                        if (curCell.IsValid && curCell.InBounds(map))
                        {
                            victim = curCell.GetFirstPawn(map);
                            if (victim != null && !victim.Downed && !victim.Dead && victim.Faction != this.caster.Faction)
                            {
                                LocalTargetInfo t = new LocalTargetInfo(victim.Position + (6 * GetVector(this.beamPos[i], victim.DrawPos)).ToIntVec3());
                                Job job = new Job(JobDefOf.Goto, t);
                                victim.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                            }
                            else
                            {
                                victim = null;
                            }
                        }
                    }
                    cellList = GenRadial.RadialCellsAround(this.beamPos[i].ToIntVec3(), this.beamSize[i], true);
                    for (int j = 0; j < cellList.Count(); j++)
                    {
                        curCell = cellList.ToArray<IntVec3>()[j];
                        if (curCell.IsValid && curCell.InBounds(map))
                        {
                            victim = curCell.GetFirstPawn(map);
                            if (victim != null && !victim.Downed && !victim.Dead && victim.Faction != this.caster.Faction)
                            {
                                float distanceFromCenter = Mathf.Min((this.beamPos[i].ToIntVec3() - victim.Position).LengthHorizontal, 1);
                                DamageEntities(victim, 2 / distanceFromCenter);
                                HealthUtility.AdjustSeverity(victim, HediffDef.Named("LotRW_Blindness"), 1f / distanceFromCenter);
                            }
                            else
                            {
                                victim = null;
                            }
                        }
                    }
                }
            }
        }

        public Vector3 GetVector(Vector3 casterPos, Vector3 targetPos)
        {
            CellRect cellRect = CellRect.CenteredOn(targetPos.ToIntVec3(), 5);
            targetPos = cellRect.RandomVector3;
            Vector3 heading = (targetPos - casterPos);
            float distance = heading.magnitude;
            Vector3 direction = heading / distance;
            return direction;
        }

        public void GenerateBeams(IntVec3 center, IntVec3 casterPos, Map map)
        {
            CellRect cellRect = CellRect.CenteredOn(center, 2);
            this.beamNum = Rand.Range(3, 5);
            cellRect.ClipInsideMap(map);
            for (int i = 0; i < this.beamNum; i++)
            {
                this.beamPos.Add(cellRect.RandomCell.ToVector3());                
                this.beamMaxSize.Add(Rand.Range(2f, 6f));
                this.beamSize.Add(this.beamMaxSize[i] / 10f);
                this.beamAge.Add(0);
                this.beamDuration.Add(Rand.Range(480, 600)); 
                this.beamStartTick.Add(Rand.Range(0, 120));
                //Random range here determines how much variation occurs while beam travels
                //Since this calculation is based on caster position relative to target position, beam behavior will be different if the target position is close 
                //and variation is large (ie beams traveling in all directions)
                //this could also be adjusted with additive values to produce larger xy variations
                this.beamVector.Add(GetVector(casterPos.ToVector3(), center.ToVector3()) * Rand.Range(.04f, .06f));
            }
        }

        public void AdjustBeams()
        {
            for (int i = 0; i < this.beamNum; i++)
            {
                if (this.beamAge[i] > this.beamStartTick[i])
                {
                    this.beamPos[i] += this.beamVector[i];
                    if (this.beamAge[i] > this.beamDuration[i] * .8f)
                    {
                        //gracefully end beam
                        this.beamSize[i] -= (this.beamMaxSize[i] / (this.beamDuration[i] * .2f));
                    }
                    else if (this.beamSize[i] < this.beamMaxSize[i])
                    {
                        //expand beam until it reaches max size or until it needs to start shrinking
                        this.beamSize[i] += (this.beamMaxSize[i] / (this.beamDuration[i] * .3f));
                    }
                }
                this.beamAge[i]++;
            }
        }

        public void RemoveExpiredBeams()
        {
            //remove beam from list
            for (int i = 0; i < this.beamNum; i++)
            {
                if(this.beamAge[i] > this.beamDuration[i])
                {
                    this.beamAge.Remove(this.beamAge[i]);
                    this.beamDuration.Remove(this.beamDuration[i]);
                    this.beamPos.Remove(this.beamPos[i]);
                    this.beamMaxSize.Remove(this.beamMaxSize[i]);
                    this.beamVector.Remove(this.beamVector[i]);
                    this.beamSize.Remove(this.beamSize[i]);
                    this.beamStartTick.Remove(this.beamStartTick[i]);
                    this.beamNum--;
                }
            }
        }

        public override void Draw()
        {
            base.Draw();
            for (int i = 0; i < beamNum; i++)
            {
                if (this.beamAge[i] >= this.beamStartTick[i] && this.beamAge[i] < this.beamDuration[i])
                {
                    DrawBeam(this.beamPos[i], this.beamSize[i]);
                }
            }
        }

        public void DrawBeam(Vector3 drawPos, float size)
        {
            float num = ((float)base.Map.Size.z - drawPos.z) * 1.4f;        //distance towards top of map from the target position
            this.angle = ((anglePos.x - drawPos.x) * .3f);                  //beams originate from above caster (original position) head, and the angle moves, not the entire beam
            Vector3 a = Vector3Utility.FromAngleFlat(this.angle - 90f);     //angle of beam adjusted for quaternian
            //matrix4x4 will draw the stretched beam (matrix) with center at drawPos, so it must be adjusted so that the end of the beam appears at drawPos
            //so we create a new vector and add half the length of the beam to the original position so that the end of the beam appears at drawPos
            Vector3 a2 = drawPos + a * num * 0.5f;                          //original position adjusted by half the beam length
            a2.y = Altitudes.AltitudeFor(AltitudeLayer.MetaOverlays);       //mote depth (should be drawn in front of everything else)

            //Color arg_50_0 = colorInt.ToColor;
            //Color color = arg_50_0;
            //color.a *= num3;
            //Projectile_LightChaser.MatPropertyBlock.SetColor(ShaderPropertyIDs.Color, color);

            if (size > 0) //failsafe to prevent graphic anomolies not already handled
            {
                //Draw the beam
                Matrix4x4 matrix = default(Matrix4x4);
                //Create the graphics based on translation, rotation, and scaling
                //Beam is drawn where the bottom end of the beam looks like it's at the target position
                matrix.SetTRS(a2, Quaternion.Euler(0f, this.angle, 0f), new Vector3(size, 1f, num));
                Graphics.DrawMesh(MeshPool.plane10, matrix, Projectile_LightChaser.BeamMat, 0, null, 0, Projectile_LightChaser.MatPropertyBlock);

                //The beam is a rectangle and we want to soften the end, so add a beam end mote and adjust for size and angle offsets
                Vector3 vectorPos = drawPos - (a * size *.5f);
                vectorPos.y = Altitudes.AltitudeFor(AltitudeLayer.MetaOverlays);
                Matrix4x4 matrix2 = default(Matrix4x4);
                matrix2.SetTRS(vectorPos, Quaternion.Euler(0f, this.angle, 0f), new Vector3(size, 1f, size));
                Graphics.DrawMesh(MeshPool.plane10, matrix2, Projectile_LightChaser.BeamEndMat, 0, null, 0, Projectile_LightChaser.MatPropertyBlock);

                //Additional softening of the end point and add more intensity by adding light "splash" at the end of the beam, so add another, circular mesh at the end            
                Matrix4x4 matrix3 = default(Matrix4x4);
                matrix3.SetTRS(drawPos, Quaternion.Euler(0f, this.angle, 0f), new Vector3(10f * size, 1f, 10f * size));
                Graphics.DrawMesh(MeshPool.plane10, matrix3, Projectile_LightChaser.BombardMat, 0, null, 0, Projectile_LightChaser.MatPropertyBlock);
            }
        }

        public void DamageEntities(Thing e, float amt)
        {
            amt = Rand.Range(amt * .75f, amt * 1.25f);
            DamageInfo dinfo = new DamageInfo(DamageDefOf.Flame, Mathf.RoundToInt(amt), 2, (float)-1, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown);
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
            if(this.age >= 120 && this.beamNum == 0)
            {
                this.age = 721;
                Destroy(DestroyMode.Vanish);
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            bool flag = this.age <= 600;
            if (!flag)
            {
                base.Destroy(mode);
            }
        }
    }
}
