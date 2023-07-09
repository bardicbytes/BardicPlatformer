//alex@bardicbytes.com
using BardicBytes.BardicFramework;
using BardicBytes.BardicFramework.Actions;
using UnityEngine;

namespace BardicBytes.BardicPlatformer
{
    public class PlayerInputModule : ActorModule, IProvidePlatformMovementInput, IProvideActionInput
    {
        public PlatformMovementInputData MovementInputData { get; protected set; }
        public ActionInputData ActionInputData { get; protected set; }

        public string jumpButtonName = "Jump";
        public string action1ButtonName = "Fire1";
        public string action2ButtonName = "Fire2";
        public string HorizontalAxisName = "Horizontal";
        public string verticalAxisName = "Vertical";

        public AttackAction attack1;

        public bool maskHorizontal = false;

        protected override void ActorUpdate()
        {
            var j = Input.GetButtonDown(jumpButtonName);
            var jh = Input.GetButton(jumpButtonName);
            var jr = Input.GetButtonUp(jumpButtonName);
            var h = Input.GetAxis(HorizontalAxisName);
            var v = Input.GetAxis(verticalAxisName);
            
            if(maskHorizontal && Mathf.Approximately(h,0))
            {
                maskHorizontal = false;
            }
            else if (maskHorizontal)
            {
                h = 0;
            }

            var d = new Vector2(h, v);

            MovementInputData = new PlatformMovementInputData() { direction = d, jumpDown = j, jumpHeld = jh || j, jumpRelease = jr };

            var a1d = Input.GetButtonDown(action1ButtonName);
            var a1h = Input.GetButton(action1ButtonName);
            var a1r = Input.GetButtonUp(action1ButtonName);
            ActionInputData = new ActionInputData() { actionADown = a1d, actionAHeld = a1h, actionRelease = a1r };

            if (a1d) Actor.GetModule<AttackPerformer>().Perform(attack1);
        }

        public void BeginMaskingHorizontal()
        {
            maskHorizontal = true;
        }
    }
}

