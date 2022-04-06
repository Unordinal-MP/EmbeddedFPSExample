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
#pragma warning disable UNT0004 // Time.fixedDeltaTime used with Update
        float t = timeSinceLastInput / Time.fixedDeltaTime; //between frames timestep
#pragma warning restore UNT0004 // Time.fixedDeltaTime used with Update
        transform.position = Vector3.LerpUnclamped(PreviousData.Position, CurrentData.Position, t);
        if (!IsOwn)
        {
            transform.rotation = Quaternion.SlerpUnclamped(PreviousData.Rotation, CurrentData.Rotation, t);
        }
    }
}
