using UnityEngine;

namespace ReimaginedFoundation
{
    public interface IDraweable
    {
        Rect DefaultRect { get; }
        Rect Rect { set; get; }

        void Draw(Rect rect);
    }
}
