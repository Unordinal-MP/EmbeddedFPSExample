using UnityEngine;

public interface IPlayerLogic
{
    PlayerStateData GetNextFrameData(PlayerStateData currentStateData, uint time);
}

[RequireComponent(typeof(CharacterController))]
public class PlayerLogic : MonoBehaviour, IPlayerLogic
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

    public PlayerStateData GetNextFrameData(PlayerStateData currentStateData, uint time)
    {
        float h = currentStateData.horizontal;
        float v = currentStateData.vertical;

        bool jump = currentStateData.isJumping;
        bool sprint = currentStateData.isSprinting;
        bool shoot = currentStateData.isShooting;

        transform.rotation = Quaternion.Euler(new Vector3(0f, currentStateData.LookDirection.y, 0f));
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

        //Parsing over the data from what was received instead of applying the movement from the server player is temporary, since the server player isn't finished yet
        return new PlayerStateData(currentStateData.Id, gravity.y, currentStateData.Position, currentStateData.LookDirection, currentStateData.MovementInputs, currentStateData.Keyinputs, time);
    }
}