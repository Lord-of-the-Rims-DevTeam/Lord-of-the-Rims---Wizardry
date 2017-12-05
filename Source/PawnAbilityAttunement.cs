using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using AbilityUser;

namespace Wizardry
{
    public class PawnAbilityAttunement : PawnAbility
    {
        public override bool ShouldShowGizmo()
        {
            return true;
        }
    }
}
