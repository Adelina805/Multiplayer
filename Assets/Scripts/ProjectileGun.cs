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
    public GameObject firstPersonGun; // The local-view gun mesh/model
    public GameObject thirdPersonGun; // The third-person gun mesh/model (networked)
    [SerializeField] private Transform cameraHolder; // parent of your camera

    // Graphics
    public GameObject muzzleFlash;
    public TextMeshProUGUI ammunitionDisplay;
    [SerializeField] private GameObject crosshairText;

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
        // Decide which camera/gun to show: 
        // The local/first-person camera + gun is only for the owner.
        // The third-person gun is for everyone else.
        if (!IsOwner)
        {
            // Disable local FPS camera & local gun
            if (fpsCam) fpsCam.gameObject.SetActive(false);
            if (firstPersonGun) firstPersonGun.SetActive(false);
        }
        else
        {
            // Disable the third-person gun for our local view (so we don't see it floating in front of us)
            if (thirdPersonGun) thirdPersonGun.SetActive(false);
        }

        // If Cinemachine is not used, we track the cameraHolder for rotation
        PlayerCam = cameraHolder ? cameraHolder : fpsCam.transform;

        // Hide ammo display and crosshair if we are not the owner
        if (!IsOwner && ammunitionDisplay != null)
        {
            ammunitionDisplay.gameObject.SetActive(false);
            crosshairText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        // Only handle input if we own this gun
        if (!IsOwner) return;

        // Match the camera rotation
        transform.rotation = PlayerCam.rotation;

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
        NetworkAudioManager.Instance.PlaySoundGlobal(AudioClipID.GunShoot);
        NetworkAudioManager.Instance.PlaySoundGlobal(AudioClipID.GunPew);
        Debug.Log("Pew!");

        // Raycast from camera to find target point
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

        // Compute direction from attackPoint to targetPoint
        Vector3 directionWithoutSpread = targetPoint - attackPoint.position;

        // Apply spread
        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);
        Vector3 directionWithSpread = directionWithoutSpread + new Vector3(x, y, 0f);

        // Spawn bullet on the server
        SpawnBulletServerRpc(attackPoint.position, directionWithSpread.normalized);

        bulletsLeft--;
        bulletsShot++;

        // Use the gun’s transform rotation
        Quaternion muzzleRotation = attackPoint.rotation;

        // Request to Spawn muzzle flash on the server
        RequestMuzzleFlashServerRpc(attackPoint.position, muzzleRotation);

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
        NetworkAudioManager.Instance.PlaySoundLocal(AudioClipID.GunReload);
        Invoke(nameof(ReloadFinished), reloadTime);
    }

    private void ReloadFinished()
    {
        bulletsLeft = magazineSize;
        reloading = false;
    }

    // ServerRPC for bullet spawn
    [ServerRpc]
    private void SpawnBulletServerRpc(Vector3 spawnPos, Vector3 bulletForward)
    {
        // Instantiate from the NetworkObject bullet prefab
        NetworkObject bulletInstance = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

        // Orient bullet
        bulletInstance.transform.forward = bulletForward;

        // Spawn across all clients
        bulletInstance.Spawn();

        // apply forces to bullet 
        if (bulletInstance.TryGetComponent(out Rigidbody bulletRb))
        {
            // Server applies the force
            bulletRb.AddForce(bulletForward * shootForce, ForceMode.Impulse);
            bulletRb.AddForce(Vector3.up * upwardForce, ForceMode.Impulse);
        }
    }

   // ServerRPC for muzzle flash 
    [ServerRpc]
    private void RequestMuzzleFlashServerRpc(Vector3 position, Quaternion rotation)
    {
        // The server calls the ClientRpc for all clients
        SpawnMuzzleFlashClientRpc(position, rotation);
    }

    // ClientRPC for muzzle flash 
    [ClientRpc]
    private void SpawnMuzzleFlashClientRpc(Vector3 position, Quaternion rotation)
    {
        if (muzzleFlash != null)
        {
            // Instantiate the muzzle flash prefab with the given rotation
            Instantiate(muzzleFlash, position, rotation);
        }
    }

}
