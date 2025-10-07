using UnityEngine;
using System.Collections;

public class EnemyBehaviour : MonoBehaviour
{
    [Header("Stats")]
    [Tooltip("Vida inicial del enemigo.")]
    // CAMBIO: 'private' ahora es 'protected' para que las clases hijas puedan acceder.
    [SerializeField] protected int initialHealth = 100;
    [SerializeField] protected int pointsOnDeath = 10;

    [Header("Movement")]
    [Tooltip("Velocidad de movimiento del enemigo al perseguir.")]
    // CAMBIO: 'private' ahora es 'protected'.
    [SerializeField] protected float moveSpeed = 2.5f;
    [Tooltip("Velocidad de rotación para encarar al jugador.")]
    [SerializeField] protected float rotateSpeed = 10f;

    // ... EL RESTO DEL SCRIPT NO NECESITA CAMBIOS ...
    // Las variables de abajo pueden seguir siendo 'private' si no necesitas modificarlas.
    [Header("Player Detection")]
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] [Range(0, 360)] private float fieldOfViewAngle = 120f;
    [SerializeField] private LayerMask obstacleMask;

    [Header("Animation")]
    [SerializeField] private float deathDestroyDelay = 2f;

    private Transform playerTarget;
    private bool isChasing = false;
    private bool isAlive = true;
    private Animator animator;
    private Collider enemyCollider;
    protected int currentHealth; // <-- CAMBIO: También a 'protected' por si quieres lógica especial de vida.

    // El resto de los métodos (Awake, OnEnable, TakeDamage, Die, etc.) se quedan exactamente igual.
    // Unity llamará al 'Awake' de la clase base automáticamente.
    protected virtual void Awake() // <-- CAMBIO OPCIONAL: Hacerlo 'protected virtual' es una buena práctica.
    {
        animator = GetComponent<Animator>();
        enemyCollider = GetComponent<Collider>();
        if (animator == null)
        {
            Debug.LogError($"EnemyBehaviour: No se encontró un componente Animator en {name}.");
        }
    }

    protected virtual void OnEnable() // <-- CAMBIO OPCIONAL: También hacerlo 'protected virtual'.
    {
        ResetEnemyState();
    }

    // ... no es necesario pegar el resto del script, los métodos no cambian.
    public void Initialize(Transform player)
    {
        playerTarget = player;
    }

    private void Update()
    {
        if (!isAlive || playerTarget == null) return;

        if (isChasing)
        {
            ChasePlayer();
        }
        else
        {
            DetectPlayer();
        }
    }

    protected void ResetEnemyState()
    {
        currentHealth = initialHealth;
        isAlive = true;
        isChasing = false;

        if (enemyCollider != null)
        {
            enemyCollider.enabled = true;
        }

        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }
    }

    private void DetectPlayer()
    {
        if (Vector3.Distance(transform.position, playerTarget.position) > detectionRadius) return;
        Vector3 directionToPlayer = (playerTarget.position - transform.position).normalized;
        if (Vector3.Angle(transform.forward, directionToPlayer) > fieldOfViewAngle / 2) return;
        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);
        if (Physics.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleMask)) return;

        isChasing = true;
        animator.SetBool("isChasing", true);
    }

    private void ChasePlayer()
    {
        Vector3 direction = (playerTarget.position - transform.position).normalized;
        direction.y = 0;
        transform.position += direction * moveSpeed * Time.deltaTime;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (!isAlive) return;

        currentHealth -= damageAmount;
        animator.SetTrigger("takeDamage");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isAlive = false;
        animator.SetBool("isChasing", false);
        animator.SetTrigger("die");

        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(pointsOnDeath);
        }
        StartCoroutine(DeactivateAfterAnimation());
    }
    private IEnumerator DeactivateAfterAnimation()
    {
        yield return new WaitForSeconds(deathDestroyDelay);
        gameObject.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Vector3 forward = transform.forward;
        Vector3 coneDirectionLeft = Quaternion.Euler(0, -fieldOfViewAngle / 2, 0) * forward;
        Vector3 coneDirectionRight = Quaternion.Euler(0, fieldOfViewAngle / 2, 0) * forward;
        Gizmos.DrawLine(transform.position, transform.position + coneDirectionLeft * detectionRadius);
        Gizmos.DrawLine(transform.position, transform.position + coneDirectionRight * detectionRadius);
    }
}