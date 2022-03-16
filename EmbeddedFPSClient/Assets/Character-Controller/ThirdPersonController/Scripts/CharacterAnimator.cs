using StarterAssets;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    [SerializeField]
    Transform debugTransform;

    Animator anim;

    public Camera cam;
    
    [SerializeField]
    private LayerMask aimColliderMask;

    [SerializeField]
    Transform leftHandPoint;

    [SerializeField]
    GameObject bullet;
    [SerializeField]
    Transform bulletSpawnPoint;
    [SerializeField]
    float shootForce;
    [SerializeField]
    float rateOfFire;

    float lastFired;

    bool rifle;
    private StarterAssetsInputs input;

    private void Awake()
    {
        input = GetComponent<StarterAssetsInputs>();
        anim = GetComponent<Animator>();

        //The cam will be disabled by default
        //cam = GetComponentInChildren<Camera>();

        rifle = true;
    }

    private void LateUpdate()
    {
        GetTarget();

        //if (rifle)
        //{
        //    if (Input.GetMouseButton(0) && Time.time - lastFired > 1 / rateOfFire)
        //    {
        //        Shoot();
        //        lastFired = Time.time;
        //    }
        //}
        //else
        //{
        //    if(Input.GetMouseButtonDown(0))
        //    Shoot();
        //}
    }

    public void GetTarget()
    {
        if (!debugTransform || !cam) return;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, cam.farClipPlane, aimColliderMask))
            debugTransform.position = hit.point;
        else
            debugTransform.transform.position = cam.transform.position + cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)).direction.normalized * cam.farClipPlane;
    }

    private void OnAnimatorIK(int layerIndex)
    {
        anim.SetIKPosition(AvatarIKGoal.LeftHand, leftHandPoint.position);
        anim.SetIKRotation(AvatarIKGoal.LeftHand, leftHandPoint.rotation);
    }

    //void Shoot()
    //{
    //    GameObject go = Instantiate(bullet, bulletSpawnPoint.position, Quaternion.identity);
    //    go.transform.forward = bulletSpawnPoint.forward;
    //    go.GetComponent<Rigidbody>().AddForce(bulletSpawnPoint.forward * shootForce);
    //    Destroy(go, 20);
    //}
}
