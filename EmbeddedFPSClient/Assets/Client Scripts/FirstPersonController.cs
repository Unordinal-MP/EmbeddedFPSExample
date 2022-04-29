using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
#pragma warning disable SA1307 // Accessible fields should begin with upper-case letter
    public new Camera camera;
#pragma warning restore SA1307 // Accessible fields should begin with upper-case letter

    [SerializeField]
    private float clampCamAngle = 70f;
    
    private WeaponController weaponController;

    private bool cameraLocked;

    public float MouseSensitivity { get; set; } = 1;

    private bool[] instantaneousKeyInputs;
    private bool[] cumulativeKeyInputs;

    private void Start()
    {
        instantaneousKeyInputs = new bool[(int)PlayerAction.NumActions];
        cumulativeKeyInputs = new bool[(int)PlayerAction.NumActions];

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
        {
            return;
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void UnlockCursor()
    {
        if (!IsCursorLocked())
        {
            return;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private bool IsCursorLocked()
    {
        return Cursor.lockState != CursorLockMode.None;
    }

    public PlayerInputData GetInputs(uint time, uint sequenceNumber)
    {
        HandleInputs(out bool[] inputs); //TODO: factor out side effects and rename

        Quaternion lookRotation = Quaternion.identity;
        if (camera)
        {
            lookRotation = camera.transform.rotation; //new Vector3(camera.transform.localEulerAngles.x, transform.localEulerAngles.y, 0f);
        }

        return new PlayerInputData(inputs, lookRotation, time, sequenceNumber);
    }

    private void Update()
    {
        GetInputVector(instantaneousKeyInputs);

        for (int i = 0; i < instantaneousKeyInputs.Length; ++i)
        {
            cumulativeKeyInputs[i] |= instantaneousKeyInputs[i];
        }

        CameraMovement();
    }

    private void CameraMovement()
    {
        if (cameraLocked)
            return;

        float axisX = Input.GetAxisRaw("Mouse X") * MouseSensitivity;
        float axisY = Input.GetAxisRaw("Mouse Y") * MouseSensitivity;

        float newYaw = camera.transform.localEulerAngles.y + axisX;
        float newPitch = camera.transform.localEulerAngles.x - axisY;

        if (newPitch > 180)
        {
            newPitch -= 360;
        }

        newPitch = Mathf.Clamp(newPitch, -clampCamAngle, clampCamAngle);

        camera.transform.localRotation = Quaternion.Euler(new Vector3(newPitch, newYaw, 0f));
    }

    private static void GetInputVector(bool[] inputs)
    {
        inputs[(int)PlayerAction.Jump] = Input.GetKeyDown(KeyCode.Space);
        inputs[(int)PlayerAction.Sprint] = Input.GetKeyDown(KeyCode.LeftShift);
        inputs[(int)PlayerAction.Fire] = Input.GetMouseButton(0);
        inputs[(int)PlayerAction.Aim] = Input.GetMouseButton(1);
        inputs[(int)PlayerAction.Reload] = Input.GetKeyDown(KeyCode.R);
        inputs[(int)PlayerAction.Inspect] = Input.GetKeyDown(KeyCode.I);
        inputs[(int)PlayerAction.SwitchWeapon] = Input.GetKeyDown(KeyCode.V);
        inputs[(int)PlayerAction.Forward] = Input.GetKey(KeyCode.W);
        inputs[(int)PlayerAction.Left] = Input.GetKey(KeyCode.A);

        //at time of writing Unity has swapped the letters D and S for whatever reason
        //if your character is mixing backing and going right, swap D and S below
        //(or migrate to the input system of your choice)
        inputs[(int)PlayerAction.Right] = Input.GetKey(KeyCode.D);
        inputs[(int)PlayerAction.Back] = Input.GetKey(KeyCode.S);
    }

    private void HandleInputs(out bool[] outInputs)
    {
        outInputs = cumulativeKeyInputs;
        cumulativeKeyInputs = new bool[(int)PlayerAction.NumActions];

        var inputs = outInputs;
        bool HasAction(PlayerAction which)
        {
            return inputs[(int)which];
        }

        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(2))
        {
            LockCursor();
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

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.P))
        {
            //much easier to debug multiple editors
            cameraLocked = !cameraLocked;
        }
#endif

        //TODO: move synchronizing actions to PlayerLogic
        if (IsCursorLocked())
        {
            if (HasAction(PlayerAction.Fire))
            {
                weaponController.Fire();
            }

            if (HasAction(PlayerAction.Aim))
            {
                //TODO: implement
            }
        }

        if (HasAction(PlayerAction.Sprint))
        {
            //TODO: implement
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
    }
}
