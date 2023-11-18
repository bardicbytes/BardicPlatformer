//alex@bardicbytes.com
using BardicBytes.BardicFramework;
using BardicBytes.BardicFramework.Actions;
using System.Collections.Generic;
using UnityEngine;

namespace BardicBytes.BardicPlatformer
{
    public class PlayerInputModule : ActorModule, IProvidePlatformMovementInput, IProvideActionInput
    {
        public PlatformMovementInputData MovementInputData { get; protected set; }
        public ActionInputData[] ActionInputData { get {
                if (actionInputData == null || actionInputData.Length != 4) actionInputData = new ActionInputData[4];
                return actionInputData;
            }
            set
            {
                if (actionInputData == null || actionInputData.Length != 4) actionInputData = new ActionInputData[4];
                actionInputData = value;
            }
        }

        private ActionInputData[] actionInputData;


        public string jumpButtonName = "Jump";
        public string action1ButtonName = "Fire1";
        public string action2ButtonName = "Fire2";
        public string action3ButtonName = "Fire3";
        public string action4ButtonName = "Fire4";
        public string HorizontalAxisName = "Horizontal";
        public string verticalAxisName = "Vertical";
        public float horizontalMaskDur = .01f;

        protected override void ActorUpdate()
        {
            var jumpDown = Input.GetButtonDown(jumpButtonName);
            var jumpHeld = Input.GetButton(jumpButtonName);
            var jumpUp = Input.GetButtonUp(jumpButtonName);
            var h = Input.GetAxis(HorizontalAxisName);
            var v = Input.GetAxis(verticalAxisName);



            var d = new Vector2(h, v);

            MovementInputData = new PlatformMovementInputData() { direction = d, jumpDown = jumpDown, jumpHeld = jumpHeld || jumpDown, jumpUp = jumpUp };
            SetActionInputData(0, action1ButtonName);
            SetActionInputData(1, action1ButtonName);
            SetActionInputData(2, action1ButtonName);
            SetActionInputData(3, action1ButtonName);
        }

        private bool SetActionInputData(int index, string buttonName)
        {
            var down = Input.GetButtonDown(buttonName);
            var held = Input.GetButton(buttonName);
            var released = Input.GetButtonUp(buttonName);

            ActionInputData[index] = new ActionInputData() { actionDown = down, actionHeld = held, actionRelease = released };
            
            return down;
        }

    }
}

