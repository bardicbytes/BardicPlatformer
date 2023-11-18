//alex@bardicbytes.com

using static BardicBytes.BardicPlatformer.PlayerInputModule;

namespace BardicBytes.BardicPlatformer
{
    public interface IProvidePlatformMovementInput
    {
        PlatformMovementInputData MovementInputData { get; }
    }
}
