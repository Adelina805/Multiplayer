using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunDamage : MonoBehaviour
{
    public float Damage;
    public float BulletRange;
    private Transform PlayerCam;

    //[SerializeField] private CinemachineVirtualCamera camera;
    [SerializeField] private Transform cameraHolder;
    
    private void Start()
    {
        PlayerCam = cameraHolder.transform;
    }

    public void Shoot()
    {
        Ray gunRay = new Ray(PlayerCam.position, PlayerCam.forward);

        if (Physics.Raycast(gunRay, out RaycastHit hitInfo, BulletRange))
        {
            if (hitInfo.collider.gameObject.TryGetComponent(out Entity enemy))
            {
                enemy.Health -= Damage;
            }
        }
        
    }
}
