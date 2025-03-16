using UnityEngine;
using TMPro;
using Unity.Netcode;
using UnityEngine.Events;

public class ProjectileGun : NetworkBehaviour
{
    public UnityEvent OnGunShoot;

    [Header("Prefabs")]
    [SerializeField] private NetworkObject bulletPrefab; // Must have NetworkObject on bullet!

    // bullet force
    public float shootForce, upwardForce;

    // Gun stats
    public float timeBetweenShooting, spread, reloadTime, timeBetweenShots;
    public int magazineSize, bulletsPerTap;
    public bool allowButtonHold;

    // References
    public Camera fpsCam;             // Or Cinemachine camera
    public Transform attackPoint;     // Where bullets spawn
    [SerializeField] private Transform cameraHolder; // Possibly the parent of your camera

    // Graphics
    public GameObject muzzleFlash;
    public TextMeshProUGUI ammunitionDisplay;

    // Recoil
    public Rigidbody playerRb;
    public float recoilForce;

    // Private
    private Transform PlayerCam;
    private bool shooting, readyToShoot, reloading;
    private int bulletsLeft, bulletsShot;
    private bool allowInvoke = true;

    private void Awake()
    {
        // Fill magazine
        bulletsLeft = magazineSize;
        readyToShoot = true;
    }

    private void Start()
    {
        // Possibly override this if Cinemachine is controlling the camera
        // but cameraHolder is the transform we rotate
        PlayerCam = cameraHolder ? cameraHolder : fpsCam.transform;

        // Hide ammo display if we are not the owner
        if (!IsOwner && ammunitionDisplay != null)
        {
            ammunitionDisplay.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        // Only handle input if we own this gun
        if (!IsOwner) return;

        MyInput();

        // Update ammo display (local owner only)
        if (ammunitionDisplay != null)
        {
            ammunitionDisplay.SetText(
                $"{bulletsLeft / bulletsPerTap} / {magazineSize / bulletsPerTap}"
            );
        }
    }

    private void MyInput()
    {
        // Check if allowed to hold down button
        shooting = allowButtonHold ? Input.GetKey(KeyCode.Mouse0) : Input.GetKeyDown(KeyCode.Mouse0);

        // Reload input
        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !reloading)
        {
            Reload();
        }

        // Auto reload if trying to shoot with no ammo
        if (readyToShoot && shooting && !reloading && bulletsLeft <= 0)
        {
            Reload();
        }

        // Fire
        if (readyToShoot && shooting && !reloading && bulletsLeft > 0)
        {
            bulletsShot = 0; // reset shot counter
            Shoot();
        }
    }

    private void Shoot()
    {
        readyToShoot = false;
        OnGunShoot?.Invoke();
        Debug.Log("Pew!");

        // ---------------------------------------------
        // 1) Raycast from camera to find target point
        // ---------------------------------------------
        // Use PlayerCam if that is truly your viewpoint,
        // or fpsCam.transform if using a direct reference to the real camera.
        Ray ray = new Ray(PlayerCam.position, PlayerCam.forward);
        RaycastHit hit;
        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit, 100f))
        {
            targetPoint = hit.point;
        }
        else
        {
            // If nothing is hit, pick a point far away in front
            targetPoint = ray.GetPoint(100f);
        }

        // ---------------------------------------------
        // 2) Compute direction from attackPoint to targetPoint
        // ---------------------------------------------
        Vector3 directionWithoutSpread = targetPoint - attackPoint.position;

        // ---------------------------------------------
        // 3) Apply spread
        // ---------------------------------------------
        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);
        Vector3 directionWithSpread = directionWithoutSpread + new Vector3(x, y, 0f);

        // ---------------------------------------------
        // 4) Spawn bullet on the server
        // ---------------------------------------------
        // We pass in attackPoint.position as spawn position
        // and directionWithSpread as forward direction
        SpawnBulletServerRpc(attackPoint.position, directionWithSpread.normalized);

        // Instantiate muzzle flash locally if desired
        if (muzzleFlash != null)
        {
            Instantiate(muzzleFlash, attackPoint.position, Quaternion.identity);
        }

        // Subtract bullet and increment shot count
        bulletsLeft--;
        bulletsShot++;

        // Add recoil to local player's RigidBody (one time only)
        if (allowInvoke)
        {
            // Negative of bullet direction
            playerRb.AddForce(-directionWithSpread.normalized * recoilForce, ForceMode.Impulse);
            Invoke(nameof(ResetShot), timeBetweenShooting);
            allowInvoke = false;
        }

        // If we have multiple bulletsPerTap, keep firing
        if (bulletsShot < bulletsPerTap && bulletsLeft > 0)
        {
            Invoke(nameof(Shoot), timeBetweenShots);
        }
    }

    private void ResetShot()
    {
        readyToShoot = true;
        allowInvoke = true;
    }

    private void Reload()
    {
        reloading = true;
        Invoke(nameof(ReloadFinished), reloadTime);
    }

    private void ReloadFinished()
    {
        bulletsLeft = magazineSize;
        reloading = false;
    }

    // -----------------------------
    // ServerRPC for bullet spawn
    // -----------------------------
    [ServerRpc]
    private void SpawnBulletServerRpc(Vector3 spawnPos, Vector3 bulletForward)
    {
        // Instantiate from the NetworkObject bullet prefab
        NetworkObject bulletInstance = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

        // Orient bullet
        bulletInstance.transform.forward = bulletForward;

        // Spawn across all clients
        bulletInstance.Spawn();

        // If the bullet has a script that moves it (like physics),
        // that script can be responsible for applying forces or movement.
        // Or you could do it right here if you added a rigidbody to the bullet.
        if (bulletInstance.TryGetComponent(out Rigidbody bulletRb))
        {
            // Server applies the force
            bulletRb.AddForce(bulletForward * shootForce, ForceMode.Impulse);
            bulletRb.AddForce(Vector3.up * upwardForce, ForceMode.Impulse);
        }
    }
}
