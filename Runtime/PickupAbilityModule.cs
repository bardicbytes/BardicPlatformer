using BardicBytes.BardicFramework;
using UnityEngine;

namespace BardicBytes.BardicPlatformer
{
    public class PickupAbilityModule : ActorModule
    {
        [SerializeField]
        private string targetTag = "";
        private void OnTriggerEnter(UnityEngine.Collider other)
        {
            bool valid = true;
            if(!string.IsNullOrEmpty(targetTag))
            {
                valid |= other.CompareTag(targetTag);
            }
            if (!valid) return;
            var otherActor = other.GetComponent<Actor>();
            var ipm = otherActor.GetModule<ItemPickupModule>();
            ipm.DoPickup();
        }
    }
}
