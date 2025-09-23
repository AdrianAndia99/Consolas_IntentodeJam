using UnityEngine;
using System.Collections.Generic;

// Clase auxiliar para organizar la información de cada oleada en el Inspector.
[System.Serializable]
public class SpawnWave
{
    [Tooltip("Nombre de la oleada para identificarla en el Inspector.")]
    public string waveName;

    [Tooltip("El índice del waypoint del jugador que activará esta oleada.")]
    public int triggerWaypointIndex;

    [Tooltip("Los puntos en el mundo donde aparecerán los enemigos de esta oleada.")]
    public List<Transform> spawnPoints;

    [Tooltip("Cantidad mínima de enemigos que aparecerán EN CADA PUNTO de spawn.")]
    [Min(1)] public int minEnemiesPerPoint = 1;

    [Tooltip("Cantidad máxima de enemigos que aparecerán EN CADA PUNTO de spawn.")]
    [Min(1)] public int maxEnemiesPerPoint = 3;
}


public class EnemySpawner : MonoBehaviour
{
    [Header("Configuración General")]
    [Tooltip("Prefab del enemigo a instanciar.")]
    [SerializeField] private GameObject enemyPrefab;

    [Tooltip("Lista de todas las oleadas de enemigos que ocurrirán en la escena.")]
    [SerializeField] private List<SpawnWave> spawnWaves = new List<SpawnWave>();

    [Tooltip("Si es true, cada oleada sólo se activará una vez por partida.")]
    [SerializeField] private bool spawnOncePerWave = true;

    [Tooltip("Radio de dispersión aleatoria alrededor de cada punto de spawn.")]
    [SerializeField] private float scatterRadius = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool logSpawns = true;

    private HashSet<int> triggeredWaypointIndices = new HashSet<int>();
    private Transform playerTransform;

    private void Awake()
    {
        // Busca al jugador UNA SOLA VEZ al inicio de la escena.
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
        else
        {
            Debug.LogError("EnemySpawner: No se pudo encontrar al jugador. Asegúrate de que tenga el tag 'Player'.");
        }
    }

    private void OnEnable()
    {
        // Se suscribe al evento del jugador para saber cuándo llega a un waypoint.
        PlayerControl.OnReachedWaypoint += HandleReachedWaypoint;
    }

    private void OnDisable()
    {
        // Se desuscribe del evento para evitar errores al cambiar de escena.
        PlayerControl.OnReachedWaypoint -= HandleReachedWaypoint;
    }

    private void HandleReachedWaypoint(PlayerControl pc, int index)
    {
        // Busca si alguna oleada corresponde al waypoint actual.
        SpawnWave waveToTrigger = spawnWaves.Find(wave => wave.triggerWaypointIndex == index);

        if (waveToTrigger == null) return;
        if (spawnOncePerWave && triggeredWaypointIndices.Contains(index)) return;
        if (enemyPrefab == null || playerTransform == null) return;

        if (logSpawns)
        {
            Debug.Log($"<color=orange>Activando oleada '{waveToTrigger.waveName}' en waypoint {index}.</color>");
        }

        // Procesa cada punto de spawn definido en la oleada.
        foreach (Transform spawnPoint in waveToTrigger.spawnPoints)
        {
            int enemyCount = Random.Range(waveToTrigger.minEnemiesPerPoint, waveToTrigger.maxEnemiesPerPoint + 1);

            for (int i = 0; i < enemyCount; i++)
            {
                Vector3 randomOffset = (scatterRadius > 0f) ? Random.insideUnitSphere * scatterRadius : Vector3.zero;
                randomOffset.y = 0;
                Vector3 spawnPos = spawnPoint.position + randomOffset;

                GameObject enemyInstance = Instantiate(enemyPrefab, spawnPos, spawnPoint.rotation);

                // Inyección de dependencias: Pasa la referencia del jugador al nuevo enemigo.
                EnemyBehaviour enemyBehaviour = enemyInstance.GetComponent<EnemyBehaviour>();
                if (enemyBehaviour != null)
                {
                    enemyBehaviour.Initialize(playerTransform);
                }
            }
        }

        if (spawnOncePerWave)
        {
            triggeredWaypointIndices.Add(index);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Dibuja ayudas visuales en el editor para facilitar la configuración.
        if (spawnWaves == null) return;

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f);
        Gizmos.DrawCube(transform.position, Vector3.one * 0.3f);

        foreach (var wave in spawnWaves)
        {
            if (wave.spawnPoints == null) continue;
            foreach (var point in wave.spawnPoints)
            {
                if (point == null) continue;
                Gizmos.color = new Color(1f, 0.3f, 0f, 0.5f);
                Gizmos.DrawSphere(point.position, 0.25f);
                Gizmos.DrawLine(transform.position, point.position);

                if (scatterRadius > 0f)
                {
                    Gizmos.color = new Color(1f, 0.3f, 0f, 0.15f);
                    Gizmos.DrawWireSphere(point.position, scatterRadius);
                }
            }
        }
    }
}
