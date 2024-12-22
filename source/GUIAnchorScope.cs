using System;
using UnityEngine;
using Verse;

namespace ReimaginedFoundation
{
    public class GUIAnchorScope : IDisposable
    {
        private readonly TextAnchor _previousAnchor;

        public GUIAnchorScope(TextAnchor newAnchor)
        {
            _previousAnchor = Text.Anchor;
            Text.Anchor = newAnchor;
        }

        public void Dispose()
        {
            Text.Anchor = _previousAnchor;
        }
    }
}