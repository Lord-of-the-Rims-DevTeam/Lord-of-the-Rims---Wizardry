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
            if (pawn?.story?.traits?.HasTrait(TraitDef.Named("LotRW_Istari")) ?? false)
                return true;
            return false;
        }

        public static bool IsMage(this Pawn pawn)
        {
            if (pawn?.story?.traits?.HasTrait(TraitDef.Named("LotRW_MagicAttuned")) ?? false)
                return true;
            return false;
        }

    }
}
