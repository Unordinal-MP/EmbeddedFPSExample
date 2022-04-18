using UnityEngine;

public class CharacterAnimator : MonoBehaviour, IServerUpdateListener
{
    [SerializeField]
    private Animator animator;

    [SerializeField]
    private float syncSpeed = 5f;

    private int inputYID;

    private int inputXID;

    private int isAimingID;

    private int isSprintingID;

    private int isReloadingID;

    private int isShootingID;

    private int isHitID;

    private void Awake()
    {
        if (!animator)
        {
            animator = GetComponent<Animator>();
        }

        inputYID = Animator.StringToHash("inputY");
        inputXID = Animator.StringToHash("inputX");
        isAimingID = Animator.StringToHash("isAiming");
        isSprintingID = Animator.StringToHash("isSprinting");
        isReloadingID = Animator.StringToHash("isReloading");
        isShootingID = Animator.StringToHash("Shoot");
        isHitID = Animator.StringToHash("isHit");
    }

    public void OnServerDataUpdate(PlayerStateData playerStateData, bool isOwn)
    {
        if (isOwn)
        {
            return;
        }

        float checkRight = playerStateData.Input.HasAction(PlayerAction.Right) ? 1 : 0;
        float horizontal = playerStateData.Input.HasAction(PlayerAction.Left) ? -1 : checkRight;

        float checkForward = playerStateData.Input.HasAction(PlayerAction.Forward) ? 1 : 0;
        float vertical = playerStateData.Input.HasAction(PlayerAction.Back) ? -1 : checkForward;

        animator.SetFloat(inputXID, horizontal);
        animator.SetFloat(inputYID, vertical);

        if (playerStateData.Input.HasAction(PlayerAction.Fire))
        {
            animator.Play(isShootingID);
        }
        
        SetBool(isSprintingID, playerStateData.Input.HasAction(PlayerAction.Sprint));
        SetBool(isAimingID, playerStateData.Input.HasAction(PlayerAction.Aim));

        SetTrigger(isReloadingID, playerStateData.Input.HasAction(PlayerAction.Reload));
    }

    private void SetBool(int id, bool value)
    {
        if (value != animator.GetBool(id))
        {
            animator.SetBool(id, value);
        }
    }

    private void SetTrigger(int id, bool value)
    {
        if (value && !animator.GetBool(id))
        {
            animator.SetTrigger(id);
        }
    }
}
