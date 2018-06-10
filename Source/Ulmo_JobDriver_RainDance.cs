using System.Collections.Generic;
using Verse.AI;
using RimWorld;
using Verse;
using AbilityUser;
using System.Linq;

namespace Wizardry
{
    public class Ulmo_JobDriver_RainDance : JobDriver
    {
        public override bool TryMakePreToilReservations()
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            IntVec3 nextCell = TargetLocA;
            Toil gotoSpot = new Toil()
            {
                initAction = () =>
                {
                    pawn.pather.StartPath(TargetLocA, PathEndMode.Touch);
                },
                defaultCompleteMode = ToilCompleteMode.PatherArrival
            };
            yield return gotoSpot;
            yield return Toils_General.Wait(30);
            yield return Toils_Dance.Dance();
            nextCell.x += -2;
            yield return Toils_Goto.GotoCell(nextCell, PathEndMode.OnCell);
            nextCell.z += -1;
            Toil turn1 = new Toil()
            {
                initAction = () =>
                {
                    pawn.pather.StartPath(nextCell, PathEndMode.Touch);
                },
                defaultCompleteMode = ToilCompleteMode.PatherArrival
            };
            yield return turn1;
            nextCell.z += 1;
            nextCell.x += -1;
            Toil turn2 = new Toil()
            {
                initAction = () =>
                {
                    pawn.pather.StartPath(nextCell, PathEndMode.Touch);
                },
                defaultCompleteMode = ToilCompleteMode.PatherArrival
            };
            yield return turn2;
            nextCell.z += 1;
            nextCell.x += 1;
            Toil turn3 = new Toil()
            {
                initAction = () =>
                {
                    pawn.pather.StartPath(nextCell, PathEndMode.Touch);
                },
                defaultCompleteMode = ToilCompleteMode.PatherArrival
            };
            yield return turn3;
            yield return Toils_General.Wait(10);
            yield return Toils_Dance.Dance();
            //
            nextCell.x += 1;
            nextCell.z += 2;
            yield return Toils_Goto.GotoCell(nextCell, PathEndMode.OnCell);
            yield return Toils_General.Wait(10);
            yield return Toils_Dance.Dance();
            nextCell.z += -1;
            Toil turn11 = new Toil()
            {
                initAction = () =>
                {
                    pawn.pather.StartPath(nextCell, PathEndMode.Touch);
                },
                defaultCompleteMode = ToilCompleteMode.PatherArrival
            };
            yield return turn11;
            nextCell.z += 1;
            nextCell.x += -1;
            Toil turn12 = new Toil()
            {
                initAction = () =>
                {
                    pawn.pather.StartPath(nextCell, PathEndMode.Touch);
                },
                defaultCompleteMode = ToilCompleteMode.PatherArrival
            };
            yield return turn12;
            nextCell.z += 1;
            nextCell.x += 1;
            Toil turn13 = new Toil()
            {
                initAction = () =>
                {
                    pawn.pather.StartPath(nextCell, PathEndMode.Touch);
                },
                defaultCompleteMode = ToilCompleteMode.PatherArrival
            };
            yield return turn13;
            yield return Toils_General.Wait(10);
            yield return Toils_Dance.Dance();
            //this.FailOnCannotTouch(TargetIndex.A, PathEndMode.OnCell);
            //this.FailOnDowned(TargetIndex.A);
            //this.FailOnMentalState(TargetIndex.A);
            //yield return Toils_Dance.Dance();

            yield return Toils_Goto.GotoCell(nextCell, PathEndMode.OnCell);
            yield return Toils_Dance.Dance();
            nextCell.x += 2;
            nextCell.z += -2;
            yield return Toils_Goto.GotoCell(nextCell, PathEndMode.OnCell);
            yield return Toils_Dance.Dance();
            nextCell.x += -2;
            nextCell.z += -2;
            yield return Toils_Goto.GotoCell(nextCell, PathEndMode.OnCell);
            yield return Toils_Dance.Dance();
            nextCell.z += 2;
            yield return Toils_Goto.GotoCell(nextCell, PathEndMode.OnCell);
            yield return Toils_Dance.Dance();

        }
    }
}
