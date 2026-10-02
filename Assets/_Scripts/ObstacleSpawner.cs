using System.Collections.Generic;
using UnityEngine;

namespace RunnerGame
{
    /// <summary>
    /// Spawns obstacles (and rows of coins between them) ahead of the player and
    /// recycles them once they pass behind the player. Uses object pools so no
    /// allocation happens at runtime.
    /// </summary>
    public class ObstacleSpawner : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject obstaclePrefab;
        [SerializeField] private GameObject coinPrefab;

        [Header("Markers")]
        [SerializeField] private Transform spawnMarker;
        [SerializeField] private Transform destroyMarker;

        [Header("Pools")]
        [SerializeField] private int poolSize = 10;

        [Header("Obstacles")]
        [SerializeField] private float minGap = 8f;
        [SerializeField] private float maxGap = 24f;
        [SerializeField] private float[] laneOffsets = new float[] { -2.5f, 0f, 2.5f };
        [SerializeField] private float yOffset = -0.47f;

        [Header("Coins")]
        [SerializeField] private int minCoinsPerRow = 4;
        [SerializeField] private int maxCoinsPerRow = 8;
        [SerializeField] private float coinSpacing = 1.2f;
        [SerializeField] private float coinRowEdgeMargin = 2.5f;
        [Header("Coin magnet")]
        [SerializeField] private float magnetRadius = 1.7f;

        private readonly List<GameObject> _obstacles = new List<GameObject>();
        private readonly List<GameObject> _coins = new List<GameObject>();
        private readonly Stack<int> _freeObstacles = new Stack<int>();
        private readonly Stack<int> _freeCoins = new Stack<int>();

        private float _nextSpawnZ;
        private int _lastBlockerIndex = -1;

        private void Start()
        {
            BuildPools();
            _nextSpawnZ = 10f;

            // Pre-fill the track up to the spawn marker.
            while (spawnMarker != null && spawnMarker.position.z > _nextSpawnZ)
            {
                SpawnRow();
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
                SpawnRow();
            }

            Recycle(_obstacles, _freeObstacles, 2f);
            Recycle(_coins, _freeCoins, 2f);
            AttractCoins();
        }

        // ---- Spawning ----

        private void SpawnRow()
        {
            if (obstaclePrefab == null)
            {
                _nextSpawnZ += 30f;
                return;
            }

            float gap = CurrentGap();
            float obstacleZ = _nextSpawnZ;
            int lane = RandomLane();
            int blockerIndex = PickBlocker();

            GameObject obstacle = GetPooled(_obstacles, _freeObstacles, obstaclePrefab);
            obstacle.transform.position = new Vector3(laneOffsets[lane], yOffset, obstacleZ);
            EnableBlocker(obstacle, blockerIndex);
            obstacle.SetActive(true);

            SpawnCoinRow(obstacleZ, gap, lane);

            _nextSpawnZ = obstacleZ + gap;
        }

        /// <summary>Gap grows with the player's current speed so dodging stays fair.</summary>
        private float CurrentGap()
        {
            float speed = PlayerController.Instance != null ? PlayerController.Instance.CurrentSpeed : 10f;
            float speedFactor = Mathf.Clamp01((speed - 10f) / (26f - 10f));
            return Mathf.Lerp(minGap, maxGap, speedFactor);
        }

        private int RandomLane()
        {
            int lane;
            do
            {
                lane = Random.Range(0, Mathf.Max(1, laneOffsets.Length));
            }
            while (lane == _lastObstacleLane && Random.value < 0.6f);
            _lastObstacleLane = lane;
            return lane;
        }

        private int _lastObstacleLane = -1;

        /// <summary>Picks a random blocker variant, avoiding three identical rows in a row.</summary>
        private int PickBlocker()
        {
            int count = 3; // the obstacle prefab carries BlockerStandard/Jump/Roll
            int index;
            do
            {
                index = Random.Range(0, count);
            }
            while (index == _lastBlockerIndex && Random.value < 0.7f);
            _lastBlockerIndex = index;
            return index;
        }

        private void EnableBlocker(GameObject obstacle, int index)
        {
            int childCount = obstacle.transform.childCount;
            if (childCount == 0)
            {
                return;
            }

            int enabledIndex = Mathf.Clamp(index, 0, childCount - 1);
            for (int i = 0; i < childCount; i++)
            {
                obstacle.transform.GetChild(i).gameObject.SetActive(i == enabledIndex);
            }
        }

        /// <summary>
        /// Places a coin row in the open gap between this obstacle and the next one,
        /// so coins can never end up inside the next obstacle.
        /// </summary>
        private void SpawnCoinRow(float obstacleZ, float gap, int obstacleLane)
        {
            if (coinPrefab == null)
            {
                return;
            }

            int count = Random.Range(Mathf.Max(1, minCoinsPerRow), Mathf.Max(1, maxCoinsPerRow) + 1);
            float rowLength = (count - 1) * coinSpacing;

            float minCenter = obstacleZ + coinRowEdgeMargin + rowLength * 0.5f;
            float maxCenter = obstacleZ + gap - coinRowEdgeMargin - rowLength * 0.5f;
            if (maxCenter < minCenter)
            {
                return; // gap too small for a coin row
            }

            float rowCenter = Random.Range(minCenter, maxCenter);

            // Coins always go to a lane the player can reach; prefer a different
            // lane than the obstacle so the row rewards a lane change.
            int lane = RandomLaneForCoins(obstacleLane);
            float x = laneOffsets[Mathf.Clamp(lane, 0, laneOffsets.Length - 1)];

            for (int i = 0; i < count; i++)
            {
                GameObject coin = GetPooled(_coins, _freeCoins, coinPrefab);
                coin.transform.position = new Vector3(x, yOffset, rowCenter - rowLength * 0.5f + i * coinSpacing);
                coin.SetActive(true);
            }
        }

        private int _lastCoinLane = -1;

        private int RandomLaneForCoins(int obstacleLane)
        {
            int count = Mathf.Max(1, laneOffsets.Length);
            int lane;
            do
            {
                lane = Random.Range(0, count);
            }
            while (lane == _lastCoinLane && Random.value < 0.6f);
            _lastCoinLane = lane;
            return lane;
        }

        // ---- Pools ----

        private void BuildPools()
        {
            for (int i = 0; i < poolSize; i++)
            {
                if (obstaclePrefab != null)
                {
                    var obstacle = Instantiate(obstaclePrefab, transform);
                    obstacle.SetActive(false);
                    _obstacles.Add(obstacle);
                    _freeObstacles.Push(_obstacles.Count - 1);
                }

                if (coinPrefab != null)
                {
                    var coin = Instantiate(coinPrefab, transform);
                    coin.SetActive(false);
                    _coins.Add(coin);
                    _freeCoins.Push(_coins.Count - 1);
                }
            }
        }

        private GameObject GetPooled(List<GameObject> pool, Stack<int> free, GameObject prefab)
        {
            if (free.Count > 0)
            {
                return pool[free.Pop()];
            }

            // Pool exhausted: grow it.
            var obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            pool.Add(obj);
            return obj;
        }

        private void Recycle(List<GameObject> pool, Stack<int> free, float margin)
        {
            for (int i = 0; i < pool.Count; i++)
            {
                var obj = pool[i];
                if (obj.activeInHierarchy && obj.transform.position.z + margin < destroyMarker.position.z)
                {
                    obj.SetActive(false);
                    obj.transform.position = Vector3.zero;
                    free.Push(i);
                }
            }
        }

        // ---- Coin magnet ----

        /// <summary>Gently pulls nearby coins toward the player for a satisfying pickup feel.</summary>
        private void AttractCoins()
        {
            if (magnetRadius <= 0f || PlayerController.Instance == null)
            {
                return;
            }

            Vector3 playerPosition = PlayerController.Instance.transform.position;
            float radiusSq = magnetRadius * magnetRadius;
            float pullStep = 20f * Time.deltaTime;

            for (int i = 0; i < _coins.Count; i++)
            {
                var coin = _coins[i];
                if (!coin.activeInHierarchy)
                {
                    continue;
                }

                Vector3 coinPosition = coin.transform.position;
                Vector3 toPlayer = playerPosition - coinPosition;
                if (toPlayer.sqrMagnitude > radiusSq)
                {
                    continue;
                }

                coin.transform.position = Vector3.MoveTowards(coinPosition, playerPosition, pullStep);
            }
        }
    }
}
