using UnityEngine;
using System.Collections.Generic;

public class ObjectPooler : MonoBehaviour
{
    // Implementación de un Singleton para fácil acceso desde otros scripts.
    public static ObjectPooler Instance;

    [Tooltip("El prefab del objeto que este pool gestionará.")]
    [SerializeField] private GameObject objectToPool;

    [Tooltip("La cantidad inicial de objetos a crear en el pool.")]
    [SerializeField] private int amountToPool;

    private List<GameObject> pooledObjects;

    private void Awake()
    {
        // Asegura que solo haya una instancia del ObjectPooler.
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Inicializa el pool creando los objetos.
        pooledObjects = new List<GameObject>();
        for (int i = 0; i < amountToPool; i++)
        {
            GameObject obj = Instantiate(objectToPool);
            obj.SetActive(false); // Los mantiene desactivados hasta que se necesiten.
            pooledObjects.Add(obj);
        }
    }

    /// <summary>
    /// Obtiene un objeto del pool. Si no hay disponibles, opcionalmente puede crear uno nuevo.
    /// </summary>
    /// <returns>Un GameObject del pool que está inactivo, o null si no hay ninguno.</returns>
    public GameObject GetPooledObject()
    {
        // Busca en la lista un objeto que no esté activo en la jerarquía.
        foreach (GameObject obj in pooledObjects)
        {
            if (!obj.activeInHierarchy)
            {
                return obj;
            }
        }

        // Opcional: Si te quedas sin objetos, puedes instanciar uno nuevo y añadirlo al pool.
        // GameObject newObj = Instantiate(objectToPool);
        // newObj.SetActive(false);
        // pooledObjects.Add(newObj);
        // return newObj;

        // O retornar null si prefieres un pool de tamaño fijo.
        Debug.LogWarning("ObjectPooler: No hay suficientes objetos en el pool.");
        return null;
    }
}