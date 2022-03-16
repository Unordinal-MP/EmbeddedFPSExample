using UnityEngine;

public class PlayerInterpolation : MonoBehaviour
{
    private float lastInputTime;

    public PlayerStateData CurrentData { get; set; }
    public PlayerStateData PreviousData { get; private set; }

    public void SetFramePosition(PlayerStateData data)
    {
        RefreshToPosition(data, CurrentData);
    }

    public void RefreshToPosition(PlayerStateData data, PlayerStateData prevData)
    {
        PreviousData = prevData;
        CurrentData = data;
        lastInputTime = Time.time;
    }

    public void Update()
    {
        float timeSinceLastInput = Time.time - lastInputTime;
        float t = timeSinceLastInput / Time.fixedDeltaTime;
        transform.position = Vector3.Lerp(PreviousData.Position, CurrentData.Position, t);
        Vector3 lookRotation = new Vector3(0, CurrentData.LookDirection.y, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation,Quaternion.Euler(lookRotation), t); // remove but cause auto rotation
    }
}