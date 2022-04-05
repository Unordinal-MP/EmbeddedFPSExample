using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour, IStreamData
{
    [SerializeField]
    private List<Weapon> weapons = new List<Weapon>();

    [SerializeField]
    private Animator characterAnimator;

    [SerializeField]
    private Transform tpWeaponHolder;

    public Weapon currentWeapon { get; private set; }

    protected int currentWeaponIndex = 0;

    private Coroutine switchCoroutine;

    public void OnServerDataUpdate(PlayerStateData playerStateData, bool isOwn)
    {
        if (isOwn) return;

        if (playerStateData.Input.HasAction(PlayerAction.SwitchWeapon))
        {
            switchCoroutine = StartCoroutine(SwitchWeapon(currentWeaponIndex + 1));
        }
    }

    private void Start()
    {
        switchCoroutine = StartCoroutine(SwitchWeapon(0));
    }

    public void Fire()
    {
        currentWeapon.Fire();
    }

    public void Reload()
    {
        currentWeapon.Reload();
    }

    public void SwitchWeapon()
    {
        if (switchCoroutine != null)
        {
            StopCoroutine(switchCoroutine);

            switchCoroutine = null;
        }

        switchCoroutine = StartCoroutine(SwitchWeapon(currentWeaponIndex + 1));
    }

    public void Inspect()
    {
        currentWeapon.Inspect();
    }

    private IEnumerator SwitchWeapon(int index)
    {
        if (weapons.Count == 0) yield break;

        if (index >= weapons.Count)
        {
            index = 0;
        }

        if (currentWeapon)
        {
            if (currentWeapon == weapons[index]) yield break;

            if (currentWeapon.gameObject.activeInHierarchy)
            {
                currentWeapon.SwitchOut();

                yield return new WaitUntil(() => !currentWeapon.gameObject.activeSelf);
            }

            currentWeapon.tpWeapon.SetActive(false);
        }

        currentWeaponIndex = index;

        currentWeapon = weapons[currentWeaponIndex];

        currentWeapon.SwitchIn();

        characterAnimator.runtimeAnimatorController = currentWeapon.characterAnimatorOverrider;

        currentWeapon.tpWeapon.SetActive(true);
        currentWeapon.tpWeapon.transform.SetParent(tpWeaponHolder);
    }
}
