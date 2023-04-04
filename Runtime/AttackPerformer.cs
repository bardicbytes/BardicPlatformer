using BardicBytes.BardicFramework;
using BardicBytes.BardicFramework.Actions;
using BardicBytes.BardicFramework.EventVars;
using UnityEngine;

namespace BardicBytes.BardicPlatformer
{
    public class AttackPerformer : GenericActionPerformer<AttackAction, AttackPerformer, AttackRuntime>, IBardicEditorable
    {
        public string[] EditorFieldNames => new string[] { };
        public bool DrawOtherFields => true;

        [field: SerializeField]
        public FloatEventVar.Field AttackPower { get; protected set; } = default;

        [field: SerializeField]
        public Transform AttackFXTarget { get; protected set; } = default;

        protected override void ActorUpdate()
        {

            base.ActorUpdate();
        }
    }
}
