using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DamageOnContact : MonoBehaviour
{
    [Header("Daño")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float hitCooldown = 0.7f;
    [SerializeField] private bool onlyOnTrigger = true;

    private float nextAllowedTime;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true; // recomendado
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!onlyOnTrigger) return;
        TryDamage(other.gameObject);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!onlyOnTrigger) return;
        TryDamage(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (onlyOnTrigger) return;
        TryDamage(collision.gameObject);
    }

    private void TryDamage(GameObject other)
    {
        if (Time.time < nextAllowedTime) return;

        var ph = other.GetComponent<PlayerHealth>();
        if (ph == null) return;

        ph.TakeDamage(damage);
        nextAllowedTime = Time.time + hitCooldown;
    }
}
