using UnityEngine;
using System.Collections;

namespace MobaGame.Gameplay
{
    public class MinionSpawner : MonoBehaviour
    {
        [Header("Spawner Configuration")]
        public int teamId;
        public int laneId; // 0 = Top, 1 = Mid, 2 = Bot
        public GameObject minionPrefab;
        public Transform[] waypoints;
        
        [Header("Spawn Settings")]
        public float spawnInterval = 30f;
        public int minionsPerWave = 3;

        private float spawnTimer;

        void Start()
        {
            spawnTimer = 5f; // First wave after 5 seconds
        }

        void Update()
        {
            spawnTimer -= Time.deltaTime;
            
            if (spawnTimer <= 0)
            {
                SpawnWave();
                spawnTimer = spawnInterval;
            }
        }

        void SpawnWave()
        {
            for (int i = 0; i < minionsPerWave; i++)
            {
                StartCoroutine(SpawnMinionWithDelay(i * 0.5f));
            }
        }

        IEnumerator SpawnMinionWithDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            GameObject minion = Instantiate(minionPrefab, transform.position, Quaternion.identity);
            Minion minionComponent = minion.GetComponent<Minion>();
            
            if (minionComponent != null)
            {
                minionComponent.teamId = teamId;
                minionComponent.waypoints = waypoints;
            }

            // Set color based on team
            Renderer renderer = minion.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = teamId == 0 ? Color.blue : Color.red;
            }
        }
    }
}
