using UnityEngine;

public class PlayerInterpolation : MonoBehaviour
{
    private double lastInputTime;

    public PlayerStateData CurrentData { get; set; }
    public PlayerStateData PreviousData { get; private set; }

    public bool IsOwn { get; set; }

    public void SetFramePosition(PlayerStateData data)
    {
        PreviousData = CurrentData;

        if (!IsOwn)
        {
            var previous = PreviousData;
            previous.Position = transform.position;
            previous.Rotation = transform.rotation;
            PreviousData = previous;
        }

        CurrentData = data;
        lastInputTime = Time.timeAsDouble;
    }

    private Vector3 interpolatedVelocity;

    public void Update()
    {
        float timeSinceLastInput = (float)(Time.timeAsDouble - lastInputTime);
        float timeBetweenFrames = Constants.TickInterval;
        float t = timeSinceLastInput / timeBetweenFrames;
        
        if (IsOwn)
        {
            transform.position = Vector3.LerpUnclamped(PreviousData.Position, CurrentData.Position, t);
        }
        else
        {
            transform.position = Vector3.SmoothDamp(transform.position, CurrentData.Position, ref interpolatedVelocity, 0.002f / Time.deltaTime, 1.05f * (CurrentData.Position - PreviousData.Position).magnitude / Time.deltaTime);
            transform.rotation = Quaternion.SlerpUnclamped(PreviousData.Rotation, CurrentData.Rotation, t);
        }
    }
}
