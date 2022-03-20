using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    protected List<Weapon> weapons = new List<Weapon>();

    [SerializeField]
    private bool isGrounded = false;

    [SerializeField]
    private LayerMask groundLayer;

    [SerializeField]
    private float jumpTimer = 0f;

    [SerializeField]
    private float gravityMultiplier = 8f;

    [SerializeField]
    private Transform tpWeaponHolder;

    private float cachedJumpTimer = 0f;

    public float MouseSensitivity = 1;

    public Weapon currentWeapon { get; private set; }

    protected int currentWeaponIndex = 0;

    private Coroutine switchCoroutine;

    private bool isJumping = false;

    // Start is called before the first frame updateok
    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;

        switchCoroutine = StartCoroutine(SwitchWeapon(0));
    }

    // 
    //private void Update()
    //{
    //    ComputeInputs();
    //
    //    CameraMovement();
    //}

    public PlayerStateData GetNextFrameData(PlayerStateData currentStateData, uint time)
    {
        ComputeInputsAndRotations(out float[] _movement, out bool[] _inputs, out Vector3 _lookRotation);

        return new PlayerStateData(currentStateData.Id, Physics.gravity.y, transform.position, _lookRotation, _movement, _inputs, time);
    }

    public void OnServerDataUpdate(PlayerStateData playerStateData, bool isOwn)
    {
        if (isOwn) return;

        if (playerStateData.isSwitchingWeapon)
        {
            StartCoroutine(SwitchWeapon(currentWeaponIndex + 1));
        }
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

        if (inputs[1] = Input.GetKeyDown(KeyCode.LeftShift))
        {

        }

        if (inputs[2] = Input.GetMouseButton(0))
        {
            currentWeapon.Fire();
        }

        if (inputs[4] = Input.GetMouseButton(1))
        { 
        
        }

        if (inputs[5] = Input.GetKeyDown(KeyCode.R))
        {
            currentWeapon.Reload();
        }

        if (inputs[6] = Input.GetKeyDown(KeyCode.I))
        {
            currentWeapon.Inspect();
        }

        if (inputs[7] = Input.GetKeyDown(KeyCode.V))
        {
            if (switchCoroutine != null)
            {
                StopCoroutine(switchCoroutine);

                switchCoroutine = null;
            }

            switchCoroutine = StartCoroutine(SwitchWeapon(currentWeaponIndex + 1));
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

    private IEnumerator SwitchWeapon(int index)
    {
        if (weapons.Count == 0) yield break;

        if (index >= weapons.Count)
        {
            index = 0;
        }

        if (currentWeapon && currentWeapon.enabled)
        {
            if (currentWeapon == weapons[index]) yield break;

            currentWeapon.SwitchOut();

            yield return new WaitUntil(() => !currentWeapon.gameObject.activeSelf);
        }

        currentWeaponIndex = index;

        currentWeapon = weapons[currentWeaponIndex];

        if (currentWeapon.enabled)
        {
            currentWeapon.SwitchIn();
        }


    }

    private void GroundCheck()
    {
        if (Physics.Linecast(controller.bounds.center, controller.bounds.center + (-Vector3.up * (controller.height * 0.6f)), out RaycastHit _hit, groundLayer))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
    }
}
