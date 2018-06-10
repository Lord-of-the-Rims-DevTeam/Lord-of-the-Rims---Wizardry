using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Wizardry
{
    [DefOf]
    public class WizardryDefOf
    {
        //AbilityDefs
        public static WizardAbilityDef LOTR_Varda_FocusFlames;
        public static WizardAbilityDef LOTR_Varda_ConeOfFire;
        public static WizardAbilityDef LOTR_Varda_RainOfFire;

        public static WizardAbilityDef LotRW_Ulmo_RainDance;
        public static WizardAbilityDef LotRW_Ulmo_WolfSong;
        public static WizardAbilityDef LotRW_Ulmo_FlameSong;

        //Effects
        public static ThingDef Mote_ExpandingFlame;
        public static ThingDef Mote_RecedingFlame;
        public static ThingDef Mote_Sparks;
        public static ThingDef Mote_WolfSong_North;
        public static ThingDef Mote_WolfSong_South;
        public static ThingDef Mote_WolfSong_East;
        public static ThingDef Mote_WolfSong_West;
        public static ThingDef FlyingObject_WolfSong;

        public static TraitDef LotRW_Istari;
        public static TraitDef LotRW_MagicAttuned;

        //Ability Jobs
        public static JobDef Ulmo_JobDriver_RainDance;
    }
}
