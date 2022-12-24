using BardicBytes.BardicFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BardicBytes.BardicPlatformerSamples
{
    [RequireComponent(typeof(TagModule))]
    public class Portal : ActorModule
    {
        [field: SerializeField]
        public Vector3 TargetA { get; protected set; } = default;
        [field:SerializeField]
        public Vector3 TargetB { get; protected set; } = default;
        [field: SerializeField]
        public Color Color { get; protected set; } = Color.white;
        [field: SerializeField]
        public ActorTag PortalableActors { get; protected set; }

        private void Reset()
        {
            TargetA = transform.position;
            TargetB = transform.position + Vector3.right*5f;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color;
            Gizmos.DrawLine(TargetA, TargetB);
        }

        protected override void ActorUpdate()
        {
            for(int i = 0; i < PortalableActors.ActiveActors.Count; i++)
            {
                var a = PortalableActors.ActiveActors[i];
                
            }
        }
    }
}
