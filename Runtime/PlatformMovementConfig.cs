//alex@bardicbytes.com
using BardicBytes.BardicFramework.Effects;
using BardicBytes.BardicFramework.Physics;
using UnityEngine;

namespace BardicBytes.BardicPlatformer
{

    [CreateAssetMenu(menuName = Prefixes.Platformer + "Movement Config")]
    public class PlatformMovementConfig : ScriptableObject
    {
        [field: SerializeField]
        public float MaxSpeed { get; protected set; } = 4;
        [field: SerializeField]
        public float MoveForce { get; protected set; } = 5;
        [field: SerializeField]
        public float JumpPower { get; protected set; } = 20;
        [field: SerializeField]
        public float JumpHoldPower { get; protected set; } = 8;
        [field: SerializeField]
        public bool JumpOnHold { get; protected set; } = false;
        [field: SerializeField]
        public float MaxJumpHeight { get; protected set; } = 2.5f;
        [field: SerializeField]
        public bool FastFall { get; protected set; } = false;
        [field: SerializeField]
        public bool AirStop { get; protected set; } = false;
        [field: SerializeField]
        public float WallJumpInputInturrupt { get; protected set; } = .1f;
        [field: SerializeField]
        public float AirJumpMinHeight { get; protected set; } = .15f;
        [field: SerializeField]
        public float BunnyBoost { get; protected set; } = 1f;
        [field: SerializeField]
        public int MaxAirJumps { get; protected set; } = 0;
        [field: SerializeField]
        public bool LimitHeightForRunningJumps { get; protected set; } = true;
        [field: SerializeField]
        public float AirControl { get; protected set; } = 0;

        [field: SerializeField]
        public PhysicMaterial Material { get; protected set; }
        [field: SerializeField]
        public PhysicsMaterial2D Material2D { get; protected set; }
        [field: SerializeField]
        public bool CanFly { get; protected set; }
        [field: SerializeField]
        public RigidbodyConfig RigidbodyConfig { get; protected set; }

        [field: SerializeField]
        public SoundEffect JumpSFX { get; protected set; }

        public float SqrMaxSpeed => MaxSpeed * MaxSpeed;

        public void Apply(Rigidbody rb, params Collider[] colliders)
        {
            rb.isKinematic = RigidbodyConfig.isKinematic;
            rb.useGravity = RigidbodyConfig.useGravity;
            rb.drag = RigidbodyConfig.drag;
            rb.angularDrag = RigidbodyConfig.angularDrag;

            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].sharedMaterial = Material;
            }
        }

        public void Apply(PlatformerBody2D body)
        {
            body.RigidBody2D.isKinematic = RigidbodyConfig.isKinematic;
            body.RigidBody2D.gravityScale = RigidbodyConfig.useGravity ? RigidbodyConfig.gravityScale2D : 0;
            body.RigidBody2D.drag = RigidbodyConfig.drag;
            body.RigidBody2D.angularDrag = RigidbodyConfig.angularDrag;

            body.Collider.sharedMaterial = Material2D;
        }
    }
}
