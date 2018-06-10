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
        public override bool TryTransformPawn() => this.Pawn.IsIstari() || this.Pawn.IsMage();

        public override void CompTick()
        {
            base.CompTick();
            if (Find.TickManager.TicksGame % 30 == 0)
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

                this.AddPawnAbility(WizardryDefOf.LOTR_Varda_FocusFlames);
                this.AddPawnAbility(WizardryDefOf.LOTR_Varda_ConeOfFire);
                this.AddPawnAbility(WizardryDefOf.LOTR_Varda_RainOfFire);
                this.AddPawnAbility(WizardryDefOf.LotRW_Ulmo_RainDance);
                this.AddPawnAbility(WizardryDefOf.LotRW_Ulmo_WolfSong);
                this.AddPawnAbility(WizardryDefOf.LotRW_Ulmo_FlameSong);
                this.doOnce = false;
            }
        }
    }
}