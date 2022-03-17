using StarterAssets;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour, IStreamData
{
    [SerializeField]
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void OnServerDataUpdate(PlayerStateData playerStateData)
    {
        //TODO: Implement this once the new model is implemented
        //Vector3 _movement = (playerStateData.Position - transform.position).normalized;

        //animator.SetFloat(animIDSpeedHorizontal, _movement.x * speedModifier, Time.deltaTime, Time.deltaTime);
        //animator.SetFloat(animIDSpeedVertical, _movement.y * speedModifier, Time.deltaTime, Time.deltaTime);
        //animator.SetFloat(animIDMotionSpeed, new Vector2(_movement.y, _movement.x).magnitude);
    }
}
