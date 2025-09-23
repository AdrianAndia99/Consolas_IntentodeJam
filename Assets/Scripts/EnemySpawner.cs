using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")] 
    [Tooltip("Prefab del enemigo a instanciar.")] 
    [SerializeField] private GameObject enemyPrefab;

    [Tooltip("Waypoints en los que se generarán enemigos.")]
    [SerializeField] private List<int> spawnWaypointIndices = new List<int> { 1,4,8,11,13,15,16 };

    [Tooltip("Cantidad mínima de enemigos por oleada.")]
    [SerializeField] private int minPerSpawn = 1;

    [Tooltip("Cantidad máxima de enemigos por oleada.")]
    [SerializeField] private int maxPerSpawn = 3;

    [Tooltip("Si es true, sólo genera una vez por índice. Si es false, cada vez que se vuelva a pasar (loop).")]
    [SerializeField] private bool spawnOncePerIndex = true;

    [Tooltip("Radio de dispersión aleatoria alrededor del spawner.")]
    [SerializeField] private float scatterRadius = 0.5f;

    [Tooltip("Altura mínima para el punto de spawn respecto al spawner (usado si se necesita ajustar en escenarios irregulares). 0 = ignorar.")]
    [SerializeField] private float yOffset = 0f;

    [Header("Debug")] 
    [SerializeField] private bool logSpawns = true;

    private HashSet<int> spawnedIndices = new HashSet<int>();

    private void OnEnable()
    {
        PlayerControl.OnReachedWaypoint += HandleReachedWaypoint;
    }

    private void OnDisable()
    {
        PlayerControl.OnReachedWaypoint -= HandleReachedWaypoint;
    }

    private void HandleReachedWaypoint(PlayerControl pc, int index)
    {
        if (!spawnWaypointIndices.Contains(index)) return;
        if (spawnOncePerIndex && spawnedIndices.Contains(index)) return;
        if (enemyPrefab == null)
        {
            Debug.LogWarning($"EnemySpawner: No se asignó enemyPrefab en {name}.");
            return;
        }

        int count = Random.Range(minPerSpawn, maxPerSpawn + 1);
        for (int i = 0; i < count; i++)
        {
            Vector3 randomOffset = (scatterRadius > 0f) ? Random.insideUnitSphere * scatterRadius : Vector3.zero;
            randomOffset.y = 0f; // Mantener en plano horizontal
            Vector3 spawnPos = transform.position + randomOffset + Vector3.up * yOffset;
            Instantiate(enemyPrefab, spawnPos, transform.rotation);
        }

        if (logSpawns)
        {
            Debug.Log($"EnemySpawner {name}: Generados {count} enemigos en waypoint {index}.");
        }

        if (spawnOncePerIndex)
        {
            spawnedIndices.Add(index);
        }
    }

    private void OnValidate()
    {
        if (minPerSpawn < 1) minPerSpawn = 1;
        if (maxPerSpawn < minPerSpawn) maxPerSpawn = minPerSpawn;
        if (scatterRadius < 0f) scatterRadius = 0f;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f);
        Gizmos.DrawSphere(transform.position + Vector3.up * yOffset, 0.15f);
        if (scatterRadius > 0f)
        {
            Gizmos.color = new Color(1f, 0.3f, 0f, 0.15f);
            Gizmos.DrawWireSphere(transform.position + Vector3.up * yOffset, scatterRadius);
        }
    }
}
