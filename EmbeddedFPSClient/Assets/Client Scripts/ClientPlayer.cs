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

    private readonly Queue<ReconciliationInfo> reconciliationHistory = new Queue<ReconciliationInfo>();
    public int ReconciliationHistorySize => reconciliationHistory.Count;

    public string playerName { get; private set; }
    public bool isOwn { get; private set; }

    private int health;

    public int Kills { get; set; }
    public int Deaths { get; set; }

    [Header("Prefabs")]
    [SerializeField]
    private GameObject shotPrefab;
    private readonly List<IStreamData> streamDatas = new List<IStreamData>();

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
        this.playerName = playerName;
        SetHealth(100);
        if (ConnectionManager.Instance.OwnPlayerId == id)
        {
            isOwn = true;
            interpolation.CurrentData = new PlayerStateData(id, new PlayerInputData(), 0, transform.position, transform.rotation, CollisionFlags.None);
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

            if (inputData.HasAction(PlayerAction.Reload)) //TODO: find better place for this
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

                //uncomment logging statements to debug reconciliation differences
                //Debug.Log("Local: " + info.Data.ToString());
                //Debug.Log("Remote: " + playerStateData.ToString());

                if (Vector3.Distance(info.Data.Position, playerStateData.Position) > 0.05f)
                {
                    //Debug.Log("RECONCILE!!!");

                    ClientStats.instance.Reconciliations.AddNow();

                    FirstPersonController fpController = GetComponent<FirstPersonController>();

                    List<ReconciliationInfo> infos = reconciliationHistory.ToList();
                    interpolation.CurrentData = playerStateData;
                    Quaternion oldHeadRotation = fpController.camera.transform.rotation;

                    transform.position = playerStateData.Position;
                    transform.rotation = playerStateData.Rotation;
                    for (int i = 0; i < infos.Count; i++)
                    {
                        PlayerStateData u = playerLogic.GetNextFrameData(infos[i].Input, interpolation.CurrentData);
                        interpolation.SetFramePosition(u);
                    }

                    fpController.camera.transform.rotation = oldHeadRotation;
                }
                else
                {
                    //Debug.Log(" ");
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

