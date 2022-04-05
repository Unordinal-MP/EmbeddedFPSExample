using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField]
    protected Animator animator;

    [SerializeField]
    protected GameObject bullet;

    [SerializeField]
    protected float bulletForce;

    [SerializeField]
    protected Transform bulletTransform;

#pragma warning disable SA1307 // Accessible fields should begin with upper-case letter
    public GameObject tpWeapon;
#pragma warning restore SA1307 // Accessible fields should begin with upper-case letter

#pragma warning disable SA1307 // Accessible fields should begin with upper-case letter
    public AnimatorOverrideController characterAnimatorOverrider;
#pragma warning restore SA1307 // Accessible fields should begin with upper-case letter

    [SerializeField]
    private ParticleSystem muzzleFlash;

    [Header("Stats")]
#pragma warning disable SA1307 // Accessible fields should begin with upper-case letter
    public float shootCooldown = 0.1f;
#pragma warning restore SA1307 // Accessible fields should begin with upper-case letter

    protected bool isPuttingAway;

    protected bool isGettingWeapon;

    public bool IsReloading { get; private set; }

    public bool IsShooting { get; private set; }

    public bool IsReady { get => true; } //TODO: !isPuttingAway

    [Header("Whacky")]
#pragma warning disable SA1307 // Accessible fields should begin with upper-case letter
    public float reloadTime = 1f;
#pragma warning restore SA1307 // Accessible fields should begin with upper-case letter

#pragma warning disable SA1307 // Accessible fields should begin with upper-case letter
    public float switchInTime = 1f;
#pragma warning restore SA1307 // Accessible fields should begin with upper-case letter

#pragma warning disable SA1307 // Accessible fields should begin with upper-case letter
    public float switchOutTime = 1f;
#pragma warning restore SA1307 // Accessible fields should begin with upper-case letter

    private Coroutine reloadRoutine;
    private Coroutine switchInRoutine;
    private Coroutine switchOutRoutine;

    // Start is called before the first frame update
    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void Fire()
    {
        if (!IsReady || IsReloading || IsShooting)
        {
            return;
        }

        IsShooting = true;

        animator.Play("Fire");
        muzzleFlash.Play();

        GameObject go = Instantiate(bullet, bulletTransform.position, Quaternion.identity);
        go.transform.forward = bulletTransform.forward;
        go.GetComponent<Rigidbody>().AddForce(bulletTransform.forward * bulletForce);
        Destroy(go, 20);

        StartCoroutine(ShootCooldown());
    }

    public void Reload()
    {
        if (!IsReady || IsReloading)
        {
            return;
        }

        animator.Play("Reload Full");

        IsReloading = true;

        reloadRoutine = StartCoroutine(ReloadFinished());
    }

    public void Inspect()
    {
        if (!IsReady || IsReloading)
        {
            return;
        }

        animator.Play("Inspect");
    }

    public void SwitchIn()
    {
        if (isGettingWeapon || isPuttingAway || IsReloading)
        {
            return;
        }

        isGettingWeapon = true;

        gameObject.SetActive(true);

        animator.Play("Draw");

        if (gameObject.activeInHierarchy)
        {
            switchInRoutine = StartCoroutine(SwitchInFinished());
        }
    }

    public void SwitchOut()
    {
        if (isGettingWeapon || isPuttingAway || IsReloading)
        {
            return;
        }

        isPuttingAway = true;

        animator.Play("Putaway");

        switchOutRoutine = StartCoroutine(SwitchOutFinished());
    }

    private IEnumerator ShootCooldown()
    {
        yield return new WaitForSecondsRealtime(shootCooldown);

        IsShooting = false;
    }

    public IEnumerator SwitchInFinished()
    {
        if (!isGettingWeapon)
        {
            yield break;
        }

        yield return new WaitForSecondsRealtime(switchInTime);

        isGettingWeapon = false;
    }

    public IEnumerator SwitchOutFinished()
    {
        if (!isPuttingAway)
        {
            yield break;
        }

        yield return new WaitForSecondsRealtime(switchOutTime);

        gameObject.SetActive(false);

        isPuttingAway = false;
    }

    public IEnumerator ReloadFinished()
    {
        yield return new WaitForSecondsRealtime(reloadTime);

        IsReloading = false;
    }
}
