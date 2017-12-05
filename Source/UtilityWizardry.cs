using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Wizardry
{
    public static class UtilityWizardry
    {
        public static bool IsIstari(this Pawn pawn)
        {
            if (pawn?.story?.traits?.HasTrait(TraitDef.Named("LOTR_Istari")) ?? false)
                return true;
            return false;
        }

        public static bool IsWizard(this Pawn pawn)
        {
            if (pawn?.story?.traits?.HasTrait(TraitDef.Named("LOTR_MagicAttunement")) ?? false)
                return true;
            return false;
        }

    }
}
