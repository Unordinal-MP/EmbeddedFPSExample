#pragma warning disable //TODO: uncomment when file is done

using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerLogic : MonoBehaviour
{
    private float cachedX;
    private float cachedZ;
    private bool isJumping;


    //TODO: enter better way of input settings (ScriptableObject?)
    private const float walkSpeed = 8;
    private const float gravityConstant = 2;
    private const float jumpStrength = 11;
    private const float jumpSpeed = 6;
    private const int groundLayerMask = 1;
    private const float groundHeight = 0.51f;
    private const float gravityMultiplier = 8;

    const float forwardSpeed = 1;
    const float backSpeed = 0.95f * forwardSpeed;
    const float strafeSpeed = 0.7f * forwardSpeed;
    const float fullFowardSpeed = 9;

    private bool isGrounded = true;

    [SerializeField]
    private float jumpHeight = 5f;

    private Vector3 lastVelocity;

    public CharacterController CharacterController => controller;

    private CharacterController controller;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    public PlayerStateData GetNextFrameData(PlayerInputData input, PlayerStateData currentStateData)
    {
        float dt = Constants.TickInterval;

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

        Vector3 inputWorld = Vector3.zero;
        if (input.HasAction(PlayerAction.Forward))
        {
            inputWorld.z += forwardSpeed;
        }

        if (input.HasAction(PlayerAction.Back))
        {
            inputWorld.z -= backSpeed;
        }

        if (input.HasAction(PlayerAction.Left))
        {
            inputWorld.x -= strafeSpeed;
        }

        if (input.HasAction(PlayerAction.Right))
        {
            inputWorld.x += strafeSpeed;
        }

        if (input.HasAction(PlayerAction.Forward) && input.HasAction(PlayerAction.Back))
        {
            inputWorld.z = 0;
        }

        if (input.HasAction(PlayerAction.Left) && input.HasAction(PlayerAction.Right))
        {
            inputWorld.x = 0;
        }

        //inputWorld *= fullFowardSpeed;
        inputWorld = transform.TransformDirection(inputWorld);
        inputWorld.y = 0;

        var _movementDir = inputWorld;
        float _yChange = 0;

        if (isGrounded)
        {
            if (input.HasAction(PlayerAction.Jump))
            {
                // Ensure player doesn't jump to high.

                var jumpSpeed = Mathf.Sqrt(2f * jumpHeight * -Physics.gravity.y);
                jumpSpeed = Mathf.Clamp(jumpSpeed, float.MinValue, 14.0f);
                _yChange = jumpSpeed;
            }
            else
            {
                // Walking parallel along the ground doesn't update CharacterController.IsGrounded correctly.
                // So add downward velocity.

                _yChange = -1.0f;
            }
        }
        else
        {
            // Apply gravity when in air.

            var gravityToApply = Physics.gravity.y * dt;
            _yChange += gravityToApply;
        }

        if (!isGrounded)
        {
            // Partial control when in air.

            var horizontalLastVelocity = Vector3.ProjectOnPlane(lastVelocity, Vector3.up);
            var horizontalInputWorldVelocity = Vector3.ProjectOnPlane(_movementDir, Vector3.up);
            _movementDir = horizontalLastVelocity + horizontalInputWorldVelocity * dt;
        }

        _movementDir.y = _yChange;

        //TODO: decompose fullFowardSpeed application (see inserted comment earlier) so we don't scale EVERYTHING by this value
        CollisionFlags flags = controller.Move(dt * fullFowardSpeed * _movementDir);

        lastVelocity = _movementDir;
        isGrounded = controller.isGrounded;

        return new PlayerStateData(currentStateData.PlayerId, input, transform.position, transform.rotation, flags);
    }
}