using StarterAssets;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour, IStreamData
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
        if(!animator)
            animator = GetComponent<Animator>();

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

        animator.SetFloat(inputXID, playerStateData.horizontal);
        animator.SetFloat(inputYID, playerStateData.vertical);

        if(playerStateData.isShooting)
        {
            animator.Play(isShootingID);
        }
        
        SetBool(isSprintingID, playerStateData.isSprinting);
        SetBool(isAimingID, playerStateData.isAiming);

        SetTrigger(isReloadingID, playerStateData.isReloading);
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
