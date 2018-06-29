using AbilityUser;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

namespace Wizardry
{
    /// CompWizardry

    public class CompWizardry : CompAbilityUser
    {
        private bool doOnce = true;
        public LocalTargetInfo SecondTarget = null;
        public override bool TryTransformPawn() => this.Pawn.IsIstari() || this.Pawn.IsMage();

        public override void CompTick()
        {
            base.CompTick();
            if (Find.TickManager.TicksGame % 30 == 0 && this.doOnce)
            {
                TempResolvePowers();
            }
        }

        private void TempResolvePowers()
        {
            if (this.Pawn.IsIstari() && doOnce)
            {
                this.RemovePawnAbility(WizardryDefOf.LOTR_Varda_FocusFlames);
                this.RemovePawnAbility(WizardryDefOf.LOTR_Varda_ConeOfFire);
                this.RemovePawnAbility(WizardryDefOf.LOTR_Varda_RainOfFire);
                this.RemovePawnAbility(WizardryDefOf.LotRW_Ulmo_RainDance);
                this.RemovePawnAbility(WizardryDefOf.LotRW_Ulmo_WolfSong);
                this.RemovePawnAbility(WizardryDefOf.LotRW_Ulmo_FlameSong);
                this.RemovePawnAbility(WizardryDefOf.LotRW_StormCalling);
                this.RemovePawnAbility(WizardryDefOf.LotRW_LightChaser);
                this.RemovePawnAbility(WizardryDefOf.LotRW_Manwe_WindControl);
                this.RemovePawnAbility(WizardryDefOf.LotRW_Manwe_Vortex);
                this.RemovePawnAbility(WizardryDefOf.LotRW_Manwe_AirWall);

                this.AddPawnAbility(WizardryDefOf.LOTR_Varda_FocusFlames);
                this.AddPawnAbility(WizardryDefOf.LOTR_Varda_ConeOfFire);
                this.AddPawnAbility(WizardryDefOf.LOTR_Varda_RainOfFire);
                this.AddPawnAbility(WizardryDefOf.LotRW_Ulmo_RainDance);
                this.AddPawnAbility(WizardryDefOf.LotRW_Ulmo_WolfSong);
                this.AddPawnAbility(WizardryDefOf.LotRW_Ulmo_FlameSong);
                this.AddPawnAbility(WizardryDefOf.LotRW_LightChaser);
                this.AddPawnAbility(WizardryDefOf.LotRW_StormCalling);
                this.AddPawnAbility(WizardryDefOf.LotRW_Manwe_WindControl);
                this.AddPawnAbility(WizardryDefOf.LotRW_Manwe_Vortex);
                this.AddPawnAbility(WizardryDefOf.LotRW_Manwe_AirWall);
                this.doOnce = false;
            }
        }
    }
}