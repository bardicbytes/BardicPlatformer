using BardicBytes.BardicFramework.Actions;
using UnityEngine;

namespace BardicBytes.BardicPlatformer
{
    [System.Serializable]
    public class AttackRuntime : GenericActionRuntime<AttackAction, AttackPerformer, AttackRuntime>
    {
        public AttackRuntime(AttackAction action, AttackPerformer performer) : base(action, performer)
        {
        }

        public override void StartAction()
        {
            base.StartAction();
            base.action.attackFX.Play(base.actionPerformer.AttackFXTarget);
            Debug.Log("Attack! " + action.name + " " + base.actionPerformer.AttackPower.Value);
        }
    }
}
