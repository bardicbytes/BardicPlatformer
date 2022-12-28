//alex@bardicbytes.com
using BardicBytes.BardicFramework;
using BardicBytes.BardicFramework.EventVars;
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

        [field:SerializeField]
        public LayerMask groundCastMask { get; protected set; } = -1;
        [field:SerializeField]
        public Collider[] Colliders { get; protected set; }
        
        [field:Range(0,180)]
        [field:SerializeField]
        public float GroundAngle { get; protected set; } = 45f;

        public int AirJumpsMade { get; protected set; } = 99;
        public string[] EditorFieldNames => new string[] {};
        public bool DrawOtherFields => true;
        public PlatformMovementConfig Config => ConfigField;
        
        [field:SerializeField]
        [Tooltip("serialized for debug purposes")]
        public bool IsGrounded { get; protected set; } = false;

        private bool hasJumpReq = false;
        private bool isJumping = false;
        private float groundLostTime = 0;
        private List<Collider> groundColliders;

        private float distFromGround = 0;
        private Vector3 groundNormal;
        private Vector2 moveForce;
        private float speedControlMult;
        private float invSpeedRatio;
        private float b;

        private bool IsWithinCoyoteTime => !isJumping && (Time.time - groundLostTime) <= Config.CoyoteTime;
        private float FeetY => Actor.transform.position.y;
        private bool ShouldAirJump => !IsGrounded 
                    && !IsWithinCoyoteTime
                    && distFromGround >= Config.AirJumpMinHeight
                    && AirJumpsMade < Config.MaxAirJumps;

        /// <summary>
        /// Grounded or purposefully airborne AND we're not speeding
        /// </summary>
        private bool MayApplyMoveForce => (IsGrounded || Config.CanFly || isJumping)
                    && Actor.Rigidbody.velocity.x + moveForce.x * Time.fixedDeltaTime < Config.MaxSpeed
                    && Actor.Rigidbody.velocity.x + moveForce.x * Time.fixedDeltaTime > -Config.MaxSpeed;

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

        protected virtual void OnCollisionEnter(Collision c)
        {
            CheckForLanding(c);
        }


        protected virtual void OnCollisionStay(Collision c)
        {
            CheckForLanding(c);
        }

        protected virtual void OnCollisionExit(Collision c)
        {
            if(groundColliders.Contains(c.collider)) DoGroundLost();
        }
        private void CheckForLanding(Collision c)
        {
            var normal = c.contacts[0].normal;
            if (IsGrounded || NormalIsLandable(normal)) return;
            DoLanding(c.collider);
        }

        private bool NormalIsLandable(Vector3 normal)
        {
            return Vector3.Angle(Vector3.up, normal) > GroundAngle;
        }

        private void DoGroundLost()
        {
            groundLostTime = Time.time;
            IsGrounded = false;
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
            if (InputSource.MovementInputData.jumpDown || (InputSource.MovementInputData.jumpHeld && Config.JumpOnHold))
            {
                MakeJumpRequest();
            }
        }

        public void MakeJumpRequest()
        {
            hasJumpReq = true;
        }
        
        private void DoLanding(Collider c)
        {
            Debug.Assert(c != null);
            Debug.Assert(groundColliders != null);

            IsGrounded = true;
            AirJumpsMade = 0;
            
            if (!groundColliders.Contains(c)) groundColliders.Add(c);

            //bunnyhop
            if (hasJumpReq && InputSource.MovementInputData.jumpHeld)
            {
                MakeJumpRequest();
            }
            else
            {
                isJumping = false;
                hasJumpReq = false;
            }
        }
        public override void CollectActorDebugInfo(System.Text.StringBuilder sb)
        {
            sb.AppendLine("<b>Platform Movement Module</b>");
            sb.AppendLineFormat("-IsGrounded: {0}", IsGrounded);
            sb.AppendLineFormat("-IsJumping: {0}", isJumping);
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
            sb.AppendLineFormat("-invSpeedRatio: {0}", invSpeedRatio);
            sb.AppendLineFormat("-b: {0}", b);
            sb.AppendLineFormat("-speedControlMult: {0}", speedControlMult);
            sb.AppendLine("");
            sb.AppendLineFormat("-other: {0}", otherDebugInfo);
        }

        string otherDebugInfo = "?";

        protected override void ActorFixedUpdate()
        {
            RecordDistFromGround();
            DoMovement();

            void RecordDistFromGround()
            {
                RaycastHit hit = default;
                if (!IsGrounded
                    && Physics.Raycast(transform.position, Vector3.down, out hit, Mathf.Infinity, this.groundCastMask, QueryTriggerInteraction.Ignore))
                {
                    distFromGround = FeetY - hit.point.y;
                    groundNormal = hit.normal;
                }
            }
        }

        protected virtual void DoMovement()
        {
            otherDebugInfo = "DoMovement";

            var airMaxSpeed = Config.MaxSpeed * Config.AirControl;

            var effMaxSpeed = IsGrounded || Config.CanFly ? Config.MaxSpeed : airMaxSpeed;

            var curSpeed = Mathf.Min(Mathf.Abs(Actor.Rigidbody.velocity.x), effMaxSpeed);
            var speedRatio = (curSpeed / effMaxSpeed);//1 = max speed, 0 = not moving
            invSpeedRatio = 1 - speedRatio;// 1 = not moving, 0 = fullspeed
            b = invSpeedRatio * 9 + 1;
            speedControlMult = Mathf.Log10(/*1 to 10*/b);//0-1, f(5) = .699

            var dir = InputSource.MovementInputData.direction;
            moveForce = dir * Config.MoveForce * speedControlMult;

            //limit lateral control in air
            if (!IsGrounded && !Config.CanFly) moveForce.x *= Config.AirControl * (isJumping ? 1 : 0);

            //prevent flight
            if (!Config.CanFly && moveForce.y > 0) moveForce.y = 0;

            //prevent fastfall input force
            if (!IsGrounded && !Config.FastFall && moveForce.y < 0) moveForce.y = 0;

            if (MayApplyMoveForce) Actor.Rigidbody.AddForce(moveForce, Config.ForceMode);

            var v = Actor.Rigidbody.velocity; //cache

            //eliminate lateral movement when fastfalling
            if (!IsGrounded && Config.FastFall
                && Mathf.Abs(Actor.Rigidbody.velocity.x) > .001f
                && Mathf.Abs(dir.x) == 0
                && dir.y < 0)
            {
                v.x = 0;
            }

            otherDebugInfo += "\n" + moveForce.x.ToString("00.00") + ", " + moveForce.y.ToString("00.00");

            if ((Config.AirStop || IsGrounded) && Config.PrecisionMovementEnabled)
            {
                //we're moving and trying to move in the opposite direction
                if (!Mathf.Approximately(0, v.x)
                    && (dir.x <= 0 && Actor.Rigidbody.velocity.x > 0
                    || dir.x >= 0 && Actor.Rigidbody.velocity.x < 0))
                {
                    v.x = 0;
                    otherDebugInfo += "\nPrecision stopping";
                }
                else if (IsGrounded && v.x > Config.MaxSpeed)//speeding
                {
                    otherDebugInfo += "\nRIGHT ____________speeding";
                    v.x = Config.MaxSpeed;
                }
                else if (IsGrounded && v.x < -Config.MaxSpeed)//speeding
                {
                    otherDebugInfo += "\nLEFT _____________speeding";
                    v.x = -Config.MaxSpeed;
                }
                else
                {
                    otherDebugInfo += "\n " + (Config.AirStop ? "Precision Can AirStop" : "Precision Grounded");
                }
            }
            else if (Config.PrecisionMovementEnabled)
            {
                otherDebugInfo += "\nairborne";
            }

            Actor.Rigidbody.velocity = v;

            TryJump();

            void TryJump()
            {
                if (!hasJumpReq) return;
                bool should = true;
                if (!IsGrounded && !IsWithinCoyoteTime) should = false;

                bool isAirJump = ShouldAirJump;
                if (isAirJump)
                {
                    should = true;
                    AirJumpsMade++;
                }
                if (!should) return;

                //JUMP!
                hasJumpReq = false;
                var inputX = 0;
                if (Actor.Rigidbody.velocity.x > .01) inputX = 1;
                else if (Actor.Rigidbody.velocity.x < -.01) inputX = -1;

                //air jump direction change
                var v = Actor.Rigidbody.velocity;
                //if input opposes velocity, zero out velocity first
                if (isAirJump
                    && (inputX > Actor.Rigidbody.velocity.x || inputX < Actor.Rigidbody.velocity.x)
                    && !Mathf.Approximately(0, inputX))
                {
                    v.x = 0;
                }

                if (isAirJump) v.y = 0;
                Actor.Rigidbody.velocity = v;

                var jumpForce = new Vector3(inputX / 1, 1, 0).normalized * Config.JumpPower;
                if (!isAirJump
                    && InputSource.MovementInputData.jumpHeld
                    && isJumping)
                {
                    jumpForce.y *= Config.BunnyBoost;
                }

                Actor.Rigidbody.AddForce(jumpForce, ForceMode.VelocityChange);

                v = Actor.Rigidbody.velocity;
                if (Actor.Rigidbody.velocity.x > Config.MaxSpeed)
                {
                    if (v.x > 0) v.x = Config.MaxSpeed;
                    if (v.x < 0) v.x = -Config.MaxSpeed;
                }
                Actor.Rigidbody.velocity = v;
                //Debug.Log(Time.frameCount + "f. Jump! " + Actor.Rigidbody.velocity + ", AJ?" + isAirJump + ", J?" + isJumping + ", G? " + IsGrounded);
                Config.JumpSFX.Play();
                isJumping = true;
                DoGroundLost();
            }
        }
    }
}
