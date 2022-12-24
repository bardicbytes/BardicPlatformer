//alex@bardicbytes.com
using BardicBytes.BardicFramework;
using UnityEngine;

namespace BardicBytes.BardicPlatformer
{
    public class PlayerInputModule : ActorModule, IProvidePlatformMovementInput
    {
        public PlatformMovementInputData InputData { get; protected set; }

        protected override void ActorUpdate()
        {
            var j = Input.GetButtonDown("Jump");
            var jh = Input.GetButton("Jump");
            var jr = Input.GetButtonUp("Jump");
            var h = Input.GetAxis("Horizontal");
            var v = Input.GetAxis("Vertical");
            var d = new Vector2(h,v);
            InputData = new PlatformMovementInputData() { direction = d, jumpDown = j, jumpHeld = jh, jumpRelease = jr};
        }
    }
}
