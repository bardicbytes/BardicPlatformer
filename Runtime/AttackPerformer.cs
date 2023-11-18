using BardicBytes.BardicFramework;
using BardicBytes.BardicFramework.Actions;
using BardicBytes.BardicFramework.EventVars;
using UnityEngine;

namespace BardicBytes.BardicPlatformer
{
    public class AttackPerformer : GenericActionPerformer<AttackAction, AttackPerformer, AttackRuntime>, IBardicEditorable, IUsePlatformerActionInput
    {
        public string[] EditorFieldNames => new string[] { };
        public bool DrawOtherFields => true;

        [field: SerializeField]
        public FloatEventVar.Field AttackPower { get; protected set; } = default;

        [field: SerializeField]
        public Transform AttackFXTarget { get; protected set; } = default;
        
        public IProvideActionInput ActionInputSource
        {
            get
            {
                if (currentActionInput == null && serializedInputSource != null)
                {
                    currentActionInput = serializedInputSource as IProvideActionInput;
                }
                return currentActionInput;
            }
            protected set
            {
                currentActionInput = value;
            }
        }


        [SerializeField]
        protected MonoBehaviour serializedInputSource;


        private IProvideActionInput currentActionInput;
        private IProvideActionInput defaultActionInput;

        public IProvideActionInput InputSource => serializedInputSource == null ? null : serializedInputSource as IProvideActionInput;

        protected override void OnValidate()
        {
            base.OnValidate();

            if (serializedInputSource == null)
            {
                serializedInputSource = GetComponent<IProvideActionInput>() as MonoBehaviour;
            }
            else if (!(serializedInputSource is IProvideActionInput))
            {
                serializedInputSource = serializedInputSource.GetComponent<IProvideActionInput>() as MonoBehaviour;
            }

            if (!(serializedInputSource is IProvideActionInput))
            {
                Debug.LogWarning("serializedInputSource must implement IProvideActionInput");
                serializedInputSource = null;
            }
        }

        protected override void ActorUpdate()
        {
            base.ActorUpdate();


        }


        public void ChangeInput(IProvideActionInput newInputSource)
        {
            if (defaultActionInput == null) defaultActionInput = this.ActionInputSource;
            this.ActionInputSource = newInputSource;
            Debug.Log(gameObject.name + " movement input source changed to " + ActionInputSource);
        }
    }
}
