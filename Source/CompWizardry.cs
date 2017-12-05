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
        public override bool TryTransformPawn() => this.Pawn.IsIstari() || this.Pawn.IsWizard(); 
    }
}