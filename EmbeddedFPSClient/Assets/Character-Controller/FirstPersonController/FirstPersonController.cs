using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [SerializeField]
    public float movementSpeed = 5f;

    [SerializeField]
    protected CharacterController controller;

    [SerializeField]
    public new Camera camera;

    [SerializeField]
    protected float clampCamAngle = 70f;

    [SerializeField]
    public float jumpSpeed = 4f;

    [SerializeField]
    public LayerMask groundLayer;

    [SerializeField]

    public float GroundHieght = 0.6f;

    [SerializeField]
    public float gravityMultiplier = 8f;

    public float MouseSensitivity { get; set; } = 1;

    private WeaponController weaponController;

    // Start is called before the first frame updateok
    private void Start()
    {
        controller = GetComponent<CharacterController>();
        weaponController = GetComponent<WeaponController>();
    }

    private void OnEnable()
    {
        LockCursor();
    }

    private void OnDisable()
    {
        UnlockCursor();
    }

    private void LockCursor()
    {
        if (IsCursorLocked())
            return;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void UnlockCursor()
    {
        if (!IsCursorLocked())
            return;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private bool IsCursorLocked()
    {
        return Cursor.lockState != CursorLockMode.None;
    }

    public PlayerInputData GetInputs(uint time)
    {
        ComputeInputsAndRotations(out bool[] _inputs, out Quaternion _lookRotation);

        return new PlayerInputData(_inputs, _lookRotation, time);
    }

    public void OnServerDataUpdate(PlayerStateData playerStateData, bool isOwn)
    {
        if (isOwn) return;
    }

    public void Update()
    {
        CameraMovement();
    }

    void CameraMovement()
    {
        float _axisX = Input.GetAxisRaw("Mouse X") * MouseSensitivity;
        float _axisY = Input.GetAxisRaw("Mouse Y") * MouseSensitivity;

        float newYaw = camera.transform.localEulerAngles.y + _axisX;
        float newPitch = camera.transform.localEulerAngles.x - _axisY;

        if (newPitch > 180)
            newPitch -= 360;
        newPitch = Mathf.Clamp(newPitch, -clampCamAngle, clampCamAngle);

        camera.transform.localRotation = Quaternion.Euler(new Vector3(newPitch, newYaw, 0f));
    }

    private void ComputeInputsAndRotations(out bool[] outInputs, out Quaternion rotation)
    {
        var inputs = new bool[(int)PlayerAction.NumActions];
        outInputs = inputs;

        inputs[(int)PlayerAction.Jump] = Input.GetKeyDown(KeyCode.Space);
        inputs[(int)PlayerAction.Sprint] = Input.GetKeyDown(KeyCode.LeftShift);
        inputs[(int)PlayerAction.Fire] = Input.GetMouseButton(0);
        inputs[(int)PlayerAction.Grounded] = true; //TODO: must compute locally until we have server auth map loading
        inputs[(int)PlayerAction.Aim] = Input.GetMouseButton(1);
        inputs[(int)PlayerAction.Reload] = Input.GetKeyDown(KeyCode.R);
        inputs[(int)PlayerAction.Inspect] = Input.GetKeyDown(KeyCode.I);
        inputs[(int)PlayerAction.SwitchWeapon] = Input.GetKeyDown(KeyCode.V);
        inputs[(int)PlayerAction.Forward] = Input.GetKey(KeyCode.W);
        inputs[(int)PlayerAction.Left] = Input.GetKey(KeyCode.A);
        inputs[(int)PlayerAction.Right] = Input.GetKey(KeyCode.S);
        inputs[(int)PlayerAction.Back] = Input.GetKey(KeyCode.D);

        bool HasAction(PlayerAction which)
        {
            return inputs[(int)which];
        }

        if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2))
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                LockCursor();
            }
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsCursorLocked())
            {
                UnlockCursor(); 
            }
            else
            {
                Application.Quit();
            }
        }

        //TODO: move synchronizing actions to PlayerLogic
        if (IsCursorLocked())
        {
            if (HasAction(PlayerAction.Fire))
            {
                weaponController.Fire();
            }

            if (HasAction(PlayerAction.Aim))
            {

            }
        }

        if (HasAction(PlayerAction.Sprint))
        {

        }

        if (HasAction(PlayerAction.Reload))
        {
            weaponController.Reload();
        }

        if (HasAction(PlayerAction.Inspect))
        {
            weaponController.Inspect();
        }

        if (HasAction(PlayerAction.SwitchWeapon))
        {
            weaponController.SwitchWeapon();
        }

        if (camera)
        {
            rotation = camera.transform.rotation;//new Vector3(camera.transform.localEulerAngles.x, transform.localEulerAngles.y, 0f);
        }
        else
        {
            rotation = Quaternion.identity;
        }
    }
}
