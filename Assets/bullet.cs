using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour
{
    public float lifeTime = 2f;
    public int damage = 25; 
    [HideInInspector] public PlayerShoot pool;

    private Coroutine lifetimeCoroutine;
    private bool hasCollided = false;

    private void OnEnable()
    {
        hasCollided = false;
        
        // Detener corrutina anterior si existe
        if (lifetimeCoroutine != null)
        {
            StopCoroutine(lifetimeCoroutine);
        }
        
        lifetimeCoroutine = StartCoroutine(DisableAfterTime());
    }

    private void OnDisable()
    {
        // Limpiar corrutina al desactivar
        if (lifetimeCoroutine != null)
        {
            StopCoroutine(lifetimeCoroutine);
            lifetimeCoroutine = null;
        }
    }

    private IEnumerator DisableAfterTime()
    {
        yield return new WaitForSeconds(lifeTime);
        ReturnToPool();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Evitar múltiples colisiones
        if (hasCollided) return;
        hasCollided = true;

        // Intentar dañar enemigo
        EnemyBehaviour enemy = collision.collider.GetComponent<EnemyBehaviour>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }

        ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (pool != null)
        {
            pool.ReturnBullet(gameObject);
        }
        else
        {
            // Si no hay pool (bala temporal), autodestruirse
            Destroy(gameObject);
        }
    }
}