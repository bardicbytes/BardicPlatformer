//alex@bardicbytes.com
using BardicBytes.BardicFramework;
using BardicBytes.BardicFramework.Effects;
using System.Collections.Generic;
using UnityEngine;

namespace BardicBytes.BardicPlatformer
{
    [RequireComponent(typeof(PlatformerBody2D))]

    [RequireComponent(typeof(Rigidbody2D))]
    public class PlatformMovementModule2D : ActorModule, IBardicEditorable
    {
        [field: SerializeField]
        public PlatformMovementConfigEventVar.Field ConfigField { get; protected set; }
        [SerializeField]
        protected MonoBehaviour serializedInputSource;

        public IProvidePlatformMovementInput InputSource => serializedInputSource == null ? null : serializedInputSource as IProvidePlatformMovementInput;

        [field: Range(0, 180)]
        
        [field: SerializeField]
        public Transform bodyLookTarget { get; protected set; }


        string otherDebugInfo = "?";

        //from the new CharacterController2D
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float airControl = 0.5f;
        [SerializeField] private float initialJumpForce = 2f;
        [SerializeField] private float additionalJumpForce = 5f;
        [SerializeField] private float maxJumpTime = 0.2f;
        [SerializeField] private float distanceThreshold = 0.01f;


        public bool IsWallClinging { get; protected set; } = false;
        public Vector2 wallJumpDirection { get; protected set; } = Vector2.up;
        public int AirJumpsMade { get; protected set; } = 99;
        public string[] EditorFieldNames => new string[] { };
        public bool DrawOtherFields => true;
        public PlatformMovementConfig Config => ConfigField;

        //private bool hasJumpRequest = false;

        private List<Collider> groundColliders;

        private Vector2 moveForce;
        private float speedControlMultiplier;
        private float groundedHeight = 0f;
        private float maxHeightSinceLastGrounding = 0f;

        private bool jumpReleasedSincePressed = false;

        private bool ShouldAirJump => !IsGrounded
                    && jumpReleasedSincePressed
                    && !Body.IsWithinCoyoteTime
                    && Body.DistFromGround >= Config.AirJumpMinHeight
                    && AirJumpsMade < Config.MaxAirJumps;

        /// <summary>
        /// Grounded or purposefully airborne AND we're not speeding
        /// </summary>
        private bool MayApplyMoveForce => (IsGrounded || Config.CanFly || IsJumping);
        private bool IsGrounded => Body.IsGrounded;
        private PlatformerBody2D Body => GetModule<PlatformerBody2D>();

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
            Config?.Apply(this.Body);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            groundColliders.Clear();
        }
        

        //protected override void ActorFixedUpdate()
        //{
        //    DoMovement();
        //}

        protected override void ActorUpdate()
        {
            Move();
            Jump();
            //OldMovement();

            //void OldMovement()
            //{
            //    if (InputSource.MovementInputData.jumpRelease)
            //    {
            //        jumpReleasedSincePressed = true;
            //        Debug.Log(Time.frameCount + " jump released");

            //    }
            //    else if (InputSource.MovementInputData.jumpDown || (InputSource.MovementInputData.jumpHeld && Config.JumpOnHold))
            //    {
            //        hasJumpRequest = true;
            //        Debug.Log(Time.frameCount + " jump request made");
            //    }

            //    Vector3 lap = bodyLookTarget.position + Actor.Rigidbody.velocity;
            //    lap.y = bodyLookTarget.position.y;
            //    bodyLookTarget.LookAt(lap);
            //    if (!IsGrounded && transform.position.y >= maxHeightSinceLastGrounding)
            //    {
            //        maxHeightSinceLastGrounding = transform.position.y;
            //    }
            //}
        }

        public override void CollectActorDebugInfo(System.Text.StringBuilder sb)
        {
            sb.AppendLine("<b>Platform Movement Module</b>");
            sb.AppendLineFormat("-IsGrounded: {0}", IsGrounded);
            sb.AppendLineFormat("-IsJumping: {0}", IsJumping);
            sb.AppendLineFormat("-DistFromGround: {0}", Body.DistFromGround.ToString("000.000"));
            sb.AppendLineFormat("-Coyote: {0}", Body.IsWithinCoyoteTime);
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


        protected void Move()
        {
            float horizontalInput = Input.GetAxis("Horizontal");

            // if we're grounded or not moving at max speed
            bool ShouldMove = Body.IsGrounded || Mathf.Abs(Body.RigidBody2D.velocity.x) < moveSpeed;
            bool inputBlocked = InputBlocked(this.InputSource.MovementInputData.direction);

            if (ShouldMove && !inputBlocked)
            {
                EndWallCling();
                float horizontalVelocity = horizontalInput * moveSpeed;
                float control = Body.IsGrounded ? 1f : airControl;
                Body.RigidBody2D.velocity = new Vector2(Mathf.Lerp(Body.VelocityX, horizontalVelocity, control), Body.VelocityY);
            }
            else if(inputBlocked)
            {
                BeginWallCling();
            }
            else
            {
                EndWallCling();
            }

            //flip sprite accordingly
            if (horizontalInput > distanceThreshold)
            {
                Body.SpriteRenderer.flipX = false;
            }
            else if (horizontalInput < -distanceThreshold)
            {
                Body.SpriteRenderer.flipX = true;
            }


            bool InputBlocked(Vector3 direction)
            {
                bool inputIsBlocked = false;
                if (direction.x > 0 && Body.DistFromRight <= distanceThreshold)
                {
                    //Debug.Log("dfR"+ Body.DistFromRight);
                    inputIsBlocked = true;
                }

                if (direction.x < 0 && Body.DistFromLeft <= distanceThreshold)
                {
                    inputIsBlocked = true;
                }

                return inputIsBlocked;
            }
        }

        private void EndWallCling()
        {
            if (!IsWallClinging) return;
            Body.RigidBody2D.constraints = RigidbodyConstraints2D.FreezeRotation;
            IsWallClinging = false;
            Body.Animator.SetBool("IsWallClinging", false);
        }

        private void BeginWallCling()
        {
            if (IsWallClinging) return;
            wallJumpDirection = Body.SpriteRenderer.flipX ? new Vector2(-.5f, .5f) : new Vector2(.5f, .5f);
            Body.RigidBody2D.constraints = RigidbodyConstraints2D.FreezePosition;
            IsWallClinging = true;
            Body.Animator.SetBool("IsWallClinging", true);
        }

        private void Jump()
        {
            if (!InputSource.MovementInputData.jumpDown || (!Body.IsGrounded && !IsWallClinging)) return;
            DoJump();
        }

        //from the new CharacterController2D
        private async void DoJump()
        {
            InputSource.BeginMaskingHorizontal();
            bool wasWallClinging = IsWallClinging;
            EndWallCling();
            
            float jumpTime = 0;
            Body.Animator.SetTrigger("Jump");
            if(wasWallClinging)
            {
                Body.RigidBody2D.AddForce(wallJumpDirection * initialJumpForce, ForceMode2D.Impulse);
            }
            else
            {
                Body.RigidBody2D.AddForce(Vector2.up * initialJumpForce, ForceMode2D.Impulse);

            }
            Config.JumpSFX.Play();
            bool setFlip = false;

            while (InputSource.MovementInputData.jumpHeld && jumpTime < maxJumpTime)
            {
                if (!setFlip && jumpTime >= maxJumpTime / 2f) Body.Animator.SetTrigger("Flip");
                Body.RigidBody2D.AddForce(new Vector2(0f, additionalJumpForce), ForceMode2D.Force);
                jumpTime += Time.deltaTime;
                await System.Threading.Tasks.Task.Yield();
            }
        }


        ////from the old PlatformMovementModule
        //protected virtual void DoMovement()
        //{
        //    // Calculate the maximum speed based on whether the character is grounded or not
        //    float maxSpeed = IsGrounded || Config.CanFly ? Config.MaxSpeed : Config.MaxSpeed * Config.AirControl;

        //    // Calculate the current speed and the ratio between the current speed and the maximum speed
        //    float currentSpeed = Mathf.Min(Mathf.Abs(Actor.Rigidbody.velocity.x), maxSpeed);
        //    float speedRatio = currentSpeed / maxSpeed;

        //    // Calculate the speed control multiplier based on the speed ratio
        //    float speedControlMultiplier = Mathf.Log10((1 - speedRatio) * 9 + 1);

        //    // Get the movement input direction
        //    Vector2 direction = InputSource.MovementInputData.direction;

        //    // Calculate the move force based on the direction and the speed control multiplier
        //    moveForce = direction * Config.MoveForce * speedControlMultiplier;

        //    // Limit lateral control in the air
        //    if (!IsGrounded && !Config.CanFly) moveForce.x *= Config.AirControl;

        //    // Prevent flight
        //    if (!Config.CanFly && moveForce.y > 0) moveForce.y = 0;

        //    // Prevent fastfall input force
        //    if (!IsGrounded && !Config.FastFall && moveForce.y < 0) moveForce.y = 0;

        //    // Add the move force to the rigidbody if it's allowed
        //    if (MayApplyMoveForce) Actor.Rigidbody.AddForce(moveForce, ForceMode.Force);

        //    // Cache the current velocity
        //    Vector3 velocity = Actor.Rigidbody.velocity;

        //    // Eliminate lateral movement when fastfalling
        //    if (!IsGrounded && Config.FastFall
        //        && Mathf.Abs(Actor.Rigidbody.velocity.x) > 0.001f
        //        && Mathf.Abs(direction.x) == 0
        //        && direction.y < 0)
        //    {
        //        velocity.x = 0;
        //    }

        //    // Stop the character if they are moving in the opposite direction of the input and precision movement is enabled
        //    if ((Config.AirStop || IsGrounded))
        //    {
        //        if (!Mathf.Approximately(0, velocity.x)
        //            && (direction.x == 0 && Actor.Rigidbody.velocity.x > 0
        //            || direction.x == 0 && Actor.Rigidbody.velocity.x < 0))
        //        {
        //            velocity.x = 0;
        //        }
        //        // Limit the character's speed to the maximum if they are going too fast
        //        else if (IsGrounded && velocity.x > Config.MaxSpeed)
        //        {
        //            velocity.x = Config.MaxSpeed;
        //        }
        //        else if (IsGrounded && velocity.x < -Config.MaxSpeed)
        //        {
        //            velocity.x = -Config.MaxSpeed;
        //        }
        //    }

        //    // Set the updated velocity
        //    Actor.Rigidbody.velocity = velocity;

        //    TryJump();
        //}

        ////from the old PlatformMovementModule
        //private void TryJump()
        //{
        //    if (!hasJumpRequest)
        //    {
        //        return;
        //    }
        //    Vector3 jumpDir = new Vector3(InputSource.MovementInputData.direction.x, 1, 0);
        //    var velocity = Actor.Rigidbody.velocity;

        //    // If the character is grounded and the jump button is pressed, start the jump
        //    if (IsGrounded)
        //    {
        //        Debug.Log(Time.frameCount + "grounded jump");

        //        velocity.y = 0;
        //        Actor.Rigidbody.velocity = velocity;

        //        jumpReleasedSincePressed = false;
        //        IsJumping = true;
        //        var f = jumpDir * Config.JumpPower;
        //        Actor.Rigidbody.AddForce(f, ForceMode.Impulse);
        //    }
        //    // If the character is in the air and is allowed to air jump and the jump button is pressed, air jump
        //    else if (ShouldAirJump)
        //    {
        //        Debug.Log(Time.frameCount + "air jump");

        //        velocity.y = 0;
        //        Actor.Rigidbody.velocity = velocity;

        //        AirJumpsMade++;
        //        var f = jumpDir * Config.JumpPower;
        //        Actor.Rigidbody.AddForce(f, ForceMode.Impulse);
        //    }
        //    // If the character is in the air and is holding the jump button, apply a constant upward force to extend the jump
        //    else if (IsJumping && LastJumpHeight < Config.MaxJumpHeight)
        //    {
        //        Debug.Log(Time.frameCount + "extend jump");

        //        var f = jumpDir * Config.JumpHoldPower;
        //        Actor.Rigidbody.AddForce(f, ForceMode.Force);
        //    }

        //    hasJumpRequest = false;
        //}

    }
}