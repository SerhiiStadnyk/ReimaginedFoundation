using System.Collections.Generic;
using Verse;

namespace ReimaginedFoundation
{
    public interface IThingRequester
    {
        Dictionary<ThingDef, int> ExpectedThings { get; }

        bool CanReceiveThings { get; }

        void OnThingHauled(ThingCount thingCount);

        ThingCount GetRequestedThing();
        List<ThingCount> GetRequestedThings();
    }
}
