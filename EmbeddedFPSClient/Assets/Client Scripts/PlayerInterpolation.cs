using UnityEngine;

public class PlayerInterpolation : MonoBehaviour
{
    private float lastInputTime;

    public PlayerStateData CurrentData { get; set; }
    public PlayerStateData PreviousData { get; private set; }

    public bool IsOwn { get; set; }

    public void SetFramePosition(PlayerStateData data)
    {
        RefreshToPosition(data, CurrentData);
    }

    private void RefreshToPosition(PlayerStateData data, PlayerStateData prevData)
    {
        PreviousData = prevData;
        CurrentData = data;
        lastInputTime = Time.fixedTime;
    }

    public void Update()
    {
        float timeSinceLastInput = Time.time - lastInputTime;
        float t = timeSinceLastInput / Time.fixedDeltaTime;
        transform.position = Vector3.LerpUnclamped(PreviousData.Position, CurrentData.Position, t);
        if (!IsOwn)
            transform.rotation = Quaternion.SlerpUnclamped(PreviousData.Rotation, CurrentData.Rotation, t);
    }
}

