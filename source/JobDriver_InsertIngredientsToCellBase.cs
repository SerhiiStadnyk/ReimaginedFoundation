using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace ReimaginedFoundation
{
    public abstract class JobDriver_InsertIngredientsToCellBase<T> : JobDriver
        where T : ThingComp
    {
        protected virtual int InsertDelay => 0;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Log.Warning("JobDriver_InsertIngredientsToCellBase.TryMakePreToilReservations");

            Thing thing = job.GetTarget(TargetIndex.A).Thing;
            if (!pawn.Reserve(job.GetTarget(TargetIndex.A), job, 1, -1, null, errorOnFailed))
            {
                return false;
            }
            pawn.ReserveAsManyAsPossible(job.GetTargetQueue(TargetIndex.B), job);
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Log.Warning("JobDriver_InsertIngredientsToCellBase.MakeNewToils");

            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.A);
            foreach (LocalTargetInfo target in job.GetTargetQueue(TargetIndex.B))
            {
                yield return Toils_General.DoAtomic(delegate
                {
                    job.SetTarget(TargetIndex.B, target);
                });
                yield return Toils_Reserve.Reserve(TargetIndex.B, 1, 1).FailOnDespawnedNullOrForbidden(TargetIndex.B);
                yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
                yield return Toils_Haul.StartCarryThing(TargetIndex.B, putRemainderInQueue: false, subtractNumTakenFromJobCount: true).FailOnDestroyedNullOrForbidden(TargetIndex.B);
                yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

                if (InsertDelay > 0) 
                {
                    yield return Toils_General.Wait(InsertDelay).FailOnDestroyedNullOrForbidden(TargetIndex.B).FailOnDestroyedNullOrForbidden(TargetIndex.A)
                        .FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch)
                        .WithProgressBarToilDelay(TargetIndex.A);
                }
                yield return Toils_General.DoAtomic(delegate
                {
                    Pawn actor = GetActor();
                    actor.carryTracker.TryDropCarriedThing(actor.Position, ThingPlaceMode.Near, out var _, delegate (Thing thing, int i)
                    {
                        T comp = job.GetTarget(TargetIndex.A).Thing.TryGetComp<T>();
                        IThingRequester ingredientsHolder = comp as IThingRequester;

                        ingredientsHolder.OnThingHauled(thing);
                    });
                });
            }

            yield return Toils_General.DoAtomic(delegate
            {
                //ingredientsHolder.OnIngredientHauled();
            });
        }
    }
}
