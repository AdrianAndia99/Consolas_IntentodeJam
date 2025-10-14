using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System; // <--- AÑADIDO

public class PlayerShoot : MonoBehaviour
{
    [Header("Munición")]
    public int maxAmmo = 6;
    public int currentAmmo;

    [Header("Arma / Proyectil")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 30f;

    [Header("Puntero UI")]
    [Tooltip("Referencia al RectTransform del puntero de disparo en el Canvas")]
    public RectTransform pointerUI;

    [Header("Pooling")]
    public int poolSize = 10;
    private Queue<GameObject> bulletPool = new Queue<GameObject>();

    [Header("Recarga")]
    public float reloadTime = 1f;
    private bool isReloading = false;

    // --- NUEVOS EVENTOS PARA LA UI ---
    public static event Action<int, int> OnAmmoChanged; // Envía (munición actual, munición máxima)
    public static event Action OnReloadStart;
    public static event Action OnReloadFinish;
    // ----------------------------------

    private void Awake()
    {
        currentAmmo = maxAmmo;
        if (firePoint == null && Camera.main != null)
            firePoint = Camera.main.transform;

        InitializeBulletPool();
    }

    // AÑADIDO: Notifica a la UI del estado inicial al activarse.
    private void OnEnable()
    {
        OnAmmoChanged?.Invoke(currentAmmo, maxAmmo);
    }

    private void InitializeBulletPool()
    {
        if (bulletPrefab == null)
        {
            Debug.LogError("PlayerShoot: bulletPrefab no está asignado!");
            return;
        }

        bulletPool.Clear();

        for (int i = 0; i < poolSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab);
            bullet.SetActive(false);

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
        // MODIFICADO: Añadido .performed para asegurar que se active solo una vez.
        if (ctx.performed && currentAmmo < maxAmmo && !isReloading)
        {
            StartCoroutine(Reload());
        }
        else if (ctx.performed && currentAmmo >= maxAmmo)
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
        OnAmmoChanged?.Invoke(currentAmmo, maxAmmo); // AÑADIDO: Notifica el cambio de munición
        StartCoroutine(Vibrate(0.3f, 0.6f, 0.1f));

        Debug.Log($"Disparo! Balas restantes: {currentAmmo}/{maxAmmo}");

        ShootBullet();
    }

    private void GetShootDirectionFromPointer(out Vector3 shootPosition, out Vector3 shootDirection)
    {
        Camera mainCam = Camera.main;

        shootPosition = firePoint != null ? firePoint.position : (mainCam != null ? mainCam.transform.position : transform.position);

        if (pointerUI != null && mainCam != null)
        {
            Vector2 pointerScreenPos = RectTransformUtility.WorldToScreenPoint(null, pointerUI.position);
            Ray ray = mainCam.ScreenPointToRay(pointerScreenPos);

            RaycastHit hit;
            Vector3 targetPoint;

            if (Physics.Raycast(ray, out hit, 1000f))
            {
                targetPoint = hit.point;
            }
            else
            {
                targetPoint = ray.GetPoint(1000f);
            }

            shootDirection = (targetPoint - shootPosition).normalized;
        }
        else
        {
            if (firePoint != null)
                shootDirection = firePoint.forward;
            else if (mainCam != null)
                shootDirection = mainCam.transform.forward;
            else
                shootDirection = transform.forward;

            Debug.LogWarning("PlayerShoot: No se encontró pointerUI o Camera.main. Usando dirección por defecto.");
        }
    }

    private void ShootBullet()
    {
        if (bulletPool.Count > 0)
        {
            GameObject bullet = bulletPool.Dequeue();

            Vector3 shootPos;
            Vector3 shootDir;
            GetShootDirectionFromPointer(out shootPos, out shootDir);

            bullet.transform.position = shootPos;
            bullet.transform.rotation = Quaternion.LookRotation(shootDir);

            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            bullet.SetActive(true);

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

        Vector3 shootPos;
        Vector3 shootDir;
        GetShootDirectionFromPointer(out shootPos, out shootDir);

        bullet.transform.position = shootPos;
        bullet.transform.rotation = Quaternion.LookRotation(shootDir);

        Bullet bulletComponent = bullet.GetComponent<Bullet>();
        if (bulletComponent != null)
        {
            bulletComponent.pool = null;
        }

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(shootDir * bulletSpeed, ForceMode.Impulse);
        }

        if (bulletComponent != null)
        {
            Destroy(bullet, bulletComponent.lifeTime);
        }
        else
        {
            Destroy(bullet, 3f);
        }
    }

    public void ReturnBullet(GameObject bullet)
    {
        if (bullet == null) return;

        if (!bullet.activeInHierarchy)
        {
            return;
        }

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
        OnReloadStart?.Invoke(); // AÑADIDO: Notifica que la recarga empieza
        StartCoroutine(Vibrate(0.2f, 0.4f, 0.3f));
        Debug.Log("Recargando...");

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;
        isReloading = false;
        OnAmmoChanged?.Invoke(currentAmmo, maxAmmo); // AÑADIDO: Notifica el cambio de munición
        OnReloadFinish?.Invoke(); // AÑADIDO: Notifica que la recarga termina
        StartCoroutine(Vibrate(0.5f, 0.8f, 0.2f));
        Debug.Log($"Recarga completa. Balas: {currentAmmo}/{maxAmmo}");
    }

    public int GetCurrentAmmo() => currentAmmo;
    public bool IsReloading() => isReloading;
}