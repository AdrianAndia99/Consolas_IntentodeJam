using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour
{
    public float lifeTime = 2f;
    public int damage = 25; 
    [HideInInspector] public PlayerShoot pool;

    private void OnEnable()
    {
        StartCoroutine(DisableAfterTime());
    }

    private IEnumerator DisableAfterTime()
    {
        yield return new WaitForSeconds(lifeTime);
        if (pool != null)
            pool.ReturnBullet(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        EnemyBehaviour enemy = collision.collider.GetComponent<EnemyBehaviour>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }

        if (pool != null)
        {
            pool.ReturnBullet(gameObject);
        }
    }
}