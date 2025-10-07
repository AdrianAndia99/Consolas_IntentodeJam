using UnityEngine;
using System.Collections.Generic;

// La clase SpawnWave no necesita cambios.
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
    [SerializeField] private List<ObjectPooler> variantPools; // Asigna aquí los poolers de cada variante
    [SerializeField] private List<float> variantWeights;
    [SerializeField] private bool useSingletonAsFallback = true;
    [Tooltip("Lista de todas las oleadas de enemigos que ocurrirán en la escena.")]
    [SerializeField] private List<SpawnWave> spawnWaves = new List<SpawnWave>();

    [Tooltip("Si es true, cada oleada sólo se activará una vez por partida.")]
    [SerializeField] private bool spawnOncePerWave = true;

    [Tooltip("Radio de dispersión aleatoria alrededor de cada punto de spawn.")]
    [SerializeField] private float scatterRadius = 0.5f;

    [Header("Dependencias")]
    [Tooltip("Arrastra aquí el objeto del Jugador desde la jerarquía.")]
    [SerializeField] private Transform playerTransform;

    [Header("Debug")]
    [SerializeField] private bool logSpawns = true;

    private HashSet<int> triggeredWaypointIndices = new HashSet<int>();

    private void Awake()
    {
        if (playerTransform == null)
        {
            Debug.LogError("EnemySpawner: ¡No se ha asignado la referencia del 'playerTransform' en el Inspector!");
        }
    }

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
        SpawnWave waveToTrigger = spawnWaves.Find(wave => wave.triggerWaypointIndex == index);

        if (waveToTrigger == null) return;
        if (spawnOncePerWave && triggeredWaypointIndices.Contains(index)) return;
        // if (enemyPrefab == null || playerTransform == null) return; // <-- CAMBIO: Ya no revisamos el prefab.
        if (playerTransform == null) return;

        if (logSpawns)
        {
            Debug.Log($"<color=orange>Activando oleada '{waveToTrigger.waveName}' en waypoint {index}.</color>");
        }

        foreach (Transform spawnPoint in waveToTrigger.spawnPoints)
        {
            int enemyCount = Random.Range(waveToTrigger.minEnemiesPerPoint, waveToTrigger.maxEnemiesPerPoint + 1);

            for (int i = 0; i < enemyCount; i++)
            {
                var pool = PickVariantPool();
                GameObject enemyInstance = pool != null ? pool.GetPooledObject() : null;

                if (enemyInstance == null)
                {
                    Debug.LogWarning("EnemySpawner: no se pudo obtener objeto del pool.");
                    continue; // o return, según tu flujo
                }

                if (enemyInstance != null)
                {
                    Vector3 randomOffset = (scatterRadius > 0f) ? Random.insideUnitSphere * scatterRadius : Vector3.zero;
                    randomOffset.y = 0;
                    Vector3 spawnPos = spawnPoint.position + randomOffset;

                    // Configura posición, rotación y activa el objeto.
                    enemyInstance.transform.position = spawnPos;
                    enemyInstance.transform.rotation = spawnPoint.rotation;
                    enemyInstance.SetActive(true);

                    // Inyección de dependencias (sigue igual).
                    EnemyBehaviour enemyBehaviour = enemyInstance.GetComponent<EnemyBehaviour>();
                    if (enemyBehaviour != null)
                    {
                        enemyBehaviour.Initialize(playerTransform);
                    }
                }
            }
        }

        if (spawnOncePerWave)
        {
            triggeredWaypointIndices.Add(index);
        }
    }
    private ObjectPooler PickVariantPool()
    {
        // El resto del código funciona perfectamente con la lista.
        if (variantPools == null || variantPools.Count == 0)
        {
            Debug.LogError("No hay pools de variantes asignados en el EnemySpawner.");
            return null;
        }

        // Si no hay pesos válidos, elige uniforme
        if (variantWeights == null || variantWeights.Count != variantPools.Count)
        {
            int i = Random.Range(0, variantPools.Count);
            return variantPools[i];
        }

        // Ruleta por pesos (se mantiene igual)...
        float total = 0f;
        for (int i = 0; i < variantWeights.Count; i++)
            total += Mathf.Max(0f, variantWeights[i]);

        if (total <= 0f)
        {
            int i = Random.Range(0, variantPools.Count);
            return variantPools[i];
        }

        float r = Random.value * total;
        for (int i = 0; i < variantPools.Count; i++)
        {
            r -= Mathf.Max(0f, variantWeights[i]);
            if (r <= 0f) return variantPools[i];
        }
        return variantPools[variantPools.Count - 1];
    }

    private void OnDrawGizmosSelected()
    {
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