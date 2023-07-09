using BardicBytes.BardicFramework;
using BardicBytes.BardicFramework.Effects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BardicBytes.BardicPlatformer
{
    public class PlatformerBody2D : ActorModule
    {
        [field:SerializeField] public LayerMask GroundLayer { get; protected set; }
        [field: SerializeField] public Transform GroundCheck { get; protected set; }

        [field: SerializeField] public SoundEffect LandingSFX { get; protected set; }

        public float GroundCheckRadius { get; protected set; } = 0.2f;
        public bool IsGrounded { get; protected set; } = true;

        [field: SerializeField] public Rigidbody2D RigidBody2D { get; protected set; }
        [field: SerializeField] public Animator Animator { get; protected set; }
        [field: SerializeField] public SpriteRenderer SpriteRenderer { get; protected set; }


        [field: SerializeField]
        public Collider2D Collider { get; protected set; }

        [field: SerializeField]
        public float GroundAngle { get; protected set; } = 45f;
        [field: SerializeField]
        public float CoyoteTime { get; protected set; } = .01f;

        public float VelocityY => RigidBody2D.velocity.y;
        public float VelocityX => RigidBody2D.velocity.x;

        public float DistFromGround { get; protected set; } = 0;
        public float DistFromLeft { get; protected set; } = 0;
        public float DistFromRight { get; protected set; } = 0;

        public bool IsWithinCoyoteTime => (Time.time - groundLostTime) <= CoyoteTime;

        private float FeetY => Actor.transform.position.y;
        
        private Collider2D groundHitCollider;
        private Vector2 groundNormal;
        private List<Collider2D> groundColliders;
        private float groundLostTime = 0;

        protected override void OnValidate()
        {
            base.OnValidate();
            if(Collider == null)Collider = GetComponentInChildren<Collider2D>();

            if (RigidBody2D == null) RigidBody2D = GetComponent<Rigidbody2D>();
            if (Animator == null) Animator = GetComponent<Animator>();
            if (GroundCheck == null) GroundCheck = transform;
        }

        private void Awake()
        {
            groundColliders = new List<Collider2D>();
        }

        protected override void ActorUpdate()
        {
            LeftRightCheck();
            DoGroundCheck();

            Animator?.SetFloat("SpeedX", Mathf.Abs(RigidBody2D.velocity.x));
            Animator?.SetFloat("SpeedY", Mathf.Abs(RigidBody2D.velocity.y));
            Animator?.SetBool("IsGrounded", IsGrounded);
        }

        private void DoGroundCheck()
        {
            var hit = Physics2D.Raycast(Collider.bounds.center, Vector2.down, Mathf.Infinity, this.GroundLayer);

            if (hit.collider != null)
            {
                groundHitCollider = hit.collider;
                DistFromGround = FeetY - hit.point.y;
                groundNormal = hit.normal;
            }
            else
            {
                groundHitCollider = null;
                DistFromGround = float.MaxValue;
                groundNormal = Vector2.right;
            }

            bool isLandable = NormalIsLandable(groundNormal);
            bool wasGrounded = IsGrounded;
            IsGrounded = Physics2D.OverlapCircle(GroundCheck.position, GroundCheckRadius, GroundLayer);

            if (IsGrounded && !isLandable && !wasGrounded)
            {
                float a = Vector2.Angle(Vector2.up, groundNormal);
                Debug.Log(a + " = ground angle.  GNorm:"+groundNormal+" hit.pint="+hit.point);
            }
            IsGrounded = isLandable && IsGrounded;

            if (IsGrounded && !wasGrounded)
            {
                if (!groundColliders.Contains(groundHitCollider)) groundColliders.Add(groundHitCollider);
                LandingSFX.Play();
            }
            else if (!IsGrounded && wasGrounded)
            {
                groundLostTime = Time.time;
                groundColliders.Clear();
            }
        }

        private void LeftRightCheck()
        {
            RaycastHit2D hit = Physics2D.Raycast(Collider.bounds.center, Vector2.right, Mathf.Infinity, this.GroundLayer);
            if (hit.collider != null)
            {
                DistFromRight = Mathf.Abs((Collider.bounds.center + (Collider.bounds.extents.x * Vector3.right)).x - hit.point.x);
            }
            else
            {
                DistFromRight = float.MaxValue;
            }

            hit = Physics2D.Raycast(Collider.bounds.center, Vector2.left, Mathf.Infinity, this.GroundLayer);
            if (hit.collider != null)
            {
                DistFromLeft = Mathf.Abs((Collider.bounds.center + (Collider.bounds.extents.x * Vector3.left)).x - hit.point.x);
            }
            else
            {
                DistFromLeft = float.MaxValue;
            }
        }

        private bool NormalIsLandable(Vector2 normal)
        {
            return Vector2.Angle(Vector2.up, normal) < GroundAngle;
        }

    }
}