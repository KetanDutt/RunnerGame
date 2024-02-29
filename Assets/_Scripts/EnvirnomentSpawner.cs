using System.Collections.Generic;
using UnityEngine;

public class EnvironmentSpawner : MonoBehaviour
{
    public GameObject environmentPrefab; // Reference to the environment prefab
    public Transform spawnMarker; // Marker for spawn distance
    public Transform destroyMarker; // Marker for destroy distance

    public int poolSize = 10; // Number of environment objects to instantiate initially
    public float prefabWidth; // Width of the environment prefab

    private List<GameObject> environmentPool = new List<GameObject>(); // Pool of environment objects
    private float lastSpawnPosition; // Position of the last spawned environment

    void Start()
    {
        // Calculate the width of the prefab from the BoxCollider component
        prefabWidth = environmentPrefab.GetComponent<BoxCollider>().size.z;

        // Initialize the object pool
        InitializePool();

        // Ensure at least one environment is spawned initially
        SpawnEnvironment();
    }

    void Update()
    {
        // Check if the spawn point is outside the bounds of the last spawned object
        if (spawnMarker.position.z > lastSpawnPosition)
        {
            SpawnEnvironment();
        }

        // Check if any environment objects are completely behind the destroy point, then reset and reposition them
        CheckAndResetEnvironment();
    }

    void InitializePool()
    {
        // Instantiate and disable environment objects, then add them to the pool
        for (int i = 0; i < poolSize; i++)
        {
            GameObject environment = Instantiate(environmentPrefab, transform);
            environment.SetActive(false);
            environmentPool.Add(environment);
        }
    }

    void SpawnEnvironment()
    {
        // Get an environment object from the pool
        GameObject newEnvironment = GetPooledEnvironment();

        // Set the position of the environment object
        newEnvironment.transform.position = new Vector3(0, 0, lastSpawnPosition);

        // Activate the environment object
        newEnvironment.SetActive(true);

        // Update last spawn position for the next environment
        lastSpawnPosition += prefabWidth;
    }

    GameObject GetPooledEnvironment()
    {
        // Find a disabled environment object in the pool and return it
        foreach (GameObject environment in environmentPool)
        {
            if (!environment.activeInHierarchy)
            {
                return environment;
            }
        }

        // If no disabled environment object is found, expand the pool
        GameObject newEnvironment = Instantiate(environmentPrefab, transform);
        newEnvironment.SetActive(false);
        environmentPool.Add(newEnvironment);
        return newEnvironment;
    }

    void CheckAndResetEnvironment()
    {
        // Loop through each environment object in the pool
        foreach (GameObject environment in environmentPool)
        {
            // Check if the environment object is completely behind the destroy point
            if (environment.activeInHierarchy && environment.transform.position.z + prefabWidth < destroyMarker.position.z)
            {
                // Disable the environment object and reset its position
                environment.SetActive(false);
                environment.transform.position = Vector3.zero; // You may want to adjust the position reset logic
            }
        }
    }
}
