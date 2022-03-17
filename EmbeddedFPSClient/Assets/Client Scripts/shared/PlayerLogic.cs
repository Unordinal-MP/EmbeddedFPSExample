using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerLogic : MonoBehaviour
{
    private Vector3 gravity;

    [Header("Settings")]
    [SerializeField]
    private float walkSpeed;
    [SerializeField]
    private float gravityConstant;
    [SerializeField]
    private float jumpStrength;

    //ClientPlayer clientPlayer;
    //ServerPlayer serverPlayer;

    public CharacterController CharacterController { get; private set; }

    void Awake()
    {
        CharacterController = GetComponent<CharacterController>();
        //if (serverSidePlayer)
        //    serverPlayer = GetComponent<ServerPlayer>();
        //else
        //    clientPlayer = GetComponent<ClientPlayer>();
    }

    public void ConfigureSettings(float walkSpeed, float jumpStrength, float gravityConstant = 9.81f)
    {
        this.walkSpeed = walkSpeed;
        this.jumpStrength = jumpStrength;
        this.gravityConstant = gravityConstant;
    }

    public PlayerStateData GetNextFrameData(PlayerInputData input, PlayerStateData currentStateData)
    {
        float h = input.MovementInputs[0];
        float v = input.MovementInputs[1];

        bool jump = input.Keyinputs[0];
        bool sprint = input.Keyinputs[1];
        bool shoot = input.Keyinputs[2];

        Vector3 rotation = transform.rotation.eulerAngles;
        gravity.y = currentStateData.Gravity;

        Vector3 movement = new Vector3(h, 0, v);

        //if (serverSidePlayer)
        //{
        //    serverPlayer.grounded = input.Keyinputs[3];
        //    speed = sprint ? serverPlayer.sprintSpeed : serverPlayer.moveSpeed;
        //}
        //else
        //{
        //    clientPlayer.grounded = input.Keyinputs[3];
        //    speed = sprint ? clientPlayer.sprintSpeed : clientPlayer.moveSpeed;
        //}
        /*if (CharacterController.isGrounded && jump)
        {
            clientPlayer.Jump();//gravity = new Vector3(0, jumpStrength, 0);
        }*/

        movement.Normalize();
        movement = transform.TransformDirection(movement);

        movement.y += gravity.y * Time.deltaTime;

        // The following code fixes character controller issues from unity. It makes sure that the controller stays connected to the ground by adding a little bit of down movement.
        CharacterController.Move(movement * Time.fixedDeltaTime);

        return new PlayerStateData(currentStateData.Id, gravity.y, transform.position, input.LookDirection);
    }
}