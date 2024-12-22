using ReimaginedFoundation;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace ReimaginedFoundation
{
    public abstract class ThingCompVirtualThingHolder : ThingComp, IThingRequester
    {
        protected Dictionary<ThingDef, int> _expectedThings = new Dictionary<ThingDef, int>();
        protected Dictionary<ThingDef, ThingCount> _insertedThings = new Dictionary<ThingDef, ThingCount>();

        public virtual Dictionary<ThingDef, int> ExpectedThings => _expectedThings;

        public abstract bool CanReceiveThings {  get; }

        public abstract ThingCount GetRequestedThing();

        public abstract List<ThingCount> GetRequestedThings();

        public virtual void OnThingHauled(ThingCount ingredient)
        {
            if (ingredient.Thing != null)
            {
                OnThingHauledInternal(ingredient);
                InsertThing(ingredient);

                bool hasAllThings = _expectedThings.All(pair =>
                    _insertedThings.TryGetValue(pair.Key, out var thingCount) && thingCount.Count == pair.Value);
                if (hasAllThings)
                {
                    OnAllThingsHauled();
                }
            }
        }

        protected virtual void OnThingHauledInternal(ThingCount item) 
        {
        }

        protected virtual void OnAllThingsHauled() 
        {
        }

        //protected virtual void DropItems(Thing thing, int count)
        //{
        //    Thing thingInstance = GenSpawn.Spawn(thing, parent.Position, parent.Map, default(Rot4));
        //    thingInstance.stackCount = count;
        //}

        private void InsertThing(ThingCount item)
        {
            if (_expectedThings.ContainsKey(item.Thing.def)) 
            {
                if (!_insertedThings.ContainsKey(item.Thing.def)) 
                {
                    _insertedThings.Add(item.Thing.def, new ThingCount(item.Thing, 0));
                }

                int requiredThingCount = _expectedThings[item.Thing.def] - _insertedThings[item.Thing.def].Count;
                int insertedThingCount = Mathf.Min(requiredThingCount, item.Count);
                _insertedThings[item.Thing.def] = new ThingCount(item.Thing, insertedThingCount);

                if (insertedThingCount == item.Count)
                {
                    item.Thing.DeSpawn();
                }
                else
                {
                    item.Thing.stackCount -= insertedThingCount;
                }

                Log.Message($"Inserted thing: {_insertedThings[item.Thing.def].Thing.def} {_insertedThings[item.Thing.def].Count}");

                //int excessThingCount = item.Count - insertedThingCount;
                //if (excessThingCount > 0) 
                //{
                //    DropItems(item.Thing, excessThingCount);
                //}
            }
        }
    }
}
