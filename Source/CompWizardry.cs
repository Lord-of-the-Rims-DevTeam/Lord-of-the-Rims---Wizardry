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

        public bool IsWizard
        {
            get
            {
                return UtilityWizardry.IsIstari(this.Pawn) || UtilityWizardry.IsMage(this.Pawn);
            }
        }

        private void TempResolvePowers()
        {
            if (this.Pawn.IsIstari() && doOnce)
            {
                this.RemovePawnAbility(WizardryDefOf.LotRW_Varda_FocusFlames);
                this.RemovePawnAbility(WizardryDefOf.LotRW_Varda_ConeOfFire);
                this.RemovePawnAbility(WizardryDefOf.LotRW_Varda_RainOfFire);
                this.RemovePawnAbility(WizardryDefOf.LotRW_Ulmo_RainDance);
                this.RemovePawnAbility(WizardryDefOf.LotRW_Ulmo_WolfSong);
                this.RemovePawnAbility(WizardryDefOf.LotRW_Ulmo_FlameSong);
                this.RemovePawnAbility(WizardryDefOf.LotRW_StormCalling);
                this.RemovePawnAbility(WizardryDefOf.LotRW_LightChaser);
                this.RemovePawnAbility(WizardryDefOf.LotRW_Manwe_WindControl);
                this.RemovePawnAbility(WizardryDefOf.LotRW_Manwe_Vortex);
                this.RemovePawnAbility(WizardryDefOf.LotRW_Manwe_AirWall);
                this.RemovePawnAbility(WizardryDefOf.LotRW_Nienna_HealingRain);
                this.RemovePawnAbility(WizardryDefOf.LotRW_Nienna_HealingTouch);
                this.RemovePawnAbility(WizardryDefOf.LotRW_Aule_RockWall);
                this.RemovePawnAbility(WizardryDefOf.LotRW_Aule_RendEarth);
                this.RemovePawnAbility(WizardryDefOf.LotRW_Mandos_Haunt);
                this.RemovePawnAbility(WizardryDefOf.LotRW_Mandos_Doom);
                this.RemovePawnAbility(WizardryDefOf.LotRW_Mandos_Darkness);

                this.AddPawnAbility(WizardryDefOf.LotRW_Varda_FocusFlames);
                this.AddPawnAbility(WizardryDefOf.LotRW_Varda_ConeOfFire);
                this.AddPawnAbility(WizardryDefOf.LotRW_Varda_RainOfFire);
                this.AddPawnAbility(WizardryDefOf.LotRW_Ulmo_RainDance);
                this.AddPawnAbility(WizardryDefOf.LotRW_Ulmo_WolfSong);
                this.AddPawnAbility(WizardryDefOf.LotRW_Ulmo_FlameSong);
                this.AddPawnAbility(WizardryDefOf.LotRW_LightChaser);
                this.AddPawnAbility(WizardryDefOf.LotRW_StormCalling);
                this.AddPawnAbility(WizardryDefOf.LotRW_Manwe_WindControl);
                this.AddPawnAbility(WizardryDefOf.LotRW_Manwe_Vortex);
                this.AddPawnAbility(WizardryDefOf.LotRW_Manwe_AirWall);
                this.AddPawnAbility(WizardryDefOf.LotRW_Nienna_HealingRain);
                this.AddPawnAbility(WizardryDefOf.LotRW_Nienna_HealingTouch);
                this.AddPawnAbility(WizardryDefOf.LotRW_Aule_RockWall);
                this.AddPawnAbility(WizardryDefOf.LotRW_Aule_RendEarth);
                this.AddPawnAbility(WizardryDefOf.LotRW_Mandos_Haunt);
                this.AddPawnAbility(WizardryDefOf.LotRW_Mandos_Doom);
                this.AddPawnAbility(WizardryDefOf.LotRW_Mandos_Darkness);
                this.doOnce = false;
            }
        }
    }
}