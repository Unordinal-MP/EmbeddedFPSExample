using UnityEngine;

public class PlayerInterpolation : MonoBehaviour
{
    private double lastInputTime;
    private Vector3 interpolatedVelocity;

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

    public void Update()
    {
        float timeSinceLastInput = (float)(Time.timeAsDouble - lastInputTime);
        float timeBetweenFrames = Constants.TickInterval;
        float t = timeSinceLastInput / timeBetweenFrames;

        if (IsOwn)
        {
            transform.position = Vector3.LerpUnclamped(PreviousData.Position, CurrentData.Position, t);
            //skip setting rotation for own player since we have authority over it
        }
        else
        {
            float smoothTime = 0.03f;
            Vector3 position = Vector3.SmoothDamp(transform.position, CurrentData.Position, ref interpolatedVelocity, smoothTime);
            Quaternion rotation = Quaternion.SlerpUnclamped(PreviousData.Rotation, CurrentData.Rotation, t);
            transform.SetPositionAndRotation(position, rotation);
        }
    }
}
