using UnityEngine;
using System.Collections.Generic;

public class EnemyONLYmenu : MonoBehaviour
{
    [Header("Waypoints / Ruta")]
    [Tooltip("Lista de puntos que define el recorrido de la rana.")]
    [SerializeField] private List<Transform> waypoints = new List<Transform>();

    [Tooltip("Colocarse directamente en el primer waypoint al iniciar.")]
    [SerializeField] private bool snapToFirstPoint = true;

    [Header("Movimiento")]
    [Tooltip("Velocidad de movimiento en unidades/seg.")]
    [SerializeField] private float moveSpeed = 3f;

    [Tooltip("Velocidad de rotación para mirar hacia el siguiente punto.")]
    [SerializeField] private float rotateSpeed = 8f;

    [Tooltip("Distancia mínima para considerar que alcanzó un waypoint.")]
    [SerializeField] private float arriveThreshold = 0.05f;

    [Header("Respawn / Bucle")]
    [Tooltip("Prefab de la rana para crear una nueva instancia al terminar la ruta.")]
    [SerializeField] private GameObject frogPrefab;

    [Tooltip("Si está activo, destruye esta rana y crea otra al inicio al terminar.")]
    [SerializeField] private bool respawnOnFinish = true;

    [Header("Animación")]
    [Tooltip("Nombre del parámetro bool en el Animator para 'perseguir'.")]
    [SerializeField] private string chasingBoolName = "isChasing";

    private int currentIndex = 0;
    private bool pathCompleted = false;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (waypoints == null || waypoints.Count == 0)
        {
            Debug.LogWarning("EnemyONLYmenu: No hay waypoints asignados.");
            pathCompleted = true;
            return;
        }

        if (snapToFirstPoint && waypoints[0] != null)
        {
            transform.position = waypoints[0].position;
            transform.rotation = waypoints[0].rotation;
        }
    }

    private void Update()
    {
        if (pathCompleted) return;
        if (waypoints == null || waypoints.Count == 0) return;
        if (currentIndex >= waypoints.Count) return;

        // Forzar animación de persecución siempre activa
        if (animator != null && !string.IsNullOrEmpty(chasingBoolName))
        {
            animator.SetBool(chasingBoolName, true);
        }

        Transform target = waypoints[currentIndex];
        if (target == null)
        {
            currentIndex++;
            return;
        }

        Vector3 toTarget = target.position - transform.position;
        float sqrDist = toTarget.sqrMagnitude;
        float thresholdSqr = arriveThreshold * arriveThreshold;

        if (sqrDist <= thresholdSqr)
        {
            transform.position = target.position; // Ajustar posición exacta
            currentIndex++;
            if (currentIndex >= waypoints.Count)
            {
                HandlePathFinished();
            }
            return;
        }

        // Movimiento
        float step = moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target.position, step);

        // Rotación hacia la dirección de movimiento
        if (toTarget.sqrMagnitude > 0.0001f)
        {
            Quaternion desired = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, desired, rotateSpeed * Time.deltaTime);
        }
    }

    private void HandlePathFinished()
    {
        pathCompleted = true;
        if (respawnOnFinish && frogPrefab != null && waypoints != null && waypoints.Count > 0 && waypoints[0] != null)
        {
            // Instanciar nueva rana en el primer punto
            Transform first = waypoints[0];
            GameObject newFrog = Instantiate(frogPrefab, first.position, first.rotation);
            // Copiar lista de waypoints al nuevo componente si existe
            EnemyONLYmenu newMenu = newFrog.GetComponent<EnemyONLYmenu>();
            if (newMenu != null)
            {
                newMenu.SetWaypoints(waypoints);
            }
        }
        // Destruir esta instancia
        Destroy(gameObject);
    }

    public void SetWaypoints(List<Transform> newWaypoints)
    {
        waypoints = newWaypoints;
    }

    private void OnValidate()
    {
        if (arriveThreshold < 0.001f) arriveThreshold = 0.001f;
        if (moveSpeed < 0f) moveSpeed = 0f;
        if (rotateSpeed < 0f) rotateSpeed = 0f;
    }

    private void OnDrawGizmosSelected()
    {
        if (waypoints == null || waypoints.Count == 0) return;
        Gizmos.color = Color.green;
        for (int i = 0; i < waypoints.Count; i++)
        {
            var wp = waypoints[i];
            if (wp == null) continue;
            Gizmos.DrawWireSphere(wp.position, 0.15f);
            if (i + 1 < waypoints.Count && waypoints[i + 1] != null)
            {
                Gizmos.DrawLine(wp.position, waypoints[i + 1].position);
            }
        }

        if (Application.isPlaying && currentIndex < waypoints.Count && currentIndex >= 0)
        {
            var current = waypoints[Mathf.Clamp(currentIndex,0,waypoints.Count-1)];
            if (current != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(current.position, arriveThreshold);
            }
        }
    }
}
