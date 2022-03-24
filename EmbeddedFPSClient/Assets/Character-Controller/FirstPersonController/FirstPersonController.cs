using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour, IPlayerLogic, IStreamData
{
    [SerializeField]
    protected float movementSpeed = 5f;

    [SerializeField]
    protected CharacterController controller;

    [SerializeField]
    protected Camera camera;

    [SerializeField]
    protected float clampCamAngle = 70f;

    [SerializeField]
    protected float jumpSpeed = 4f;

    [SerializeField]
    private bool isGrounded = false;

    [SerializeField]
    private LayerMask groundLayer;

    [SerializeField]
    private float jumpTimer = 0f;

    [SerializeField]

    private float GroundHieght = 0.6f;

    [SerializeField]
    private float gravityMultiplier = 8f;

    private float cachedJumpTimer = 0f;

    public float MouseSensitivity = 1;

    private bool isJumping = false;

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

    public PlayerStateData GetNextFrameData(PlayerStateData currentStateData, uint time)
    {
        ComputeInputsAndRotations(out float[] _movement, out bool[] _inputs, out Vector3 _lookRotation);

        return new PlayerStateData(currentStateData.Id, Physics.gravity.y, transform.position, _lookRotation, _movement, _inputs, time);
    }

    public void OnServerDataUpdate(PlayerStateData playerStateData, bool isOwn)
    {
        if (isOwn) return;
    }

    private Vector3 CameraMovement()
    {
        if (!camera) return Vector3.zero;

        float _axisX = Input.GetAxisRaw("Mouse X") * MouseSensitivity;
        float _axisY = Input.GetAxisRaw("Mouse Y") * MouseSensitivity;

        transform.localRotation = Quaternion.Euler(new Vector3(0f, transform.localEulerAngles.y + _axisX, 0f));
        camera.transform.localRotation = Quaternion.Euler(new Vector3(camera.transform.localEulerAngles.x - _axisY, 0f, 0f));

        return new Vector3(camera.transform.localEulerAngles.x, transform.localEulerAngles.y, 0f);
    }

    private void ComputeInputsAndRotations(out float[] movement, out bool[] inputs, out Vector3 rotation)
    {
        inputs = new bool[8];

        inputs[3] = isGrounded;

        MoveInputs(out movement, out inputs[0]);

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

        if (IsCursorLocked())
        {
            if (inputs[2] = Input.GetMouseButton(0))
            {
                weaponController.Fire();
            }

            if (inputs[4] = Input.GetMouseButton(1))
            {

            }
        }

        if (inputs[1] = Input.GetKeyDown(KeyCode.LeftShift))
        {

        }

        if (inputs[5] = Input.GetKeyDown(KeyCode.R))
        {
            weaponController.Reload();
        }

        if (inputs[6] = Input.GetKeyDown(KeyCode.I))
        {
            weaponController.Inspect();
        }

        if (inputs[7] = Input.GetKeyDown(KeyCode.V))
        {
            weaponController.SwitchWeapon();
        }

        rotation = CameraMovement();
    }

    private float cachedX = 0f, cachedZ = 0f;

    private float fallingTimer = 0f;

    private void MoveInputs(out float[] movement, out bool jump)
    {
        GroundCheck();

        Vector3 _movementDir = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
        {
            _movementDir.z += 1f;
        }

        if (Input.GetKey(KeyCode.A))
        {
            _movementDir.x += -1f;
        }

        if (Input.GetKey(KeyCode.S))
        {
            _movementDir.z += -1f;
        }

        if (Input.GetKey(KeyCode.D))
        {
            _movementDir.x += 1f;
        }

        if (!isGrounded)
        {
            fallingTimer += Time.deltaTime * gravityMultiplier;

            _movementDir.x = cachedX;
            _movementDir.z = cachedZ;
        }
        else
        {
            fallingTimer = 0f;

            cachedX = _movementDir.x;
            cachedZ = _movementDir.z;
        }

        _movementDir.y = Physics.gravity.y * Time.deltaTime * fallingTimer;

        if (isJumping && isGrounded)
        {
            isJumping = false;
            jumpTimer = cachedJumpTimer;
        }

        if (jump = Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded && !isJumping)
            {
                cachedJumpTimer = jumpTimer;
                isJumping = true;
            }
        }

        _movementDir.y += isJumping ? jumpSpeed * (Mathf.Clamp(jumpTimer -= Time.deltaTime, 0f, cachedJumpTimer)) : 0f;

        controller.Move(transform.TransformDirection(_movementDir) * movementSpeed * Time.deltaTime);

        movement = new float[] { _movementDir.x, _movementDir.z };
    }

    private void GroundCheck()
    {
        if (Physics.Linecast(controller.bounds.center, controller.bounds.center + (-Vector3.up * (controller.height * GroundHieght)), out RaycastHit _hit, groundLayer))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }
}
