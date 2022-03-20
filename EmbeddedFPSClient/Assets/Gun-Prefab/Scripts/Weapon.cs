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

    [SerializeField]
    protected GameObject tpWeapon;

    [SerializeField]
    protected AnimatorOverrideController tpAnimator;

    [SerializeField]
    private ParticleSystem muzzleFlash;

    [Header("Stats")]
    public float shootCooldown = 0.1f;

    protected bool isPuttingAway;

    protected bool isGettingWeapon;

    public bool isReloading { get; private set; }

    public bool isShooting { get; private set; }

    public bool isReady { get => !isPuttingAway && !isPuttingAway; }

    [Header("Whacky")]
    public float reloadTime = 1f;

    public float switchInTime = 1f;

    public float switchOutTime = 1f;

    private Coroutine reloadRoutine;
    private Coroutine switchInRoutine;
    private Coroutine switchOutRoutine;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void Fire()
    {
        if (!isReady || isReloading || isShooting) return;

        isShooting = true;

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
        if (!isReady || isReloading) return;

        animator.Play("Reload Full");

        isReloading = true;

        reloadRoutine = StartCoroutine(ReloadFinished());
    }

    public void Inspect()
    {
        if (!isReady || isReloading) return;

        animator.Play("Inspect");
    }

    public void SwitchIn()
    {
        if (isGettingWeapon || isPuttingAway || isReloading) return;

        isGettingWeapon = true;

        gameObject.SetActive(true);

        animator.Play("Draw");

        switchInRoutine = StartCoroutine(SwitchInFinished());
    }

    public void SwitchOut()
    {
        if (isGettingWeapon || isPuttingAway || isReloading) return;

        isPuttingAway = true;

        animator.Play("Putaway");

        switchOutRoutine = StartCoroutine(SwitchOutFinished());
    }

    private IEnumerator ShootCooldown()
    {
        yield return new WaitForSecondsRealtime(shootCooldown);

        isShooting = false;
    }

    public IEnumerator SwitchInFinished()
    {
        if (!isGettingWeapon) yield break;

        yield return new WaitForSecondsRealtime(switchInTime);

        isGettingWeapon = false;
    }

    public IEnumerator SwitchOutFinished()
    {
        if (!isPuttingAway) yield break;

        yield return new WaitForSecondsRealtime(switchOutTime);

        gameObject.SetActive(false);

        isPuttingAway = false;
    }

    public IEnumerator ReloadFinished()
    {
        yield return new WaitForSecondsRealtime(reloadTime);

        isReloading = false;
    }
}
