using UnityEngine;
using System.Collections.Generic;

public class ObjectPooler : MonoBehaviour
{
    [Tooltip("El prefab del objeto que este pool gestionará.")]
    [SerializeField] private GameObject objectToPool;

    [Tooltip("La cantidad inicial de objetos a crear en el pool.")]
    [SerializeField] private int amountToPool;

    private List<GameObject> pooledObjects;


    void Start()
    {
        // Esta lógica se mantiene igual.
        pooledObjects = new List<GameObject>();
        for (int i = 0; i < amountToPool; i++)
        {
            GameObject obj = Instantiate(objectToPool);
            obj.SetActive(false);
            pooledObjects.Add(obj);
        }
    }

    public GameObject GetPooledObject()
    {
        // Esta lógica se mantiene igual.
        foreach (GameObject obj in pooledObjects)
        {
            if (!obj.activeInHierarchy)
            {
                return obj;
            }
        }

        Debug.LogWarning("ObjectPooler: No hay suficientes objetos en el pool.");
        return null;
    }
}