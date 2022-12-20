using System;

namespace CatGame
{
    [Flags]
    public enum LayerMasks
    {
        Default = 0x1,
        TransparentFX = 0x2,
        IgnoreRaycast = 0x4,
        Player = 0x8,
        Water = 0x10,
        UI = 0x20,
        NoPlayerCollision = 0x40,
        Interactables = 0x80,
        PlayerCollisionInteratables = 0x100,

        All = -1
    }

    public static class LayersExtensions
    {
        public static int ToLayer(this LayerMasks val)
            => (int)Math.Log((int)val, 2);
    }
}
