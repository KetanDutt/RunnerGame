using System.Collections.Generic;
using UnityEngine;

namespace RunnerGame
{
    /// <summary>
    /// Tiles the ground/environment chunks ahead of the player and recycles them
    /// once they pass behind. Uses an object pool (no runtime allocation).
    /// </summary>
    public class EnvironmentSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject environmentPrefab;
        [SerializeField] private Transform spawnMarker;
        [SerializeField] private Transform destroyMarker;
        [SerializeField] private int poolSize = 10;

        /// <summary>Override for the chunk length in Z. When 0 it is read from the prefab's BoxCollider.</summary>
        [SerializeField] private float prefabWidth = 0f;

        private const float FallbackChunkWidth = 60f;

        private readonly List<GameObject> _pool = new List<GameObject>();
        private readonly Stack<int> _free = new Stack<int>();

        private float _chunkWidth;
        private float _nextSpawnZ;

        private void Start()
        {
            _chunkWidth = ResolveChunkWidth();
            BuildPool();
            _nextSpawnZ = 0f;

            while (spawnMarker != null && spawnMarker.position.z > _nextSpawnZ)
            {
                SpawnChunk();
            }
        }

        private void Update()
        {
            if (!GameManager.IsGameRunning() || spawnMarker == null || destroyMarker == null)
            {
                return;
            }

            while (spawnMarker.position.z > _nextSpawnZ)
            {
                SpawnChunk();
            }

            Recycle();
        }

        private float ResolveChunkWidth()
        {
            if (prefabWidth > 0f)
            {
                return prefabWidth;
            }

            if (environmentPrefab != null)
            {
                var collider = environmentPrefab.GetComponent<BoxCollider>();
                if (collider != null)
                {
                    return collider.size.z;
                }
            }

            Debug.LogWarning("EnvironmentSpawner: no BoxCollider found on the environment prefab, falling back to a 60 m chunk width.");
            return FallbackChunkWidth;
        }

        private void BuildPool()
        {
            if (environmentPrefab == null)
            {
                return;
            }

            for (int i = 0; i < poolSize; i++)
            {
                var environment = Instantiate(environmentPrefab, transform);
                environment.SetActive(false);
                _pool.Add(environment);
                _free.Push(_pool.Count - 1);
            }
        }

        private void SpawnChunk()
        {
            if (environmentPrefab == null)
            {
                _nextSpawnZ += FallbackChunkWidth;
                return;
            }

            GameObject environment = GetPooled();
            environment.transform.position = new Vector3(0f, 0f, _nextSpawnZ);
            environment.SetActive(true);

            _nextSpawnZ += _chunkWidth;
        }

        private GameObject GetPooled()
        {
            if (_free.Count > 0)
            {
                return _pool[_free.Pop()];
            }

            // Pool exhausted: grow it.
            var obj = Instantiate(environmentPrefab, transform);
            obj.SetActive(false);
            _pool.Add(obj);
            return obj;
        }

        private void Recycle()
        {
            for (int i = 0; i < _pool.Count; i++)
            {
                var environment = _pool[i];
                if (environment.activeInHierarchy && environment.transform.position.z + _chunkWidth * 0.5f < destroyMarker.position.z)
                {
                    environment.SetActive(false);
                    environment.transform.position = Vector3.zero;
                    _free.Push(i);
                }
            }
        }
    }
}
