using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [SerializeField]
    protected float movementSpeed = 5f;

    [SerializeField]
    protected CharacterController controller;

    [SerializeField]
    protected Camera camera;

    [SerializeField]
    protected ParticleSystem muzzleFlash;

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

        switchCoroutine = StartCoroutine(SwitchWeapon(0));

        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    private void Update()
    {
        ComputeInputs();
    }

    private void LateUpdate()
    {
        CameraMovement();
    }

    private void CameraMovement()
    {
        if (!camera) return;

        float _axisX = Input.GetAxisRaw("Mouse X") * MouseSensitivity;
        float _axisY = Input.GetAxisRaw("Mouse Y") * MouseSensitivity;

        transform.localRotation = Quaternion.Euler(new Vector3(0f, transform.localEulerAngles.y + _axisX, 0f));
        camera.transform.localRotation = Quaternion.Euler(new Vector3(camera.transform.localEulerAngles.x - _axisY, 0f, 0f));
    }

    private void ComputeInputs()
    {
        MoveInputs();

        if (Input.GetMouseButton(0))
        {
            currentWeapon.Fire();
            muzzleFlash.Play();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            currentWeapon.Reload();
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            currentWeapon.Inspect();
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            if (switchCoroutine != null)
            {
                StopCoroutine(switchCoroutine);

                switchCoroutine = null;
            }

            switchCoroutine = StartCoroutine(SwitchWeapon(currentWeaponIndex + 1));
        }
    }

    private float cachedX = 0f, cachedZ = 0f;

    private float fallingTimer = 0f;

    private Vector3 MoveInputs()
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

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded && !isJumping)
            {
                cachedJumpTimer = jumpTimer;
                isJumping = true;
            }
        }

        _movementDir.y += isJumping ? jumpSpeed * (Mathf.Clamp(jumpTimer -= Time.deltaTime, 0f, cachedJumpTimer)) : 0f;

        controller.Move(transform.TransformDirection(_movementDir) * movementSpeed * Time.deltaTime);

        //controller.Move(new Vector3(0f, , 0f) * Time.deltaTime);

        return _movementDir;
    }

    private IEnumerator SwitchWeapon(int index)
    {
        if (weapons.Count == 0) yield break;

        if (index >= weapons.Count)
        {
            index = 0;
        }

        if (currentWeapon)
        {
            if (currentWeapon == weapons[index]) yield break;

            currentWeapon.SwitchOut();

            yield return new WaitUntil(() => !currentWeapon.gameObject.activeSelf);
        }

        currentWeaponIndex = index;

        currentWeapon = weapons[currentWeaponIndex];

        currentWeapon.SwitchIn();
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

    private void OnDestroy()
    {
        Cursor.lockState = CursorLockMode.None;
    }
}
