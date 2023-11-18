//alex@bardicbytes.com
using BardicBytes.BardicFramework;
using BardicBytes.BardicFramework.Actions;
using BardicBytes.BardicFramework.Effects;
using System.Collections.Generic;
using UnityEngine;
using static BardicBytes.BardicPlatformer.PlayerInputModule;

namespace BardicBytes.BardicPlatformer
{
    [RequireComponent(typeof(PlatformerBody2D))]

    [RequireComponent(typeof(Rigidbody2D))]
    public class PlatformMovementModule2D : ActorModule, IBardicEditorable, IUsePlatformerMovementInput
    {
        public enum OverrideType { Horizontal }
        public struct InputOverride
        {
            public OverrideType overrideType;
            public float amount;
            public float endTime;
        }

        [field: SerializeField]
        public PlatformMovementConfigEventVar.Field ConfigField { get; protected set; }
        [SerializeField]
        protected MonoBehaviour serializedInputSource;

        [field: SerializeField] public SoundEffect wallClingSFX { get; protected set; }

        public IProvidePlatformMovementInput MovementInputSource {
            get
            {
                if(currentMovementInput == null && serializedInputSource != null)
                {
                    currentMovementInput = serializedInputSource as IProvidePlatformMovementInput;
                }
                return currentMovementInput;
            }
            protected set
            {
                currentMovementInput = value;
            }
        }
        [SerializeField]
        private float wallClingJumpOverrideDur = 2f;
        [field: Range(0, 180)]
        
        [field: SerializeField]
        public Transform bodyLookTarget { get; protected set; }

        //from the new CharacterController2D
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float airControl = 0.5f;
        [SerializeField] private float initialJumpForce = 2f;
        [SerializeField] private float additionalJumpForce = 5f;
        [SerializeField] private float maxJumpTime = 0.2f;
        [SerializeField] private float distanceThreshold = 0.01f;
        [SerializeField] private float wallClingLinger = 0.35f;

        private List<InputOverride> inputOverrides = new List<InputOverride>();

        public bool IsWallClinging { get; protected set; } = false;
        public Vector2 wallJumpDirection { get; protected set; } = Vector2.up;
        public int AirJumpsMade { get; protected set; } = 99;
        public string[] EditorFieldNames => new string[] { };
        public bool DrawOtherFields => true;
        public PlatformMovementConfig Config => ConfigField;

        private IProvidePlatformMovementInput currentMovementInput;
        private IProvidePlatformMovementInput defaultMovementInput;

        string otherDebugInfo = "?";

        private List<Collider> groundColliders;

        private Vector2 moveForce;
        private float speedControlMultiplier;
        private float groundedHeight = 0f;
        private float maxHeightSinceLastGrounding = 0f;

        private Vector2 jumpForce = Vector2.zero;
        private bool hasJumpRequest = false;
        private float jumpDurationElapsed = 0;
        private bool setFlip = false;
        private float wallClingLingerEndTime = 0;

        private bool wallClingAvailable = true;

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
            this.Body.justGrounded += Body_justGrounded;
        }


        protected override void OnEnable()
        {
            base.OnEnable();
            groundColliders.Clear();
            hasJumpRequest = false;
        }

        protected override void ActorFixedUpdate()
        {
            DoJumping();
        }

        protected override void ActorUpdate()
        {
            DoMovement();
            CollectJump();

            if (!IsJumping) return;
            jumpDurationElapsed += Time.deltaTime;
            if (!setFlip && jumpDurationElapsed >= maxJumpTime / 2f)
            {
                setFlip = true;
                Body.Animator.SetTrigger("Flip");
            }
        }

        private void DoJumping()
        {
            if (hasJumpRequest)
            {
                // if the player was wall clinging, end the wall cling
                bool wasWallClinging = IsWallClinging;
                EndWallCling();
                wallClingAvailable = true;

                // set jump direction direction based on if we were wall clinging
                jumpForce = Vector3.up * initialJumpForce;
                
                if (wasWallClinging)
                {
                    jumpForce = wallJumpDirection * initialJumpForce;
                    Debug.Log(jumpForce);
                    int amount = MovementInputSource.MovementInputData.direction.x > distanceThreshold ? 1 : -1;
                    inputOverrides.Add(new InputOverride() { overrideType = OverrideType.Horizontal,
                    endTime = Time.time + wallClingJumpOverrideDur,
                    amount = amount });
                }

                Body.RigidBody2D.AddForce(jumpForce, ForceMode2D.Impulse);

                hasJumpRequest = false;
                jumpDurationElapsed = 0;
                Config.JumpSFX.Play();
                Body.Animator.SetTrigger("Jump");

                IsJumping = true;
            }

            if (IsJumping && MovementInputSource.MovementInputData.jumpHeld && jumpDurationElapsed < maxJumpTime)
            {
                Body.RigidBody2D.AddForce(new Vector2(0f, additionalJumpForce), ForceMode2D.Force);
            }
        }

        private void Body_justGrounded()
        {
            EndWallCling();
            setFlip = false;
            IsJumping = false;
        }

        private void CollectJump()
        {
            if (!MovementInputSource.MovementInputData.jumpDown || (!Body.IsGrounded && !IsWallClinging)) return;

            hasJumpRequest = true;
        }

        public override void CollectActorDebugInfo(System.Text.StringBuilder sb)
        {
            sb.AppendLine("<b>Platform Movement Module</b>");
            sb.AppendLineFormat("-IsGrounded: {0}", IsGrounded);
            sb.AppendLineFormat("-IsJumping: {0}", IsJumping);
            sb.AppendLineFormat("-DistFromGround: {0}", Body.DistFromGround.ToString("000.000"));
            sb.AppendLineFormat("-Coyote: {0}", Body.IsWithinCoyoteTime);
            sb.AppendLineFormat("-AirJumps: {0}/{1}", AirJumpsMade, Config.MaxAirJumps);
            sb.AppendLine("");
            sb.AppendLineFormat("-Input Dir: {0}", MovementInputSource.MovementInputData.direction);
            sb.AppendLineFormat("-MayApplyMoveForce: {0}", MayApplyMoveForce);
            sb.AppendLineFormat("-MoveForce: {0}", moveForce);
            sb.AppendLineFormat("-Velocity: {0}, {1}", Actor.Rigidbody.velocity.x.ToString("00.00"), Actor.Rigidbody.velocity.y.ToString("00.00"));
            sb.AppendLine("");
            sb.AppendLineFormat("LastJumpPeak: {0}", LastJumpHeight);
            sb.AppendLineFormat("-speedControlMult: {0}", speedControlMultiplier);
            sb.AppendLine("");
            sb.AppendLineFormat("-other: {0}", otherDebugInfo);
        }

        protected void DoMovement()
        {
            float horizontalInput = MovementInputSource.MovementInputData.direction.x;
            for (int i = 0; i < inputOverrides.Count; i++)
            {
                if (Time.time >= inputOverrides[i].endTime)
                {
                    inputOverrides.RemoveAt(i);
                    i--;
                    continue;
                }

                switch (inputOverrides[i].overrideType)
                {
                    case OverrideType.Horizontal:
                        horizontalInput = inputOverrides[i].amount;
                        //Debug.Log("overriding horizontal input. " + horizontalInput);
                        break;
                }
            }

            // if we're grounded or not moving at max speed
            bool couldMove = Body.IsGrounded || Mathf.Abs(Body.RigidBody2D.velocity.x) < moveSpeed;
            bool inputBlocked = InputBlocked(this.MovementInputSource.MovementInputData.direction);

            // move when able
            if (this.MovementInputSource.MovementInputData.direction.sqrMagnitude > 0 && couldMove && !inputBlocked)
            {
                EndWallCling();
                float horizontalVelocity = horizontalInput * moveSpeed;
                float targetVelocityX = horizontalVelocity;

                //if (!IsGrounded)
                //{
                //    //press right, moving slowly to the right
                //    if(horizontalInput > 0 && Body.VelocityX > 0 && Body.VelocityX < moveSpeed * airControl)
                //    {
                //        //speed up to full air control speed right
                //        targetVelocityX = moveSpeed * airControl;

                //    }
                //    //press right, moving fast left
                //    else if (horizontalInput > 0 && Body.VelocityX < -moveSpeed * airControl)
                //    {
                //        targetVelocityX = 0;
                //        Debug.Log("air stop left");
                //    }
                //    //press left, moving slowly to the left
                //    else if (horizontalInput < 0 && Body.VelocityX > -moveSpeed * airControl)
                //    {
                //        //speed up to full air control speed left
                //        targetVelocityX = -moveSpeed * airControl;
                //    }
                //    //press left, moving fast right
                //    else if (horizontalInput < 0 && Body.VelocityX > moveSpeed * airControl)
                //    {
                //        targetVelocityX = 0;
                //        Debug.Log("air stop right");
                //    }
                //    else
                //    {
                //        targetVelocityX = Body.VelocityX;
                //    }
                //}
                //if (!IsGrounded && horizontalInput > 0)
                //{
                //    targetVelocityX = Mathf.MoveTowards(Body.VelocityX, horizontalVelocity, moveSpeed * airControl * Time.deltaTime);
                //}
                //else if (!IsGrounded && horizontalInput < 0)
                //{
                //    targetVelocityX = Mathf.MoveTowards(Body.VelocityX, horizontalVelocity, moveSpeed * airControl * Time.deltaTime);
                //}
                //else if (!IsGrounded)
                //{
                //    targetVelocityX = Body.VelocityX;
                //}
                
                Body.RigidBody2D.velocity = new Vector2(targetVelocityX, Body.VelocityY);
            }
            else if (couldMove && !inputBlocked) //no input
            {
                if (IsGrounded)
                {
                    Body.RigidBody2D.velocity = new Vector2(0, Body.VelocityY);
                }
                else
                {
                    Body.RigidBody2D.velocity = new Vector2(Mathf.MoveTowards(Body.VelocityX, 0, moveSpeed * airControl * Time.deltaTime), Body.VelocityY);
                }

            }
            else if(!Body.IsGrounded && inputBlocked && Body.RigidBody2D.velocity.y < 0)
            {
                DoWallCling();
            }
            else if(Time.time >= wallClingLingerEndTime)
            {
                EndWallCling();
            }

            //the sprites face right byd efault
            //when flipx is true, the character will face the left
            //flip sprite accordingly
            if (horizontalInput > .01f)
            {
                //input is directing them to the right.
                //wall clinging means the sprite should be flipped to face left
                //not wallclinging means the sprite should not be flipped to face right
                Body.SpriteRenderer.flipX = IsWallClinging;
            }
            else if (horizontalInput < -.01f)
            {
                Body.SpriteRenderer.flipX = !IsWallClinging;
            }


            bool InputBlocked(Vector3 direction)
            {
                bool inputIsBlocked = false;
                if (direction.x > 0 && Body.DistFromRight <= distanceThreshold)
                {
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

        private void DoWallCling()
        {
            this.wallClingLingerEndTime = Time.time + this.wallClingLinger;

            if (IsWallClinging || !wallClingAvailable) return;
            wallClingAvailable = false;
            wallClingSFX?.Play();
            wallJumpDirection = MovementInputSource.MovementInputData.direction.x > distanceThreshold ? new Vector2(-1f, .5f) : new Vector2(1f, .5f);
            Body.RigidBody2D.constraints = RigidbodyConstraints2D.FreezeAll;
            IsWallClinging = true;
            IsJumping = false;
            Body.Animator.SetBool("IsWallClinging", true);
        }

        public void ChangeInput(IProvidePlatformMovementInput newInputSource)
        {
            if(defaultMovementInput == null) defaultMovementInput = this.MovementInputSource;
            this.MovementInputSource = newInputSource;
            Debug.Log(gameObject.name+" movement input source changed to "+ MovementInputSource);
        }

        public void ResetInput()
        {
            this.MovementInputSource = defaultMovementInput;
        }
    }
}