using UnityEngine;

public class PlayerInterpolation : MonoBehaviour, IStreamData
{
    private float lastInputTime;

    public PlayerStateData CurrentData { get; set; }
    public PlayerStateData PreviousData { get; private set; }

    public void SetFramePosition(PlayerStateData data)
    {
        PreviousData = CurrentData;
        CurrentData = data;
        lastInputTime = Time.time;
    }

    public void OnServerDataUpdate(PlayerStateData data, bool isOwn)
    {
        //Needs to be removed once the player controller is setup for the server, so the movement actually is authorative
        if (isOwn) return;

        SetFramePosition(data);

        float timeSinceLastInput = Time.time - lastInputTime;
        float t = timeSinceLastInput / Time.fixedDeltaTime;
        transform.position = Vector3.LerpUnclamped(PreviousData.Position, CurrentData.Position, t);

        Vector3 lookRotation = new Vector3(0, CurrentData.LookDirection.y, 0);
        transform.rotation = Quaternion.SlerpUnclamped(Quaternion.Euler(new Vector3(0f, PreviousData.LookDirection.y, 0f)), Quaternion.Euler(lookRotation), t);
    }
}