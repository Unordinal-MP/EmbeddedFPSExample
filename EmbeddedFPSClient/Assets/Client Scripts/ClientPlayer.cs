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

public interface IStreamData
{
    void OnServerDataUpdate(PlayerStateData playerStateData);
}

[RequireComponent(typeof(PlayerLogic))]
[RequireComponent(typeof(PlayerInterpolation))]
public class ClientPlayer : MonoBehaviour, IStreamData
{
    [SerializeField]
    float positionError = 5;

    [SerializeField]
    private Camera playerCamera;

    protected PlayerLogic playerLogic;

    protected PlayerInterpolation interpolation;

    protected Queue<ReconciliationInfo> reconciliationHistory = new Queue<ReconciliationInfo>();

    public ushort id { get; private set; }

    public string playerName { get; private set; }

    public bool isOwn { get; private set; }

    protected float health;

    protected PlayerHealth playerHealth { get; private set; }

    [SerializeField]
    private List<IStreamData> streamDatas = new List<IStreamData>();

    protected void Awake()
    {
        // Will have to change this
        playerLogic = GetComponent<PlayerLogic>();
        interpolation = GetComponent<PlayerInterpolation>();
        playerHealth = GetComponent<PlayerHealth>();

        var _streamDatas = GetComponents<IStreamData>();

        foreach (var _streamData in _streamDatas)
        {
            if ((object)_streamData == this)
                continue;

            streamDatas.Add(_streamData);
        }
    }

    private void Update()
    {
        PlayerUpdate();
    }

    public void Initialize(ushort id, string playerName)
    {
        this.id = id;
        this.playerName = playerName;

        //nameText.text = this.playerName;
        //SetHealth(playerHealth.maxHealth);

        if (ConnectionManager.Instance.PlayerId == id)
        {
            isOwn = true;

            playerCamera.gameObject.SetActive(true);

            interpolation.CurrentData = new PlayerStateData(this.id, 0, Vector3.zero, Quaternion.identity.eulerAngles);
        }
        else
        {
            playerCamera.gameObject.SetActive(false);
        }
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
                        playerStateData = playerLogic.GetNextFrameData(infos[i].Input, interpolation.CurrentData);
                    }
                }
            }

            if (reconciliationHistory.Count > 20)
            {
                reconciliationHistory.Dequeue();
            }
        }

        foreach (var _streamData in streamDatas)
            _streamData.OnServerDataUpdate(playerStateData);
    }

    public virtual void SetHealth(float value)
    {
        health = value;
        //healthBarFill.fillAmount = value / 100f;
    }

    public void PlayerUpdate()
    {
        if (!isOwn) return;

        float[] moveInputs = new float[2];
        moveInputs[0] = Input.GetKey(KeyCode.A) ? -1f : Input.GetKey(KeyCode.D) ? 1f : 0f;
        moveInputs[1] = Input.GetKey(KeyCode.W) ? -1f : Input.GetKey(KeyCode.S) ? 1f : 0f;

        bool[] inputs = new bool[4];
        inputs[0] = Input.GetKeyDown(KeyCode.Space);
        inputs[1] = Input.GetKey(KeyCode.LeftShift);
        inputs[2] = Input.GetMouseButton(0);
        inputs[3] = true;

        Vector3 rotation = new Vector3(playerCamera.transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);

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

    //public void ShootBullet()
    //{
    //    GameObject go = Instantiate(shotPrefab, bulletSpawnPoint.position, Quaternion.identity);
    //    go.transform.forward = bulletSpawnPoint.forward;
    //    go.GetComponent<Rigidbody>().AddForce(bulletSpawnPoint.forward * shootForce);
    //    Destroy(go, 20);
    //}
}