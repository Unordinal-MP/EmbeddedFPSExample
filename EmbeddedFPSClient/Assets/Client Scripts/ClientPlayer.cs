using System.Collections.Generic;
using System.Linq;
using DarkRift;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

struct ReconciliationInfo
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
    void OnServerDataUpdate(PlayerStateData playerStateData, bool isOwn);
}

[RequireComponent(typeof(PlayerLogic))]
[RequireComponent(typeof(PlayerInterpolation))]
public class ClientPlayer : MonoBehaviour
{
    [SerializeField]
    private Camera playerCamera;

    [SerializeField]
    private UnityEvent localControllerInitalized = new UnityEvent();

    private PlayerLogic playerLogic;

    private PlayerInterpolation interpolation;

    private Queue<ReconciliationInfo> reconciliationHistory = new Queue<ReconciliationInfo>();
    public int ReconciliationHistorySize => reconciliationHistory.Count;

    // Store look direction.
    private float yaw;
    private float pitch;

    private ushort id;
    private string playerName;
    public bool isOwn { get; private set; }

    private int health;

    [Header("Prefabs")]
    [SerializeField]
    private GameObject shotPrefab;
    private List<IStreamData> streamDatas = new List<IStreamData>();

    void Awake()
    {
        playerLogic = GetComponent<PlayerLogic>();
        interpolation = GetComponent<PlayerInterpolation>();

        var _streamDatas = GetComponents<IStreamData>();

        foreach (var _streamData in _streamDatas)
        {
            streamDatas.Add(_streamData);
        }
    }

    public void Initialize(ushort id, string playerName)
    {
        this.id = id;
        this.playerName = playerName;
        SetHealth(100);
        if (ConnectionManager.Instance.PlayerId == id)
        {
            isOwn = true;
            /*Camera.main.transform.SetParent(transform);
            Camera.main.transform.localPosition = new Vector3(0,0,0);
            Camera.main.transform.localRotation = Quaternion.identity;*/
            interpolation.CurrentData = new PlayerStateData(this.id, new PlayerInputData(), 0, Vector3.zero, Quaternion.identity);
            localControllerInitalized.Invoke(); //TODO: convert to code

            ClientStats.instance.SetOwnPlayer(this);
        }

        interpolation.IsOwn = isOwn;
    }

    public void SetHealth(int value)
    {
        health = value;
    }

    void FixedUpdate()
    {
        if (isOwn)
        {
            PlayerInputData inputData = GetComponent<FirstPersonController>().GetInputs(GameManager.Instance.LastReceivedServerTick - 1, GameManager.Instance.ClientTick);

            if (inputData.isReloading) //TODO: find better place for this
            {
                GameObject go = Instantiate(shotPrefab);
                go.transform.position = interpolation.CurrentData.Position;
                go.transform.rotation = transform.rotation;
                Destroy(go, 1f);
            }

            transform.position = interpolation.CurrentData.Position;
            PlayerStateData nextStateData = playerLogic.GetNextFrameData(inputData, interpolation.CurrentData);
            interpolation.SetFramePosition(nextStateData);

            using (Message message = Message.Create((ushort) Tags.GamePlayerInput, inputData))
            {
                ConnectionManager.Instance.Client.SendMessage(message, SendMode.Unreliable);
            }

            reconciliationHistory.Enqueue(new ReconciliationInfo(GameManager.Instance.ClientTick, nextStateData, inputData));
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

                ClientStats.instance.Confirmations.AddNow();

                //Debug.Log("Rframe: " + info.Data.Position + " Lframe: " + playerStateData.Position);

                if (Vector3.Distance(info.Data.Position, playerStateData.Position) > 0.05f)
                {
                    ClientStats.instance.Reconciliations.AddNow();

                    FirstPersonController controller = GetComponent<FirstPersonController>();

                    List<ReconciliationInfo> infos = reconciliationHistory.ToList();
                    interpolation.CurrentData = playerStateData;
                    Quaternion oldHeadRotation = controller.camera.transform.rotation;

                    transform.position = playerStateData.Position;
                    transform.rotation = playerStateData.Rotation;
                    for (int i = 0; i < infos.Count; i++)
                    {
                        PlayerStateData u = playerLogic.GetNextFrameData(infos[i].Input, interpolation.CurrentData);
                        interpolation.SetFramePosition(u);
                    }

                    controller.camera.transform.rotation = oldHeadRotation;
                }
            }
        }
        else
        {
            interpolation.SetFramePosition(playerStateData);
        }

        foreach (var _streamData in streamDatas)
            _streamData.OnServerDataUpdate(playerStateData, isOwn);
    }
}

