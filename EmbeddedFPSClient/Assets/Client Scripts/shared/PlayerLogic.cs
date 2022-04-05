using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerLogic : MonoBehaviour
{
    private Vector3 gravity;
    
    private float cachedX;
    private float cachedZ;
    private float fallingTimer;
    private bool isJumping;
    private float jumpTimer;
    private float cachedJumpTimer;

    //TODO: enter better way of input settings (ScriptableObject?)
    private const float walkSpeed = 8;
    private const float gravityConstant = 2;
    private const float jumpStrength = 11;
    private const float movementSpeed = 10;
    private const float jumpSpeed = 6;
    private const int groundLayerMask = 1;
    private const float groundHeight = 0.51f;
    private const float gravityMultiplier = 8;

    //private bool isGrounded { get; set; }

    public CharacterController CharacterController => controller;

    private CharacterController controller;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    public PlayerStateData GetNextFrameData(PlayerInputData input, PlayerStateData currentStateData)
    {
        bool HasAction(PlayerAction which)
        {
            return input.Keyinputs[(int)which];
        }

        float dt = Time.fixedDeltaTime;

        //bool isGrounded = HasAction(PlayerAction.Grounded);

        bool isGrounded = true;

        Vector3 euler = input.LookDirection.eulerAngles;
        Quaternion newRotation = Quaternion.Euler(0, euler.y, 0);

        FirstPersonController fpController = GetComponent<FirstPersonController>();

        if (fpController)
        {
            Quaternion oldHeadRotation = fpController.camera.transform.rotation;
            transform.rotation = newRotation;
            fpController.camera.transform.rotation = oldHeadRotation;
        }
        else
        {
            transform.rotation = newRotation;
        }

        gravity = new Vector3(0, currentStateData.Gravity, 0);

        /*Vector3 movement = Vector3.zero;

        if (HasAction(PlayerAction.Forward))
        {
            movement += Vector3.forward;
        }
        if (HasAction(PlayerAction.Left))
        {
            movement += Vector3.left;
        }
        if (HasAction(PlayerAction.Back))
        {
            movement += Vector3.back;
        }
        if (HasAction(PlayerAction.Right))
        {
            movement += Vector3.right;
        }

        movement = Quaternion.Euler(0, rotation.y, 0) * movement; // Move towards the look direction.
        movement.Normalize();
        movement = movement * walkSpeed;

        movement = movement * dt;
        movement = movement + gravity * dt;

        // The following code fixes character controller issues from unity. It makes sure that the controller stays connected to the ground by adding a little bit of down movement.
        CharacterController.Move(new Vector3(0, -0.001f, 0));

        //bool isGrounded = CharacterController.isGrounded;

        if (isGrounded)
        {
            if (HasAction(PlayerAction.Jump))
            {
                gravity = new Vector3(0, jumpStrength, 0);
            }
        }
        else
        {
            gravity -= new Vector3(0, gravityConstant, 0);
        }

        CharacterController.Move(movement);*/

        //GroundCheck();

        Vector3 _movementDir = Vector3.zero;

        if (HasAction(PlayerAction.Forward))
        {
            _movementDir.z += 1f;
        }

        if (HasAction(PlayerAction.Left))
        {
            _movementDir.x += -1f;
        }

        if (HasAction(PlayerAction.Right))
        {
            _movementDir.z += -1f;
        }

        if (HasAction(PlayerAction.Back))
        {
            _movementDir.x += 1f;
        }

        if (isGrounded)
        {
            fallingTimer = 0f;

            cachedX = _movementDir.x;
            cachedZ = _movementDir.z;
        }
        else
        {
            fallingTimer += dt * gravityMultiplier;

            _movementDir.x = cachedX;
            _movementDir.z = cachedZ;
        }
        
        _movementDir.y = Physics.gravity.y * dt * fallingTimer;

        if (isJumping && isGrounded)
        {
            isJumping = false;
            jumpTimer = cachedJumpTimer;
        }

        if (HasAction(PlayerAction.Jump))
        {
            if (isGrounded && !isJumping)
            {
                cachedJumpTimer = jumpTimer;
                isJumping = true;
            }
        }

        if (isJumping)
        {
            _movementDir.y += jumpSpeed * Mathf.Clamp(jumpTimer, 0f, cachedJumpTimer);
            jumpTimer -= dt;
        }
        
        CollisionFlags flags = controller.Move(dt * movementSpeed * transform.TransformDirection(_movementDir));

        return new PlayerStateData(currentStateData.PlayerId, input, gravity.y, transform.position, transform.rotation, flags);
    }

    public bool IsGroundedCheck()
    {
        return true; //TODO: remove
        
        if (Physics.Linecast(controller.bounds.center, controller.bounds.center + (-Vector3.up * (controller.height * groundHeight)), out RaycastHit _, groundLayerMask))
        {
            return true;
        }

        return false;
    }
}