using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using AbilityUser;

namespace Wizardry
{
    [DefOf]
    public class WizardryDefOf
    {
        //AbilityDefs
        public static WizardAbilityDef LotRW_Varda_FocusFlames;
        public static WizardAbilityDef LotRW_Varda_ConeOfFire;
        public static WizardAbilityDef LotRW_Varda_RainOfFire;

        public static WizardAbilityDef LotRW_Ulmo_RainDance;
        public static WizardAbilityDef LotRW_Ulmo_WolfSong;
        public static WizardAbilityDef LotRW_Ulmo_FlameSong;

        public static WizardAbilityDef LotRW_LightChaser;
        public static WizardAbilityDef LotRW_StormCalling;

        public static WizardAbilityDef LotRW_Manwe_WindControl;
        public static WizardAbilityDef LotRW_Manwe_Vortex;
        public static WizardAbilityDef LotRW_Manwe_AirWall;

        public static WizardAbilityDef LotRW_Nienna_HealingRain;
        public static WizardAbilityDef LotRW_Nienna_HealingTouch;

        public static WizardAbilityDef LotRW_Aule_RendEarth;
        public static WizardAbilityDef LotRW_Aule_RockWall;

        public static WizardAbilityDef LotRW_Mandos_Haunt;
        public static WizardAbilityDef LotRW_Mandos_Doom;
        public static WizardAbilityDef LotRW_Mandos_Darkness;

        //Effects
        public static ThingDef Mote_ExpandingFlame;
        public static ThingDef Mote_RecedingFlame;
        public static ThingDef Mote_Sparks;
        public static ThingDef Mote_WolfSong_North;
        public static ThingDef Mote_WolfSong_South;
        public static ThingDef Mote_WolfSong_East;
        public static ThingDef Mote_WolfSong_West;
        public static ThingDef FlyingObject_WolfSong;
        public static ThingDef FlyingObject_Spinning;
        public static ThingDef FlyingObject_StreamingFlame;
        public static ThingDef FlyingObject_Haunt;
        public static ThingDef Mote_CastingBeam;

        //Sounds
        public static SoundDef SoftExplosion;

        //Traits
        public static TraitDef LotRW_Istari;
        public static TraitDef LotRW_MagicAttuned;

        //Ability Jobs
        public static JobDef JobDriver_StormCalling;
        public static JobDef JobDriver_HealingTouch;

        //Mental States
        public static MentalStateDef LotRW_FleeLight;

        public static WizardAbilityDef CompVerb;

        //Damages
        public static DamageDef LotRW_RockFragments;
        public static DamageDef LotRW_HauntDD;

        //Weather
        public static WeatherDef LotRW_HealingRainWD;
    }
}
