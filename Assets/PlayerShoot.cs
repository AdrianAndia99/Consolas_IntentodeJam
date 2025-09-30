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

        InitializeBulletPool();
    }

    private void InitializeBulletPool()
    {
        if (bulletPrefab == null)
        {
            Debug.LogError("PlayerShoot: bulletPrefab no está asignado!");
            return;
        }

        bulletPool.Clear(); // Limpiar por si acaso
        
        for (int i = 0; i < poolSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab);
            bullet.SetActive(false);
            
            // Verificar que el prefab tiene el componente Bullet
            Bullet bulletComponent = bullet.GetComponent<Bullet>();
            if (bulletComponent != null)
            {
                bulletComponent.pool = this;
            }
            else
            {
                Debug.LogError($"PlayerShoot: El bulletPrefab no tiene el componente Bullet!");
                Destroy(bullet);
                continue;
            }
            
            bulletPool.Enqueue(bullet);
        }
        
        Debug.Log($"Pool de balas inicializado: {bulletPool.Count} balas disponibles.");
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
            
            // Configurar posición y rotación
            Vector3 shootPos = firePoint != null ? firePoint.position : transform.position;
            Quaternion shootRot = firePoint != null ? firePoint.rotation : transform.rotation;
            Vector3 shootDir = firePoint != null ? firePoint.forward : transform.forward;
            
            bullet.transform.position = shootPos;
            bullet.transform.rotation = shootRot;
            
            // Resetear physics antes de activar
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            
            bullet.SetActive(true);
            
            // Aplicar fuerza después de activar
            if (rb != null)
            {
                rb.AddForce(shootDir * bulletSpeed, ForceMode.Impulse);
            }
        }
        else
        {
            Debug.LogWarning("Pool de balas vacío! Creando bala temporal...");
            CreateTemporaryBullet();
        }
    }
    
    private void CreateTemporaryBullet()
    {
        if (bulletPrefab == null) return;
        
        GameObject bullet = Instantiate(bulletPrefab);
        Vector3 shootPos = firePoint != null ? firePoint.position : transform.position;
        Quaternion shootRot = firePoint != null ? firePoint.rotation : transform.rotation;
        Vector3 shootDir = firePoint != null ? firePoint.forward : transform.forward;
        
        bullet.transform.position = shootPos;
        bullet.transform.rotation = shootRot;
        
        Bullet bulletComponent = bullet.GetComponent<Bullet>();
        if (bulletComponent != null)
        {
            bulletComponent.pool = null; // No pool, se autodestruirá
        }
        
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(shootDir * bulletSpeed, ForceMode.Impulse);
        }
        
        // Autodestruir después del lifetime
        if (bulletComponent != null)
        {
            Destroy(bullet, bulletComponent.lifeTime);
        }
        else
        {
            Destroy(bullet, 3f); // Fallback
        }
    }

    public void ReturnBullet(GameObject bullet)
    {
        if (bullet == null) return;
        
        // Verificar que no esté ya en el pool
        if (!bullet.activeInHierarchy)
        {
            return; // Ya está desactivada, probablemente ya en el pool
        }
        
        // Resetear physics antes de desactivar
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        bullet.SetActive(false);
        bulletPool.Enqueue(bullet);
        
        Debug.Log($"Bala retornada al pool. Disponibles: {bulletPool.Count}");
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
