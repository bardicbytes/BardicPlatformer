//alex@bardicbytes.com
using UnityEngine;

namespace BardicBytes.BardicPlatformer
{
    public interface IProvidePlatformMovementInput
    {
        PlatformMovementInputData InputData { get; }
    }

    public struct PlatformMovementInputData
    {
        public Vector2 direction;
        public bool jumpDown;
        public bool jumpHeld;
        public bool jumpRelease;
    }
}
