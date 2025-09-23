using UnityEngine;
using DG.Tweening; // Aún podemos usarlo si queremos, aunque la animación de muerte es mejor

public class EnemyBehaviour : MonoBehaviour
{
    [Header("Stats")]
    [Tooltip("Vida inicial del enemigo.")]
    [SerializeField] private int health = 100;

    [Header("Movement")]
    [Tooltip("Velocidad de movimiento del enemigo al perseguir.")]
    [SerializeField] private float moveSpeed = 2.5f;
    [Tooltip("Velocidad de rotación para encarar al jugador.")]
    [SerializeField] private float rotateSpeed = 10f;

    [Header("Player Detection")]
    [Tooltip("Rango en el que el enemigo puede detectar al jugador.")]
    [SerializeField] private float detectionRadius = 10f;
    [Tooltip("Ángulo de visión del enemigo (en grados).")]
    [SerializeField][Range(0, 360)] private float fieldOfViewAngle = 120f;
    [Tooltip("La máscara de capas para los obstáculos que pueden bloquear la línea de visión.")]
    [SerializeField] private LayerMask obstacleMask;

    [Header("Animation")] // NUEVO
    [Tooltip("Tiempo a esperar después de la animación de muerte antes de destruir el objeto.")] // NUEVO
    [SerializeField] private float deathDestroyDelay = 2f; // NUEVO

    // Estado interno
    private Transform playerTarget;
    private bool isChasing = false;
    private bool isAlive = true;
    private Animator animator; // NUEVO: Referencia al componente Animator

    private void Awake() // MODIFICADO: Usamos Awake para asegurar que el animator esté listo
    {
        // Obtenemos el componente Animator que debe estar en el mismo GameObject.
        animator = GetComponent<Animator>(); // NUEVO
        if (animator == null) // NUEVO
        {
            Debug.LogError($"EnemyBehaviour: No se encontró un componente Animator en {name}."); // NUEVO
        }
    }

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTarget = playerObject.transform;
        }
        else
        {
            Debug.LogWarning("EnemyBehaviour: No se encontró ningún objeto con el tag 'Player'.");
        }
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
        if (health <= 0)
        {
            Die();
        }
    }

    private void DetectPlayer()
    {
        if (Vector3.Distance(transform.position, playerTarget.position) > detectionRadius) return;
        Vector3 directionToPlayer = (playerTarget.position - transform.position).normalized;
        if (Vector3.Angle(transform.forward, directionToPlayer) > fieldOfViewAngle / 2) return;
        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);
        if (Physics.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleMask)) return;

        Debug.Log($"{name} ha detectado al jugador!");
        isChasing = true;
        animator.SetBool("isChasing", true); // NUEVO: Activa la animación de correr
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

        health -= damageAmount;
        Debug.Log($"{name} recibió {damageAmount} de daño. Vida restante: {health}");

        animator.SetTrigger("takeDamage"); // NUEVO: Dispara la animación de recibir daño

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isAlive = false;
        Debug.Log($"{name} ha muerto.");

        animator.SetBool("isChasing", false); // MODIFICADO: Deja de correr
        animator.SetTrigger("die"); // NUEVO: Dispara la animación de muerte

        GetComponent<Collider>().enabled = false;
        this.enabled = false; // Desactiva este script para que no siga ejecutando Update()

        // MODIFICADO: En lugar de usar DOTween, esperamos a que la animación termine y luego destruimos el objeto.
        Destroy(gameObject, deathDestroyDelay);
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