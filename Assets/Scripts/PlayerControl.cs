using UnityEngine;
using System;
using System.Collections.Generic;

public class PlayerControl : MonoBehaviour
{
    [Header("Waypoints / Rail")]
    [Tooltip("Lista de puntos que definen el recorrido del jugador (rail).")]
    [SerializeField] private List<Transform> waypoints = new List<Transform>();

    [Tooltip("Velocidad lineal de movimiento en unidades por segundo.")]
    [SerializeField] private float moveSpeed = 3f;

    [Tooltip("Velocidad de rotación para orientar al siguiente punto.")]
    [SerializeField] private float rotateSpeed = 8f;

    [Tooltip("El jugador comienza en el primer waypoint al iniciar.")]
    [SerializeField] private bool snapToFirstPoint = true;

    [Tooltip("Avanza automáticamente al siguiente punto tras llegar al actual.")]
    [SerializeField] private bool autoAdvance = true;

    [Tooltip("Si está desactivado el avance automático, se debe llamar a Advance() tras llegar a un punto.")]
    [SerializeField] private bool requireManualAdvance = false;

    [Tooltip("Volver al inicio al terminar la ruta.")]
    [SerializeField] private bool loopPath = false;

    [Tooltip("Rotar mirando hacia el siguiente punto mientras se mueve.")]
    [SerializeField] private bool lookAtNextPoint = true;

    [Tooltip("Distancia mínima para considerar que se alcanzó un waypoint.")]
    [SerializeField] private float arriveThreshold = 0.05f;

    // Eventos estáticos globales: cualquier script puede suscribirse sin referencia directa al PlayerControl
    public static event Action<PlayerControl> OnPathCompleted;              // Se invoca cuando se termina el recorrido (y no hay loop)
    public static event Action<PlayerControl, int> OnReachedWaypoint;       // (instancia, índice actual) al alcanzarlo (incluye el último)

    // Estado interno
    private int currentIndex = 0;          // Waypoint objetivo actual
    private bool moving = false;           // Sólo relevante en modo manual
    private bool pathCompleted = false;

    // Propiedades públicas de sólo lectura
    public bool PathCompleted => pathCompleted;
    public int CurrentIndex => currentIndex;
    public Transform CurrentWaypoint => (waypoints != null && currentIndex < waypoints.Count && currentIndex >= 0) ? waypoints[currentIndex] : null;

    private void Start()
    {
    if (waypoints == null || waypoints.Count == 0)
        {
            Debug.LogWarning("PlayerControl: No hay waypoints asignados.");
            return;
        }

    if (snapToFirstPoint && waypoints.Count > 0 && waypoints[0] != null)
        {
            transform.position = waypoints[0].position;
            transform.rotation = waypoints[0].rotation; // Opcional: alinear rotación inicial
        }

        // Configurar flags de acuerdo a las opciones
        if (autoAdvance) requireManualAdvance = false; // Auto y manual a la vez no tiene sentido

        // Iniciar movimiento
        if (autoAdvance)
            moving = true;
    }

    private void Update()
    {
        if (pathCompleted) return;
    if (waypoints == null || waypoints.Count == 0) return;
    if (currentIndex >= waypoints.Count) return;
        if (!autoAdvance && requireManualAdvance && !moving) return; // Esperar llamada a Advance()

    var target = waypoints[currentIndex];
        if (target == null)
        {
            Debug.LogWarning($"PlayerControl: Waypoint en índice {currentIndex} es null.");
            AvanzarIndice(); // Saltar null para evitar bloqueo
            return;
        }

        Vector3 toTarget = target.position - transform.position;
        float sqrDist = toTarget.sqrMagnitude;

        // Comprobar llegada
        if (sqrDist <= arriveThreshold * arriveThreshold)
        {
            // Asegurar posición exacta (opcional)
            transform.position = target.position;
            OnReachedWaypoint?.Invoke(this, currentIndex);
            AvanzarIndice();
            return; // Esperar siguiente frame para iniciar movimiento al nuevo objetivo
        }

        // Movimiento lineal
        float step = moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target.position, step);

        // Rotación suave
        if (lookAtNextPoint && toTarget.sqrMagnitude > 0.0001f)
        {
            Quaternion desired = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, desired, rotateSpeed * Time.deltaTime);
        }
    }

    // Avanza el índice al siguiente waypoint y gestiona fin de recorrido
    private void AvanzarIndice()
    {
        currentIndex++;
        moving = autoAdvance; // En automático seguimos, en manual detenemos y esperamos Advance()

        if (currentIndex >= waypoints.Count)
        {
            OnPathCompleted?.Invoke(this);
            if (loopPath && waypoints.Count > 0)
            {
                currentIndex = 0;
                pathCompleted = false;
                moving = autoAdvance; // Reiniciar
            }
            else
            {
                pathCompleted = true;
                moving = false;
            }
        }
        else if (!autoAdvance && requireManualAdvance)
        {
            moving = false; // Esperar input manual para continuar al nuevo waypoint
        }
    }

    /// <summary>
    /// Llamar en modo manual (autoAdvance = false, requireManualAdvance = true) para iniciar movimiento al siguiente punto.
    /// </summary>
    public void Advance()
    {
        if (autoAdvance || !requireManualAdvance) return; // No aplica
        if (pathCompleted) return;
        moving = true;
    }

    /// <summary>
    /// Asigna dinámicamente una nueva lista de waypoints y reinicia el recorrido.
    /// </summary>
    public void SetWaypoints(List<Transform> newWaypoints, bool restart = true)
    {
        waypoints = newWaypoints;
        if (restart)
        {
            currentIndex = 0;
            pathCompleted = false;
            moving = autoAdvance;
            if (snapToFirstPoint && waypoints != null && waypoints.Count > 0 && waypoints[0] != null)
            {
                transform.position = waypoints[0].position;
                transform.rotation = waypoints[0].rotation;
            }
        }
    }

    private void OnValidate()
    {
        if (arriveThreshold < 0.001f) arriveThreshold = 0.001f;
        if (moveSpeed < 0f) moveSpeed = 0f;
        if (rotateSpeed < 0f) rotateSpeed = 0f;
        if (autoAdvance) requireManualAdvance = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (waypoints == null || waypoints.Count == 0) return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Count; i++)
        {
            if (waypoints[i] == null) continue;
            Gizmos.DrawWireSphere(waypoints[i].position, 0.15f);
            if (i + 1 < waypoints.Count && waypoints[i + 1] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }
        }

        // Esfera de umbral en el waypoint actual en modo de juego
        if (Application.isPlaying && CurrentWaypoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(CurrentWaypoint.position, arriveThreshold);
        }
    }
}