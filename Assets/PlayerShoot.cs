using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class PlayerShoot : MonoBehaviour
{
    [Header("Munición")]
    public int maxAmmo = 6;
    public int currentAmmo;

    [Header("Arma / Proyectil")]
    public GameObject bulletPrefab; 
    public Transform firePoint;     
    public float bulletSpeed = 30f;

    [Header("Pooling")]
    public int poolSize = 10;
    private Queue<GameObject> bulletPool = new Queue<GameObject>();

    [Header("Recarga")]
    public float reloadTime = 1f;
    private bool isReloading = false;

    private void Awake()
    {
        currentAmmo = maxAmmo;
        if (firePoint == null && Camera.main != null)
            firePoint = Camera.main.transform;

        
        for (int i = 0; i < poolSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab);
            bullet.SetActive(false);
            bullet.GetComponent<Bullet>().pool = this;
            bulletPool.Enqueue(bullet);
        }
    }

    private IEnumerator Vibrate(float lowFreq, float highFreq, float duration)
    {
        if (Gamepad.current == null) yield break;

        Gamepad.current.SetMotorSpeeds(lowFreq, highFreq);
        yield return new WaitForSeconds(duration);
        Gamepad.current.SetMotorSpeeds(0, 0);
    }

    
    public void OnShoot(InputAction.CallbackContext ctx)
{
    if (ctx.performed) 
        TryShoot();
}
    public void OnReload(InputAction.CallbackContext ctx)
    {
        if (isReloading) return;

        if (currentAmmo < maxAmmo)
        {
            StartCoroutine(Reload());
        }
        else
        {
            Debug.Log("Munición completa.");
        }
    }

    private void TryShoot()
    {
        if (isReloading)
        {
            Debug.Log("No puedes disparar: recargando...");
            return;
        }

        if (currentAmmo <= 0)
        {
            Debug.Log("Sin balas. Presiona recargar.");
            return;
        }

        currentAmmo--;
        StartCoroutine(Vibrate(0.3f, 0.6f, 0.1f));

        Debug.Log($"Disparo! Balas restantes: {currentAmmo}/{maxAmmo}");

        ShootBullet();
    }

    private void ShootBullet()
    {
        if (bulletPool.Count > 0)
        {
            GameObject bullet = bulletPool.Dequeue();
            bullet.transform.position = firePoint != null ? firePoint.position : transform.position;
            bullet.transform.rotation = firePoint != null ? firePoint.rotation : transform.rotation;
            bullet.SetActive(true);

            Rigidbody rb = bullet.GetComponent<Rigidbody>();
if (rb != null)
{
    rb.linearVelocity = Vector3.zero; 
    rb.AddForce((firePoint != null ? firePoint.forward : transform.forward) * bulletSpeed, ForceMode.VelocityChange);
}
        }
        else
        {
            Debug.Log("No hay balas en el pool.");
        }
    }

    public void ReturnBullet(GameObject bullet)
    {
        bullet.SetActive(false);
        bulletPool.Enqueue(bullet);
    }

    private IEnumerator Reload()
    {
        isReloading = true;
        StartCoroutine(Vibrate(0.2f, 0.4f, 0.3f)); 
        Debug.Log("Recargando...");
        yield return new WaitForSeconds(reloadTime);
        currentAmmo = maxAmmo;
        isReloading = false;
        StartCoroutine(Vibrate(0.5f, 0.8f, 0.2f)); 
        Debug.Log($"Recarga completa. Balas: {currentAmmo}/{maxAmmo}");
    }

    public int GetCurrentAmmo() => currentAmmo;
    public bool IsReloading() => isReloading;
}
