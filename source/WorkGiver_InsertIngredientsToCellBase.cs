using RimWorld;
using System.Collections.Generic;
using Verse.AI;
using Verse;

namespace ReimaginedFoundation
{
    public abstract class WorkGiver_InsertIngredientsToCellBase<T> : WorkGiver_Scanner 
        where T : ThingComp
    {
        protected abstract ThingDef TargetThingDef { get; }
        protected abstract JobDef TargetJobDef { get; }
        protected abstract Job FallbackJob { get; }

        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(TargetThingDef);

        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public override bool HasJobOnThing(Pawn pawn, Thing jobGiverSource, bool forced = false)
        {
            Log.Warning("WorkGiver_InsertIngredientsToCellBase.HasJobOnThing");

            // Check if jobGiverSource implements IIngredientsHolder interface and can receive ingredients
            if (!TryGetIngredientsRequester(jobGiverSource, out IThingRequester ingredientsHolder) || !ingredientsHolder.CanReceiveThings)
            {
                return false;
            }

            // Check if jobGiverSource actually has requested ingredients
            if (ingredientsHolder.ExpectedThings == null || ingredientsHolder.ExpectedThings.Count == 0)
            {
                return false;
            }

            // Check if jobGiverSource forbiden or is it not reservable for a pawn
            if (jobGiverSource.IsForbidden(pawn) || !pawn.CanReserve(jobGiverSource, 1, 1, null, forced))
            {
                return false;
            }

            // Check if jobGiverSource not reachable or is on fire
            if (pawn.Map.designationManager.DesignationOn(jobGiverSource, DesignationDefOf.Deconstruct) != null || jobGiverSource.IsBurning())
            {
                return false;
            }

            // Check if all ingredients can be reserved by pawn (cant satisfy request fully)
            foreach (ThingCount ingredient in ingredientsHolder.GetRequestedThings())
            {
                if (!pawn.CanReserve(ingredient.Thing))
                {
                    return false;
                }
            }

            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing jobGiverSource, bool forced = false)
        {
            Log.Warning("WorkGiver_InsertIngredientsToCellBase.JobOnThing");

            // Try to get the ingredients requester
            if (!TryGetIngredientsRequester(jobGiverSource, out IThingRequester ingredientsHolder) || ingredientsHolder.ExpectedThings.Count == 0)
            {
                Log.Warning("No ingredients requester or no requested things found.");
                return FallbackJob;
            }

            // Fetch the requested ingredients
            ThingCountClass requestedThing = ingredientsHolder.TryRequestedThing();
            if (requestedThing == null) 
            {
                Log.Warning("No available things to fulfill the request.");
                return FallbackJob;
            }

            // Create and configure the job
            Job job = JobMaker.MakeJob(TargetJobDef, jobGiverSource);
            job.count = requestedThing.Count;
            job.targetQueueB = new List<LocalTargetInfo> { new LocalTargetInfo(requestedThing.thing) };
            job.haulMode = HaulMode.ToCellNonStorage;

            return job;
        }

        private bool TryGetIngredientsRequester(Thing jobGiverSource, out IThingRequester holder)
        {
            holder = jobGiverSource.TryGetComp<T>() as IThingRequester;
            return holder != null;
        }
    }
}
