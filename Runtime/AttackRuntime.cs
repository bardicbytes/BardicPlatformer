using BardicBytes.BardicFramework.Actions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BardicBytes.BardicPlatformer
{
    [System.Serializable]
    public class AttackRuntime : GenericActionRuntime<AttackAction, AttackPerformer, AttackRuntime>
    {
        public AttackRuntime(AttackAction action, AttackPerformer performer) : base(action,performer)
        {
        }

        public override void StartAction()
        {
            base.StartAction();
            action.attackFX.Play(actionPerformer.AttackFXTarget);
            Debug.Log("Attack! " + action.name + " " + actionPerformer.AttackPower.Value);
        }
    }
}
