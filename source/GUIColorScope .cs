using System;
using UnityEngine;

namespace ReimaginedFoundation
{
    public class GUIColorScope : IDisposable
    {
        private readonly Color _previousColor;

        public GUIColorScope(Color newColor)
        {
            _previousColor = GUI.color; // Store current GUI color
            GUI.color = newColor;       // Set new color
        }

        public void Dispose()
        {
            GUI.color = _previousColor; // Restore original color when scope ends
        }
    }
}