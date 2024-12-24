using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace ReimaginedFoundation
{
    public abstract class ThingCompVirtualThingHolder : ThingComp, IThingRequester
    {
        protected Dictionary<ThingDef, int> _expectedThings = new Dictionary<ThingDef, int>();
        protected List<Thing> _insertedThings = new List<Thing>();

        public virtual Dictionary<ThingDef, int> ExpectedThings => _expectedThings;

        public abstract bool CanReceiveThings {  get; }

        public virtual ThingCountClass TryRequestedThing() 
        {
            foreach (var pair in _expectedThings)
            {
                // Find the specific thing based on its definition
                Thing thingToRequest = FindThing(pair.Key);

                if (thingToRequest != null)
                {
                    int requiredCount = pair.Value; // How many are needed
                    int insertedCount = GetInsertedThingCount(pair.Key);

                    if (insertedCount < requiredCount)
                    {
                        int remainingCount = Mathf.Min(requiredCount - insertedCount, thingToRequest.stackCount);
                        return new ThingCountClass(thingToRequest, remainingCount);
                    }
                }
            }

            return null; // No suitable thing found
        }

        public int GetInsertedThingCount(ThingDef thingDef) 
        {
            int count = 0;
            foreach (Thing insertedThing in _insertedThings)
            {
                if (insertedThing.def == thingDef)
                {
                    count += insertedThing.stackCount;
                }
            }
            return count;
        }

        public virtual List<ThingCountClass> GetRequestedThings() 
        {
            List<ThingCountClass> requestedThings = new List<ThingCountClass>();

            foreach (var pair in _expectedThings)
            {
                // Find the specific thing based on its definition
                Thing thingToRequest = FindThing(pair.Key);

                if (thingToRequest != null)
                {
                    int requiredCount = pair.Value; // How many are needed
                    int insertedCount = GetInsertedThingCount(pair.Key);

                    if (insertedCount < requiredCount)
                    {
                        int remainingCount = Mathf.Min(requiredCount - insertedCount, thingToRequest.stackCount);
                        requestedThings.Add(new ThingCountClass(thingToRequest, remainingCount));
                    }
                }
            }

            return requestedThings;
        }

        protected virtual Thing FindThing(ThingDef thingDef)
        {
            Thing thing = parent.Map.listerThings.AllThings
                .FirstOrDefault(x =>
                    x.def.defName == thingDef.defName &&
                    !x.IsForbidden(Faction.OfPlayer));
            return thing;
        }

        public virtual void OnThingHauled(Thing thing)
        {
            if (thing != null && _expectedThings != null && _expectedThings.Count > 0)
            {
                OnThingHauledInternal(thing);
                InsertThing(thing);

                // Ensure requirements are evaluated after modifying the state
                if (_expectedThings.All(pair => GetInsertedThingCount(pair.Key) >= pair.Value))
                {
                    OnRequirementSatisfied();
                }
            }
        }

        protected virtual void OnThingHauledInternal(Thing thing) 
        {
        }

        protected virtual void OnRequirementSatisfied() 
        {
            Log.Message("RequirementSatisfied");
        }

        protected virtual void Reset() 
        {
            foreach (Thing thing in _insertedThings)
            {
                DropThings(thing);
            }

            _expectedThings.Clear();
            _insertedThings.Clear();
        }

        protected virtual void DropThings(Thing thing)
        {
            // Destroy the thing if its stack count is 0
            if (thing.stackCount <= 0)
            {
                thing.Destroy();
                return;
            }

            int splitingCount = Mathf.CeilToInt((float)thing.stackCount / thing.def.stackLimit);
            for (int i = splitingCount; i > 0; i--)
            {
                int countToDrop = Mathf.Min(thing.stackCount, thing.def.stackLimit);

                // Split the stack if necessary
                Thing thingToDrop = thing.stackCount > countToDrop ? thing.SplitOff(countToDrop) : thing;

                // Spawn the dropped item
                GenSpawn.Spawn(thingToDrop, parent.Position, parent.Map, default(Rot4));
                Log.Message($"Dropped thing: {thingToDrop} x{countToDrop}/{thingToDrop.stackCount}");
            }
        }

        private void InsertThing(Thing thing)
        {
            if (_expectedThings.TryGetValue(thing.def, out int requiredAmount) && requiredAmount > 0)
            {
                // Calculate how many items are still required
                int remainingRequiredCount = requiredAmount - GetInsertedThingCount(thing.def);
                int countToInsert = Mathf.Min(remainingRequiredCount, thing.stackCount);

                if (countToInsert > 0)
                {
                    Thing stackableThing = _insertedThings.FirstOrDefault(existingThing => thing.def.stackLimit > 1 && existingThing.CanStackWith(thing));

                    if (stackableThing != null)
                    {
                        // Stack with an existing item
                        Thing stackToAbsorb = (countToInsert == thing.stackCount) ? thing : thing.SplitOff(countToInsert);
                        stackableThing.TryAbsorbStack(stackToAbsorb, false);

                        Log.Message($"Inserted stackable thing: {stackToAbsorb} {GetInsertedThingCount(stackToAbsorb.def)}");
                    }
                    else
                    {
                        // Insert as a new stack
                        Thing thingToInsert = (countToInsert == thing.stackCount) ? thing : thing.SplitOff(countToInsert);
                        _insertedThings.Add(thingToInsert);
                        thingToInsert.DeSpawn();

                        Log.Message($"Inserted non-stackable thing: {thingToInsert} {GetInsertedThingCount(thingToInsert.def)} stackCount:{thing.stackCount}");
                    }
                }
            }
        }
    }
}