//alex@bardicbytes.com
using BardicBytes.BardicFramework;
using System.Collections.Generic;
using UnityEngine;

namespace BardicBytes.BardicPlatformer
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlatformMovementModule : ActorModule, IBardicEditorable
    {
        [field: SerializeField]
        public PlatformMovementConfigEventVar.Field ConfigField { get; protected set; }
        [SerializeField]
        protected MonoBehaviour serializedInputSource;

        public IProvidePlatformMovementInput InputSource => serializedInputSource == null ? null : serializedInputSource as IProvidePlatformMovementInput;

        [field: SerializeField]
        public LayerMask groundCastMask { get; protected set; } = -1;
        [field: SerializeField]
        public Collider[] Colliders { get; protected set; }
        [field: SerializeField]
        public float CoyoteTime { get; protected set; } = .01f;

        [field: Range(0, 180)]
        [field: SerializeField]
        public float GroundAngle { get; protected set; } = 45f;
        [field: SerializeField]
        public Transform bodyLookTarget { get; protected set; }

        public int AirJumpsMade { get; protected set; } = 99;
        public string[] EditorFieldNames => new string[] { };
        public bool DrawOtherFields => true;
        public PlatformMovementConfig Config => ConfigField;

        [field: SerializeField]
        [Tooltip("serialized for debug purposes")]
        public bool IsGrounded { get; protected set; } = false;

        private bool hasJumpRequest = false;
        private float groundLostTime = 0;
        private List<Collider> groundColliders;

        private Collider groundHitCollider;
        private float distFromGround = 0;
        private Vector3 groundNormal;
        
        private Vector2 moveForce;
        private float speedControlMultiplier;
        private float groundedHeight = 0f;
        private float maxHeightSinceLastGrounding = 0f;

        private bool jumpReleasedSincePressed = false;

        private bool IsWithinCoyoteTime => !IsJumping && (Time.time - groundLostTime) <= CoyoteTime;
        private float FeetY => Actor.transform.position.y;
        private bool ShouldAirJump => !IsGrounded
                    && jumpReleasedSincePressed
                    && !IsWithinCoyoteTime
                    && distFromGround >= Config.AirJumpMinHeight
                    && AirJumpsMade < Config.MaxAirJumps;

        /// <summary>
        /// Grounded or purposefully airborne AND we're not speeding
        /// </summary>
        private bool MayApplyMoveForce => (IsGrounded || Config.CanFly || IsJumping);

        private float LastJumpHeight => maxHeightSinceLastGrounding - groundedHeight;

        public bool IsJumping { get; protected set; } = false;

        protected override void OnValidate()
        {
            base.OnValidate();

            if (serializedInputSource == null)
            {
                serializedInputSource = GetComponent<IProvidePlatformMovementInput>() as MonoBehaviour;
            }
            else if (!(serializedInputSource is IProvidePlatformMovementInput))
            {
                serializedInputSource = serializedInputSource.GetComponent<IProvidePlatformMovementInput>() as MonoBehaviour;
            }

            if (!(serializedInputSource is IProvidePlatformMovementInput))
            {
                Debug.LogWarning("PlatformMovementModule.serializedInputSource must implement IProvidePlatformMovementInput");
                serializedInputSource = null;
            }

            if (bodyLookTarget == null) bodyLookTarget = transform;
        }

        protected virtual void Awake()
        {
            groundColliders = new List<Collider>();
            Config?.Apply(Actor.Rigidbody, Colliders);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            groundColliders.Clear();
        }
       

        /// <summary>
        /// collects ALL child colliders, this method is really just a context menu helper
        /// </summary>
        [ContextMenu("Add Colliders")]
        protected virtual void AddColliders()
        {
            Colliders = GetComponentsInChildren<Collider>();
        }

        protected override void ActorUpdate()
        {
            if (InputSource.MovementInputData.jumpUp)
            {
                jumpReleasedSincePressed = true;
                Debug.Log(Time.frameCount + " jump released");

            }
            else if (InputSource.MovementInputData.jumpDown || (InputSource.MovementInputData.jumpHeld && Config.JumpOnHold))
            {
                hasJumpRequest = true;
                Debug.Log(Time.frameCount+" jump request made");
            }

            Vector3 lap = bodyLookTarget.position + Actor.Rigidbody.velocity;
            lap.y = bodyLookTarget.position.y;
            bodyLookTarget.LookAt(lap);
            if (!IsGrounded && transform.position.y >= maxHeightSinceLastGrounding)
            {
                maxHeightSinceLastGrounding = transform.position.y;
            }
        }

        public override void CollectActorDebugInfo(System.Text.StringBuilder sb)
        {
            sb.AppendLine("<b>Platform Movement Module</b>");
            sb.AppendLineFormat("-IsGrounded: {0}", IsGrounded);
            sb.AppendLineFormat("-IsJumping: {0}", IsJumping);
            sb.AppendLineFormat("-DistFromGround: {0}", distFromGround.ToString("000.000"));
            sb.AppendLineFormat("-Coyote: {0}", IsWithinCoyoteTime);
            sb.AppendLineFormat("-AirJumps: {0}/{1}", AirJumpsMade, Config.MaxAirJumps);
            sb.AppendLineFormat("-Should AJ {0}", ShouldAirJump);
            sb.AppendLine("");
            sb.AppendLineFormat("-Input Dir: {0}", InputSource.MovementInputData.direction);
            sb.AppendLineFormat("-MayApplyMoveForce: {0}", MayApplyMoveForce);
            sb.AppendLineFormat("-MoveForce: {0}", moveForce);
            sb.AppendLineFormat("-Velocity: {0}, {1}", Actor.Rigidbody.velocity.x.ToString("00.00"), Actor.Rigidbody.velocity.y.ToString("00.00"));
            sb.AppendLine("");
            sb.AppendLineFormat("LastJumpPeak: {0}", LastJumpHeight);
            //sb.AppendLineFormat("-invSpeedRatio: {0}", invSpeedRatio);
            //sb.AppendLineFormat("-b: {0}", b);
            sb.AppendLineFormat("-speedControlMult: {0}", speedControlMultiplier);
            sb.AppendLine("");
            sb.AppendLineFormat("-other: {0}", otherDebugInfo);
        }

        string otherDebugInfo = "?";

        protected override void ActorFixedUpdate()
        {
            UpdateGrounded();
            DoMovement();

            void UpdateGrounded()
            {
                RaycastHit hit = default;
                if (Physics.Raycast(transform.position + Vector3.up * .015f, Vector3.down, out hit, Mathf.Infinity, this.groundCastMask, QueryTriggerInteraction.Ignore))
                {
                    groundHitCollider = hit.collider;
                    distFromGround = FeetY - hit.point.y;
                    groundNormal = hit.normal;
                }
                else
                {
                    groundHitCollider = null;
                    distFromGround = float.MaxValue;
                    groundNormal = Vector3.right;
                }

                if (!IsGrounded && NormalIsLandable(groundNormal) && distFromGround <= Config.AirJumpMinHeight && Actor.Rigidbody.velocity.y <= 0)
                {
                    Debug.Log(Time.frameCount + " Grounded. "+IsGrounded);
                    groundedHeight = transform.position.y;
                    IsGrounded = true;
                    IsJumping = false;
                    AirJumpsMade = 0;

                    if (!groundColliders.Contains(groundHitCollider)) groundColliders.Add(groundHitCollider);

                    if (!hasJumpRequest)
                    {
                        IsJumping = false;
                        hasJumpRequest = false;
                    }
                }
                else if(IsGrounded && ((hit.point - transform.position).sqrMagnitude > (Config.AirJumpMinHeight * Config.AirJumpMinHeight) || !NormalIsLandable(groundNormal)))
                {
                    groundLostTime = Time.time;
                    IsGrounded = false;
                    groundColliders.Clear();
                    maxHeightSinceLastGrounding = transform.position.y;
                }
            }
        }

        private bool NormalIsLandable(Vector3 normal)
        {
            return Vector3.Angle(Vector3.up, normal) < GroundAngle;
        }
        protected virtual void DoMovement()
        {
            // Calculate the maximum speed based on whether the character is grounded or not
            float maxSpeed = IsGrounded || Config.CanFly ? Config.MaxSpeed : Config.MaxSpeed * Config.AirControl;

            // Calculate the current speed and the ratio between the current speed and the maximum speed
            float currentSpeed = Mathf.Min(Mathf.Abs(Actor.Rigidbody.velocity.x), maxSpeed);
            float speedRatio = currentSpeed / maxSpeed;

            // Calculate the speed control multiplier based on the speed ratio
            float speedControlMultiplier = Mathf.Log10((1 - speedRatio) * 9 + 1);

            // Get the movement input direction
            Vector2 direction = InputSource.MovementInputData.direction;

            // Calculate the move force based on the direction and the speed control multiplier
            moveForce = direction * Config.MoveForce * speedControlMultiplier;

            // Limit lateral control in the air
            if (!IsGrounded && !Config.CanFly) moveForce.x *= Config.AirControl;

            // Prevent flight
            if (!Config.CanFly && moveForce.y > 0) moveForce.y = 0;

            // Prevent fastfall input force
            if (!IsGrounded && !Config.FastFall && moveForce.y < 0) moveForce.y = 0;

            // Add the move force to the rigidbody if it's allowed
            if (MayApplyMoveForce) Actor.Rigidbody.AddForce(moveForce, ForceMode.Force);

            // Cache the current velocity
            Vector3 velocity = Actor.Rigidbody.velocity;

            // Eliminate lateral movement when fastfalling
            if (!IsGrounded && Config.FastFall
                && Mathf.Abs(Actor.Rigidbody.velocity.x) > 0.001f
                && Mathf.Abs(direction.x) == 0
                && direction.y < 0)
            {
                velocity.x = 0;
            }

            // Stop the character if they are moving in the opposite direction of the input and precision movement is enabled
            if ((Config.AirStop || IsGrounded))
            {
                if (!Mathf.Approximately(0, velocity.x)
                    && (direction.x == 0 && Actor.Rigidbody.velocity.x > 0
                    || direction.x == 0 && Actor.Rigidbody.velocity.x < 0))
                {
                    velocity.x = 0;
                }
                // Limit the character's speed to the maximum if they are going too fast
                else if (IsGrounded && velocity.x > Config.MaxSpeed)
                {
                    velocity.x = Config.MaxSpeed;
                }
                else if (IsGrounded && velocity.x < -Config.MaxSpeed)
                {
                    velocity.x = -Config.MaxSpeed;
                }
            }

            // Set the updated velocity
            Actor.Rigidbody.velocity = velocity;

            TryJump();
        }

        private void TryJump()
        {
            if (!hasJumpRequest)
            {
                return;
            }
            Vector3 jumpDir = new Vector3(InputSource.MovementInputData.direction.x, 1, 0);
            var velocity = Actor.Rigidbody.velocity;

            // If the character is grounded and the jump button is pressed, start the jump
            if (IsGrounded)
            {
                Debug.Log(Time.frameCount + "grounded jump");

                velocity.y = 0;
                Actor.Rigidbody.velocity = velocity;

                jumpReleasedSincePressed = false;
                IsJumping = true;
                IsGrounded = false;
                var f = jumpDir * Config.JumpPower;
                Actor.Rigidbody.AddForce(f, ForceMode.Impulse);
            }
            // If the character is in the air and is allowed to air jump and the jump button is pressed, air jump
            else if (ShouldAirJump)
            {
                Debug.Log(Time.frameCount + "air jump");

                velocity.y = 0;
                Actor.Rigidbody.velocity = velocity;

                AirJumpsMade++;
                var f = jumpDir * Config.JumpPower;
                Actor.Rigidbody.AddForce(f, ForceMode.Impulse);
            }
            // If the character is in the air and is holding the jump button, apply a constant upward force to extend the jump
            else if (IsJumping && LastJumpHeight < Config.MaxJumpHeight)
            {
                Debug.Log(Time.frameCount + "extend jump");

                var f = jumpDir * Config.JumpHoldPower;
                Actor.Rigidbody.AddForce(f, ForceMode.Force);
            }

            hasJumpRequest = false;
        }

    }
}
