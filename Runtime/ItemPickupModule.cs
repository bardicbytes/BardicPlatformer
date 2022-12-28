using BardicBytes.BardicFramework;
using BardicBytes.BardicFramework.Effects;
using UnityEngine;

namespace BardicBytes.BardicPlatformer
{
    public class ItemPickupModule : ActorModule
    {
        [SerializeField]
        private SoundEffect sfx = default;
        public void DoPickup()
        {
            sfx?.Play();
            Actor.SelfDestruct();
        }
    }
}
