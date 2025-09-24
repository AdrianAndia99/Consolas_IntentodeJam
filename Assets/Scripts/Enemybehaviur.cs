using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    [Header("Stats")]
    [Tooltip("Vida inicial del enemigo.")]
    [SerializeField] private int health = 100;

    [Header("Movement")]
    [Tooltip("Velocidad de movimiento del enemigo al perseguir.")]
    [SerializeField] private float moveSpeed = 2.5f;
    [Tooltip("Velocidad de rotaci�n para encarar al jugador.")]
    [SerializeField] private float rotateSpeed = 10f;

    [Header("Player Detection")]
    [Tooltip("Rango en el que el enemigo puede detectar al jugador.")]
    [SerializeField] private float detectionRadius = 10f;
    [Tooltip("�ngulo de visi�n del enemigo (en grados).")]
    [SerializeField][Range(0, 360)] private float fieldOfViewAngle = 120f;
    [Tooltip("La m�scara de capas para los obst�culos que pueden bloquear la l�nea de visi�n.")]
    [SerializeField] private LayerMask obstacleMask;

    [Header("Animation")]
    [Tooltip("Tiempo a esperar despu�s de la animaci�n de muerte antes de destruir el objeto.")]
    [SerializeField] private float deathDestroyDelay = 2f;

    private Transform playerTarget;
    private bool isChasing = false;
    private bool isAlive = true;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError($"EnemyBehaviour: No se encontr� un componente Animator en {name}.");
        }
    }

    public void Initialize(Transform player)
    {
        playerTarget = player;
    }

    private void Update()
    {
        // Si el enemigo no est� vivo o no tiene un objetivo, no hace nada.
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

    private void DetectPlayer()
    {
        // Comprueba si el jugador est� dentro del rango y l�nea de visi�n.
        if (Vector3.Distance(transform.position, playerTarget.position) > detectionRadius) return;
        Vector3 directionToPlayer = (playerTarget.position - transform.position).normalized;
        if (Vector3.Angle(transform.forward, directionToPlayer) > fieldOfViewAngle / 2) return;
        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);
        if (Physics.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleMask)) return;

        // Si todas las condiciones se cumplen, comienza la persecuci�n.
        isChasing = true;
        animator.SetBool("isChasing", true);
    }

    private void ChasePlayer()
    {
        Vector3 direction = (playerTarget.position - transform.position).normalized;
        direction.y = 0; // Ignorar la altura para el movimiento en el plano.
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
        animator.SetTrigger("takeDamage");

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isAlive = false;
        animator.SetBool("isChasing", false); // Detiene la animaci�n de correr
        animator.SetTrigger("die"); // Activa la animaci�n de muerte

        GetComponent<Collider>().enabled = false; // Desactiva el collider para no interactuar m�s
        this.enabled = false; // Desactiva este script para detener el Update()

        // Destruye el objeto despu�s de un tiempo para dar lugar a la animaci�n de muerte.
        Destroy(gameObject, deathDestroyDelay);
    }

    private void OnDrawGizmosSelected()
    {
        // Dibuja las ayudas visuales en el editor de Unity.
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Vector3 forward = transform.forward;
        Vector3 coneDirectionLeft = Quaternion.Euler(0, -fieldOfViewAngle / 2, 0) * forward;
        Vector3 coneDirectionRight = Quaternion.Euler(0, fieldOfViewAngle / 2, 0) * forward;
        Gizmos.DrawLine(transform.position, transform.position + coneDirectionLeft * detectionRadius);
        Gizmos.DrawLine(transform.position, transform.position + coneDirectionRight * detectionRadius);
    }
}