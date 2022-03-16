using System.Collections.Generic;
using System.Linq;
using DarkRift;
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public struct ReconciliationInfo
{
    public ReconciliationInfo(uint frame, PlayerStateData data, PlayerInputData input)
    {
        Frame = frame;
        Data = data;
        Input = input;
    }

    public uint Frame;
    public PlayerStateData Data;
    public PlayerInputData Input;
}

[RequireComponent(typeof(PlayerLogic))]
[RequireComponent(typeof(PlayerInterpolation))]
public class ClientPlayer : ThirdPersonController
{
    [SerializeField]
    float positionError = 5;

    protected PlayerLogic playerLogic;

    protected PlayerInterpolation interpolation;

    protected Queue<ReconciliationInfo> reconciliationHistory = new Queue<ReconciliationInfo>();

    // Store look direction.
    protected float yaw;
    protected float pitch;

    protected ushort id;
    protected string playerName;

    public bool isOwn { get; private set; }

    protected float health;

    [Header("HealthBar")]
    [SerializeField]
    protected Text nameText;
    [SerializeField]
    protected Image healthBarFill;
    [SerializeField]
    protected GameObject healthBarObject;

    [Header("Prefabs")]
    [SerializeField]
    protected GameObject shotPrefab;

    [SerializeField]
    protected float shootForce;

    [SerializeField]
    protected Transform bulletSpawnPoint;

    protected PlayerHealth playerHealth { get; private set; }

    CharacterAnimator characterAnimator;

    private Transform followTarget;

    [SerializeField]
    protected Transform followCamTarget;

    [SerializeField]
    protected Transform lookAtCanTarget;

    protected override void Awake()
    {
        playerLogic = GetComponent<PlayerLogic>();
        interpolation = GetComponent<PlayerInterpolation>();
        playerHealth = GetComponent<PlayerHealth>();
        playerLogic.ConfigureSettings(moveSpeed, jumpHeight);
        characterAnimator = GetComponent<CharacterAnimator>();
    }

    protected override void OnEnable()
    {
        //Nope
    }

    protected override void Start()
    {
        base.Start();
        hasAnimator = TryGetComponent(out animator);
        controller = GetComponent<CharacterController>();
        AssignAnimationIDs();
    }

    public void Initialize(ushort id, string playerName)
    {
        this.id = id;
        this.playerName = playerName;
        //nameText.text = this.playerName;
        SetHealth(playerHealth.maxHealth);
        input = GetComponent<StarterAssetsInputs>();

        if (ConnectionManager.Instance.PlayerId == id)
        {
            isOwn = true;

            //characterAnimator.cam.enabled = isOwn;
            //characterAnimator.enabled = isOwn;

            base.OnEnable();

            interpolation.CurrentData = new PlayerStateData(this.id, 0, Vector3.zero, Quaternion.identity.eulerAngles);
        }
        else
        {
            Destroy(input);

            followTarget = new GameObject("FollowTransform", new System.Type[] { typeof(Camera) }).transform;

            characterAnimator.cam = followTarget.GetComponent<Camera>();
            characterAnimator.cam.enabled = false;

            followTarget.position = followCamTarget.position;
            followTarget.LookAt(lookAtCanTarget);
        }

        cinemachineCameraTarget.SetActive(isOwn);
    }

    public void OnServerDataUpdate(PlayerStateData playerStateData)
    {
       
        if (isOwn)
        {
            while (reconciliationHistory.Any() && reconciliationHistory.Peek().Frame < GameManager.Instance.LastReceivedServerTick)
            {
                reconciliationHistory.Dequeue();
            }
            if (reconciliationHistory.Any() && reconciliationHistory.Peek().Frame == GameManager.Instance.LastReceivedServerTick)
            {
                ReconciliationInfo info = reconciliationHistory.Dequeue();
                if (Vector3.Distance(info.Data.Position, playerStateData.Position) > 0.05f)
                {

                    List<ReconciliationInfo> infos = reconciliationHistory.ToList();
                    interpolation.CurrentData = playerStateData;
                    //transform.position = playerStateData.Position;
                    for (int i = 0; i < infos.Count; i++)
                    {
                        PlayerStateData u = playerLogic.GetNextFrameData(infos[i].Input, interpolation.CurrentData);
                        interpolation.SetFramePosition(u);
                    }
                }
            }

            if (reconciliationHistory.Count > 20)
            {
                reconciliationHistory.Dequeue();
            }
        }
        else
        {
            GroundedCheck();

            AdjustHeadTransform(playerStateData.LookDirection);

            ApplyFrameAnimations(playerStateData); //but still why is it not working for 2nd one as its not (isowneD) fot that user

            interpolation.SetFramePosition(playerStateData);

            followTarget.position = transform.position;
        }
    }

    private void ApplyFrameAnimations(PlayerStateData playerStateData)
    {
        if (hasAnimator)
        {
            Vector3 _movement = (playerStateData.Position - transform.position).normalized;

            animator.SetFloat(animIDSpeedHorizontal, _movement.x * speedModifier, Time.deltaTime, Time.deltaTime);
            animator.SetFloat(animIDSpeedVertical, _movement.y * speedModifier, Time.deltaTime, Time.deltaTime);
            animator.SetFloat(animIDMotionSpeed, new Vector2(_movement.y, _movement.x).magnitude);
        }
    }

    private void AdjustHeadTransform(Vector3 lookDir)
    {
        // if there is an input and camera position is not fixed
        if (lookDir.sqrMagnitude >= threshold && !lockCameraPosition)
        {
            cinemachineTargetYaw += lookDir.x * Time.deltaTime;
            cinemachineTargetPitch += lookDir.y * Time.deltaTime;
        }

        // clamp our rotations so our values are limited 360 degrees
        cinemachineTargetYaw = ClampAngle(cinemachineTargetYaw, float.MinValue, float.MaxValue);
        cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, bottomClamp, topClamp);

        // Cinemachine will follow this target
        cinemachineCameraTarget.transform.rotation = Quaternion.Euler(cinemachineTargetPitch + cameraAngleOverride, cinemachineTargetYaw, 0.0f);
    }

    public virtual void SetHealth(float value)
    {
        health = value;
        //healthBarFill.fillAmount = value / 100f;
    }

    protected override void Update()
    {
        if (!isOwn)
        {
            return;
        }

        base.Update();
    }

    protected override void LateUpdate()
    {
        if (!isOwn) return;
        base.LateUpdate();
        PlayerUpdate();
    }

    void PlayerUpdate()
    {
        float[] moveInputs = new float[2];
        moveInputs[0] = input.move.x;
        moveInputs[1] = input.move.y;

        bool[] inputs = new bool[4];
        inputs[0] = input.jump;
        inputs[1] = input.sprint;
        inputs[2] = input.shoot;
        inputs[3] = grounded;

        if (inputs[2])
        {
            ShootBullet();
        }

        Vector3 rotation = new Vector3(cinemachineCameraTarget.transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);

        PlayerInputData inputData = new PlayerInputData(moveInputs, inputs, rotation, GameManager.Instance.LastReceivedServerTick - 1);
        
        //check position here
        Vector3 newPosition = transform.position + transform.forward * moveInputs[1] + transform.right * moveInputs[0];

        //transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime);

        PlayerStateData nextStateData = playerLogic.GetNextFrameData(inputData, interpolation.CurrentData);
        interpolation.SetFramePosition(nextStateData);

        using (Message message = Message.Create((ushort)Tags.GamePlayerInput, inputData))
        {
            ConnectionManager.Instance.Client.SendMessage(message, SendMode.Reliable);
        }
        if (reconciliationHistory.Count < 20)
            reconciliationHistory.Enqueue(new ReconciliationInfo(GameManager.Instance.ClientTick, nextStateData, inputData));
        else
        {
            reconciliationHistory.Dequeue();
            reconciliationHistory.Enqueue(new ReconciliationInfo(GameManager.Instance.ClientTick, nextStateData, inputData));
        }
    }

    public void ShootBullet()
    {
        //GameObject go = Instantiate(shotPrefab);
        //go.transform.position = interpolation.CurrentData.Position;
        //go.transform.rotation = transform.rotation;
        //Destroy(go, 1f);

        GameObject go = Instantiate(shotPrefab, bulletSpawnPoint.position, Quaternion.identity);
        go.transform.forward = bulletSpawnPoint.forward;
        go.GetComponent<Rigidbody>().AddForce(bulletSpawnPoint.forward * shootForce);
        Destroy(go, 20);
    }
}