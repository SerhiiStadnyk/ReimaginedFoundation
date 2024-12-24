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
        protected Dictionary<ThingDef, Thing> _insertedThings = new Dictionary<ThingDef, Thing>();

        public virtual Dictionary<ThingDef, int> ExpectedThings => _expectedThings;

        public abstract bool CanReceiveThings {  get; }

        public virtual ThingCount GetRequestedThing() 
        {
            foreach (var pair in _expectedThings)
            {
                Thing thing = FindThing(pair.Key);
                if (thing != null)
                {
                    if (!_insertedThings.ContainsKey(pair.Key))
                    {
                        return new ThingCount(thing, pair.Value);
                    }
                    else if (_insertedThings[pair.Key].stackCount < pair.Value)
                    {
                        int requiredCount = pair.Value - _insertedThings[pair.Key].stackCount;
                        return new ThingCount(thing, requiredCount);
                    }
                }
            }

            return null;
        }

        public virtual List<ThingCount> GetRequestedThings() 
        {
            List<ThingCount> requestedThings = new List<ThingCount>();

            foreach (var pair in _expectedThings)
            {
                Thing thing = FindThing(pair.Key);
                if (thing != null)
                {
                    if (!_insertedThings.ContainsKey(pair.Key))
                    {
                        requestedThings.Add(new ThingCount(thing, pair.Value));
                    }
                    else if (_insertedThings[pair.Key].stackCount < pair.Value)
                    {
                        int requiredCount = pair.Value - _insertedThings[pair.Key].stackCount;
                        requestedThings.Add(new ThingCount(thing, requiredCount));
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
            if (_expectedThings != null && _expectedThings.Count > 0 && thing != null)
            {
                OnThingHauledInternal(thing);
                InsertThing(thing);

                bool requirementSatisfied = _expectedThings.All(pair =>
                    _insertedThings.TryGetValue(pair.Key, out var thingCount) && thingCount.stackCount == pair.Value);
                if (requirementSatisfied)
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
        }

        protected virtual void Reset() 
        {
            Log.Warning("Base Reset");
            foreach (KeyValuePair<ThingDef, Thing> item in _insertedThings)
            {
                DropThings(item.Value);
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
            }
        }

        private void InsertThing(Thing thing)
        {
            if (_expectedThings.ContainsKey(thing.def) && _expectedThings[thing.def] > 0)
            {
                int requiredThingCount = _expectedThings[thing.def];
                int insertedThingCount;

                // If the thing is already inserted, adjust required count
                if (_insertedThings.ContainsKey(thing.def))
                {
                    requiredThingCount -= _insertedThings[thing.def].stackCount;
                }

                // Determine how much of the thing can be inserted
                insertedThingCount = Mathf.Min(requiredThingCount, thing.stackCount);

                // Handle inserting the thing or splitting it
                if (!_insertedThings.ContainsKey(thing.def))
                {
                    // Insert the entire stack if it fits, otherwise split it off
                    if (insertedThingCount == thing.stackCount)
                    {
                        _insertedThings.Add(thing.def, thing);
                        thing.DeSpawn();
                    }
                    else
                    {
                        Thing splitOffThing = thing.SplitOff(insertedThingCount);
                        _insertedThings.Add(thing.def, splitOffThing);
                    }
                }
                else
                {
                    // Try to absorb the stack
                    _insertedThings[thing.def].TryAbsorbStack(thing, false);
                }

                Log.Message($"Inserted thing: {_insertedThings[thing.def].def} {_insertedThings[thing.def].stackCount}");
            }
        }
    }
}