using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour, IServerUpdateListener
{
    [SerializeField]
    private List<Weapon> weapons = new List<Weapon>();

    [SerializeField]
    private Animator characterAnimator;

    [SerializeField]
    private Transform tpWeaponHolder;

    public Weapon CurrentWeapon { get; private set; }

    private int currentWeaponIndex = 0;

    private Coroutine switchCoroutine;

    public void OnServerDataUpdate(PlayerStateData playerStateData, bool isOwn)
    {
        if (isOwn)
        {
            return;
        }

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
        CurrentWeapon.Fire();
    }

    public void Reload()
    {
        CurrentWeapon.Reload();
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
        CurrentWeapon.Inspect();
    }

    private IEnumerator SwitchWeapon(int index)
    {
        if (weapons.Count == 0)
        {
            yield break;
        }

        if (index >= weapons.Count)
        {
            index = 0;
        }

        if (CurrentWeapon)
        {
            if (CurrentWeapon == weapons[index])
            {
                yield break;
            }

            if (CurrentWeapon.gameObject.activeInHierarchy)
            {
                CurrentWeapon.SwitchOut();

                yield return new WaitUntil(() => !CurrentWeapon.gameObject.activeSelf);
            }

            CurrentWeapon.tpWeapon.SetActive(false);
        }

        currentWeaponIndex = index;

        CurrentWeapon = weapons[currentWeaponIndex];

        CurrentWeapon.SwitchIn();

        characterAnimator.runtimeAnimatorController = CurrentWeapon.characterAnimatorOverrider;

        CurrentWeapon.tpWeapon.SetActive(true);
        CurrentWeapon.tpWeapon.transform.SetParent(tpWeaponHolder);
    }
}
