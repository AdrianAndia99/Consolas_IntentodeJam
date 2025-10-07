using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
    [Header("Salud")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float invulSeconds = 0.5f;
    [SerializeField] private bool clampToZero = true;

    [Header("Opcionales")]
    [SerializeField] private bool debugLog = true;

    private int currentHealth;
    private float invulTimer;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsInvulnerable => invulTimer > 0f;

    // Eventos para FX/UI
    public static event Action<int, int> OnHealthChanged; // (vidaActual, vidaMax)
    public static event Action OnDamaged;                // golpe recibido
    public static event Action OnDeath;                  // muerto

    private void Awake()
    {
        currentHealth = maxHealth;
        RaiseHealthChanged();
    }

    private void Update()
    {
        if (invulTimer > 0f) invulTimer -= Time.unscaledDeltaTime;
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;
        if (IsInvulnerable) return;

        invulTimer = invulSeconds;

        currentHealth -= amount;
        if (clampToZero && currentHealth < 0) currentHealth = 0;

        OnDamaged?.Invoke();
        RaiseHealthChanged();

        if (debugLog) Debug.Log($"PlayerHealth: daño {amount}. HP {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            OnDeath?.Invoke();
            // Aquí puedes disparar flow de derrota, panel, etc.
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        RaiseHealthChanged();
    }

    private void RaiseHealthChanged() => OnHealthChanged?.Invoke(currentHealth, maxHealth);
}
