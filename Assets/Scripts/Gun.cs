using UnityEngine;
using UnityEngine.Events;

public class Gun : MonoBehaviour
{
    public UnityEvent OnGunShoot;
    public float FireCooldown;
    public bool Automatic; // default semi

    private float CurrentCooldown;

    void Start()
    {
        CurrentCooldown = FireCooldown;
        
    }

    void Update()
    {
        if (Automatic)
        {
            // Semi - Automatic weapon
            if (Input.GetMouseButton(0))
            {
                if (CurrentCooldown <= 0f)
                {
                    Debug.Log("pew");
                    OnGunShoot?.Invoke();
                    CurrentCooldown = FireCooldown;
                }
            }
        }
        else 
        {
            // other weapon
            if (Input.GetMouseButtonDown(0))
            {
                if (CurrentCooldown <= 0f)
                {
                    OnGunShoot?.Invoke();
                    CurrentCooldown = FireCooldown;
                }
            }
        }

        CurrentCooldown -= Time.deltaTime;
        
    }
}
