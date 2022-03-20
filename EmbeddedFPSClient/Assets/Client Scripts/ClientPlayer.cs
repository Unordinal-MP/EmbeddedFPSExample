using System.Collections.Generic;
using System.Linq;
using DarkRift;
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

using UnityEngine.Events;

public struct ReconciliationInfo
{
    public ReconciliationInfo(uint frame, PlayerStateData data /*PlayerInputData input*/)
    {
        Frame = frame;
        Data = data;
        //Input = input;
    }

    public uint Frame;
    public PlayerStateData Data;
    //public PlayerInputData Input;
}

public interface IStreamData
{
    void OnServerDataUpdate(PlayerStateData playerStateData, bool isOwn);
}

[RequireComponent(typeof(IPlayerLogic))]
[RequireComponent(typeof(PlayerInterpolation))]
public class ClientPlayer : MonoBehaviour
{
    [SerializeField]
    float positionError = 5;

    [SerializeField]
    private Camera playerCamera;

    [SerializeField]
    private UnityEvent localControllerInitalized = new UnityEvent();

    protected IPlayerLogic playerLogic;

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
        playerLogic = GetComponent<IPlayerLogic>();
        interpolation = GetComponent<PlayerInterpolation>();
        playerHealth = GetComponent<PlayerHealth>();

        var _streamDatas = GetComponents<IStreamData>();

        foreach (var _streamData in _streamDatas)
        {
            streamDatas.Add(_streamData);
        }
    }

    private void Update()
    {
        if (!isOwn) return;

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

            interpolation.CurrentData = new PlayerStateData(id);

            localControllerInitalized.Invoke();
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
                        playerStateData = playerLogic.GetNextFrameData(interpolation.CurrentData, GameManager.Instance.LastReceivedServerTick - 1);
                    }
                }
            }

            if (reconciliationHistory.Count > 20)
            {
                reconciliationHistory.Dequeue();
            }
        }

        foreach (var _streamData in streamDatas)
            _streamData.OnServerDataUpdate(playerStateData, isOwn);
    }

    public virtual void SetHealth(float value)
    {
        health = value;
        //healthBarFill.fillAmount = value / 100f;
    }

    public void PlayerUpdate()
    {
        PlayerStateData currentData = new PlayerStateData(id);

        //transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime);
        //interpolation.CurrentData = currentData;

        PlayerStateData nextStateData = playerLogic.GetNextFrameData(currentData, GameManager.Instance.LastReceivedServerTick - 1);
        interpolation.CurrentData = nextStateData;

        interpolation.SetFramePosition(nextStateData);

        using (Message message = Message.Create((ushort)Tags.GamePlayerInput, nextStateData))
        {
            ConnectionManager.Instance.Client.SendMessage(message, SendMode.Reliable);
        }

        if (reconciliationHistory.Count < 20)
            reconciliationHistory.Enqueue(new ReconciliationInfo(GameManager.Instance.ClientTick, nextStateData));
        else
        {
            reconciliationHistory.Dequeue();
            reconciliationHistory.Enqueue(new ReconciliationInfo(GameManager.Instance.ClientTick, nextStateData));
        }
    }
}