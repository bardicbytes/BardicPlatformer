//alex@bardicbytes.com
using BardicBytes.BardicFramework.Effects;
using BardicBytes.BardicFramework.EventVars;
using BardicBytes.BardicFramework.Physics;
using UnityEngine;

namespace BardicBytes.BardicPlatformer
{

    [CreateAssetMenu(menuName=Prefixes.Platformer+"Movement Config")]
    public class PlatformMovementConfig : ScriptableObject
    {
        [field: SerializeField]
        public float MaxSpeed { get; protected set; } = 4;
        [field: SerializeField]
        public float MoveForce { get; protected set; } = 5;
        [field: SerializeField]
        public float JumpPower { get; protected set; } = 5;
        [field: SerializeField]
        public bool JumpOnHold { get; protected set; } = false;
        [field: SerializeField]
        public bool FastFall { get; protected set; } = false;
        [field: SerializeField]
        public bool AirStop { get; protected set; } = false;
        [field: SerializeField]
        public float CoyoteTime { get; protected set; } = .01f;
        [field: SerializeField]
        public float AirJumpMinHeight { get; protected set; } = .15f;
        [field: SerializeField]
        public float BunnyBoost { get; protected set; } = 1f;
        [field: SerializeField]
        public int MaxAirJumps { get; protected set; } = 0;
        [field: SerializeField]
        public float AirControl { get; protected set; } = 0;

        [field: SerializeField]
        public PhysicMaterial Material { get; protected set; }
        [field: SerializeField]
        public bool PrecisionMovementEnabled { get; protected set; }
        [field: SerializeField]
        public bool CanFly { get; protected set; }
        [field: SerializeField]
        public RigidbodyConfig RigidbodyConfig { get; protected set; }

        [field: SerializeField]
        public SoundEffect JumpSFX { get; protected set; }

        public float SqrMaxSpeed => MaxSpeed * MaxSpeed;
        public ForceMode ForceMode => PrecisionMovementEnabled ? ForceMode.VelocityChange : ForceMode.Acceleration;
    
        public void Apply(Rigidbody rb, params Collider[] colliders)
        {
            rb.isKinematic = RigidbodyConfig.isKinematic;
            rb.useGravity = RigidbodyConfig.useGravity;
            rb.drag = RigidbodyConfig.drag;
            rb.angularDrag = RigidbodyConfig.angularDrag;

            for(int i = 0; i < colliders.Length; i++)
            {
                colliders[i].sharedMaterial = Material;
            }
        }
    }
}
