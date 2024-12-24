using System.Collections.Generic;
using Verse;

namespace ReimaginedFoundation
{
    public interface IThingRequester
    {
        Dictionary<ThingDef, int> ExpectedThings { get; }

        bool CanReceiveThings { get; }

        void OnThingHauled(Thing thing);

        ThingCountClass TryRequestedThing();
        List<ThingCountClass> GetRequestedThings();
    }
}
