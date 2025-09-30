using UnityEngine;
using System.Collections; // <-- CAMBIO: Necesario para corutinas.

public class EnemyBehaviour : MonoBehaviour
{
    [Header("Stats")]
    [Tooltip("Vida inicial del enemigo.")]
    [SerializeField] private int initialHealth = 100; // <-- CAMBIO: Renombrado para claridad.

    [Header("Movement")]
    [Tooltip("Velocidad de movimiento del enemigo al perseguir.")]
    [SerializeField] private float moveSpeed = 2.5f;
    [Tooltip("Velocidad de rotación para encarar al jugador.")]
    [SerializeField] private float rotateSpeed = 10f;

    [Header("Player Detection")]
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField][Range(0, 360)] private float fieldOfViewAngle = 120f;
    [SerializeField] private LayerMask obstacleMask;

    [Header("Animation")]
    [SerializeField] private float deathDestroyDelay = 2f;

    private Transform playerTarget;
    private bool isChasing = false;
    private bool isAlive = true;
    private Animator animator;
    private Collider enemyCollider; // <-- CAMBIO: Referencia al collider.
    private int currentHealth; // <-- CAMBIO: Vida actual separada de la inicial.

    private void Awake()
    {
        animator = GetComponent<Animator>();
        enemyCollider = GetComponent<Collider>(); // <-- CAMBIO: Obtenemos el collider.
        if (animator == null)
        {
            Debug.LogError($"EnemyBehaviour: No se encontró un componente Animator en {name}.");
        }
    }

    // <-- CAMBIO: OnEnable se ejecuta cada vez que el objeto es activado.
    // Ideal para reiniciar el estado del enemigo.
    private void OnEnable()
    {
        ResetEnemyState();
    }

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

    private void ResetEnemyState()
    {
        currentHealth = initialHealth;
        isAlive = true;
        isChasing = false;

        if (enemyCollider != null)
        {
            enemyCollider.enabled = true;
        }

        // Asegura que las animaciones se reinicien correctamente.
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

        // <-- CAMBIO: Reemplazamos Destroy por una corutina que desactiva el objeto.
        StartCoroutine(DeactivateAfterAnimation());
    }

    // <-- CAMBIO: Nueva corutina para devolver el objeto al pool después de la animación.
    private IEnumerator DeactivateAfterAnimation()
    {
        yield return new WaitForSeconds(deathDestroyDelay);
        gameObject.SetActive(false); // Devuelve el objeto al pool.
    }

    // El método OnDrawGizmosSelected no necesita cambios.
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