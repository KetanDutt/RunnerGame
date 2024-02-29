using UnityEngine;
using System.Collections.Generic;

public class ObstacleSpawner : MonoBehaviour
{
    [SerializeField] private GameObject obstaclePrefab; // Reference to the obstacle prefab
    [SerializeField] private GameObject coinPrefab; // Reference to the coin prefab
    [SerializeField] private Transform spawnMarker; // Marker for spawn distance
    [SerializeField] private Transform destroyMarker; // Marker for destroy distance

    [SerializeField] private int poolSize = 10; // Number of obstacle objects to instantiate initially
    [SerializeField] private float prefabWidth = 30; // Width of the obstacle prefab
    [SerializeField] private float[] laneOffsets; // Width of each lane
    [SerializeField] private float yOffset = 0.5f; // Y offset for both obstacles and coins

    private List<GameObject> obstaclePool = new List<GameObject>(); // Pool of obstacle objects
    private List<GameObject> coinPool = new List<GameObject>(); // Pool of coin objects
    private float lastSpawnPosition; // Position of the last spawned obstacle

    void Start()
    {
        // Initialize the object pools
        InitializePool();

        // Ensure at least one obstacle is spawned initially
        SpawnObstacle();
    }

    void Update()
    {
        // Check if the spawn point is outside the bounds of the last spawned object
        if (spawnMarker.position.z > lastSpawnPosition)
        {
            SpawnObstacle();
        }

        // Check if any obstacle objects are completely behind the destroy point, then reset and reposition them
        CheckAndResetObject(obstaclePool);
        CheckAndResetObject(coinPool);
    }

    void InitializePool()
    {
        // Instantiate and disable obstacle objects, then add them to the pool
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obstacle = Instantiate(obstaclePrefab, transform);
            obstacle.SetActive(false);
            obstaclePool.Add(obstacle);
        }

        // Instantiate and disable coin objects, then add them to the pool
        for (int i = 0; i < poolSize; i++)
        {
            GameObject coin = Instantiate(coinPrefab, transform);
            coin.SetActive(false);
            coinPool.Add(coin);
        }
    }

    void SpawnObstacle()
    {
        // Get an obstacle object from the pool
        GameObject newObstacle = GetPooledObject(obstaclePool, false);

        // Set the position of the obstacle object
        newObstacle.transform.position = new Vector3(GetRandomLaneOffset(), yOffset, lastSpawnPosition);

        // Activate the obstacle object
        newObstacle.SetActive(true);

        for (int i = 0; i < newObstacle.transform.childCount; i++)
        {
            newObstacle.transform.GetChild(i).gameObject.SetActive(false);
        }
        newObstacle.transform.GetChild(Random.Range(0, newObstacle.transform.childCount)).gameObject.SetActive(true);

        // Update last spawn position for the next obstacle
        lastSpawnPosition += prefabWidth;

        // Check if a coin should be spawned in the remaining lanes
        // if (Random.value < coinSpawnChance)
        // {
        float temp = lastSpawnPosition;
        int count = Random.Range(4, 8);
        lastSpawnPosition -= count / 2;
        float off = GetRandomLaneOffset();
        for (int i = 0; i < count; i++)
        {
            GameObject newCoin = GetPooledObject(coinPool, true);
            newCoin.transform.position = new Vector3(off, yOffset, lastSpawnPosition);
            newCoin.SetActive(true);
            lastSpawnPosition += 1;
        }
        lastSpawnPosition = temp;
        // }
    }

    GameObject GetPooledObject(List<GameObject> pool, bool isCoin)
    {
        // Find a disabled object in the pool and return it
        foreach (GameObject obj in pool)
        {
            if (!obj.activeInHierarchy)
            {
                return obj;
            }
        }

        // If no disabled object is found, expand the pool
        GameObject newObj;
        if (isCoin)
        {
            newObj = Instantiate(coinPrefab, transform);
        }
        else
        {
            newObj = Instantiate(obstaclePrefab, transform);
        }
        newObj.SetActive(false);
        pool.Add(newObj);
        return newObj;
    }

    void CheckAndResetObject(List<GameObject> objectPool)
    {
        // Loop through each object in the pool
        foreach (GameObject obj in objectPool)
        {
            // Check if the object is completely behind the destroy point
            if (obj.activeInHierarchy && obj.transform.position.z + prefabWidth < destroyMarker.position.z)
            {
                // Disable the object and reset its position
                obj.SetActive(false);
                obj.transform.position = Vector3.zero; // You may want to adjust the position reset logic
            }
        }
    }

    float GetRandomLaneOffset()
    {
        // Get a random lane offset from the array
        return laneOffsets[Random.Range(0, laneOffsets.Length)];
    }
}
