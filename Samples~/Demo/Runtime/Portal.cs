using BardicBytes.BardicFramework;
using BardicBytes.BardicFramework.Effects;
using BardicBytes.BardicFramework.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BardicBytes.BardicPlatformerSamples
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(BoxCollider))]
    public class Portal : ActorModule, IBardicEditorable
    {
        [field: SerializeField]
        public Portal Target { get; protected set; } = default;
        [field: SerializeField]
        public SpecialEffect BlinkOut { get; protected set; } = default;

        public bool DrawOtherFields => true;

        string[] IBardicEditorable.EditorFieldNames => new string[] { };

        bool IBardicEditorable.DrawOtherFields => true;

        private Actor teleportingActor = null;
        private BoxCollider boxCollider = null;
        protected override void OnValidate()
        {
            base.OnValidate();
            if (boxCollider == null) boxCollider = GetComponent<BoxCollider>();
            else boxCollider.isTrigger = true;

            if(Target != null && Target.Target == null)
            {
                Target.Target = this;
            }
        }

        private void OnDrawGizmos()
        {
            if (Target == null) return;
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(Target.transform.position, transform.position);

            Gizmos.color = teleportingActor == null ? Color.green : Color.red;
            Gizmos.DrawSphere(transform.position, .1f);
        }

        private void OnTriggerEnter(Collider other)
        {
            var actor = other.attachedRigidbody?.GetComponent<Actor>();
            if(actor == null)
            {
                //if(other.attachedRigidbody) Debug.Log("Portal Trigger, no actor: "+other.attachedRigidbody.gameObject.name);
                //else Debug.Log("Portal Trigger: no rigidbody"+other.gameObject.name);
                return;
            }

            if (actor == teleportingActor)
            {
                return;
            }

            actor.transform.position = Target.transform.position;
            Target.teleportingActor = actor;
            BlinkOut?.Play(actor.transform.position);
            BlinkOut?.Play(Target.transform.position);
        }

        private void OnTriggerExit(Collider other)
        {
            var actor = other.attachedRigidbody.GetComponent<Actor>();
            if (teleportingActor == actor)
            {
                teleportingActor = null;
            }
        }

    }
}
