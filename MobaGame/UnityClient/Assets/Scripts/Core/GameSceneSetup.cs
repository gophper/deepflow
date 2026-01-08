using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MobaGame.Core
{
    /// <summary>
    /// Helper script to set up the game scene with map, spawners, etc.
    /// </summary>
    public class GameSceneSetup : MonoBehaviour
    {
        [Header("Map Setup")]
        public GameObject mapGround;
        public GameObject[] towers;
        public GameObject[] crystals;
        public GameObject[] minionSpawners;

        [ContextMenu("Setup Basic Map")]
        public void SetupBasicMap()
        {
            CreateGround();
            CreateLanes();
            CreateTowers();
            CreateCrystals();
            CreateMinionSpawners();
            Debug.Log("Basic map setup complete!");
        }

        void CreateGround()
        {
            // Create main ground plane
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(20, 1, 20); // 200x200 map
            ground.GetComponent<Renderer>().material.color = new Color(0.3f, 0.5f, 0.3f);
        }

        void CreateLanes()
        {
            // Visual lane markers (optional)
            CreateLaneMarker("TopLane", new Vector3(-50, 0.1f, 0), new Vector3(50, 0.1f, 0));
            CreateLaneMarker("MidLane", new Vector3(-70, 0.1f, -70), new Vector3(70, 0.1f, 70));
            CreateLaneMarker("BotLane", new Vector3(0, 0.1f, -50), new Vector3(0, 0.1f, 50));
        }

        void CreateLaneMarker(string name, Vector3 start, Vector3 end)
        {
            GameObject lane = new GameObject(name);
            LineRenderer lr = lane.AddComponent<LineRenderer>();
            lr.startWidth = 2f;
            lr.endWidth = 2f;
            lr.positionCount = 2;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = Color.yellow;
            lr.endColor = Color.yellow;
        }

        void CreateTowers()
        {
            // Blue team towers (Team 0)
            CreateTower("Blue_Top_T1", new Vector3(-40, 0, 10), 0);
            CreateTower("Blue_Top_T2", new Vector3(-30, 0, 15), 0);
            CreateTower("Blue_Top_T3", new Vector3(-20, 0, 20), 0);
            
            CreateTower("Blue_Mid_T1", new Vector3(-40, 0, -40), 0);
            CreateTower("Blue_Mid_T2", new Vector3(-30, 0, -30), 0);
            CreateTower("Blue_Mid_T3", new Vector3(-20, 0, -20), 0);
            
            CreateTower("Blue_Bot_T1", new Vector3(10, 0, -40), 0);
            CreateTower("Blue_Bot_T2", new Vector3(15, 0, -30), 0);
            CreateTower("Blue_Bot_T3", new Vector3(20, 0, -20), 0);

            // Red team towers (Team 1)
            CreateTower("Red_Top_T1", new Vector3(40, 0, -10), 1);
            CreateTower("Red_Top_T2", new Vector3(30, 0, -15), 1);
            CreateTower("Red_Top_T3", new Vector3(20, 0, -20), 1);
            
            CreateTower("Red_Mid_T1", new Vector3(40, 0, 40), 1);
            CreateTower("Red_Mid_T2", new Vector3(30, 0, 30), 1);
            CreateTower("Red_Mid_T3", new Vector3(20, 0, 20), 1);
            
            CreateTower("Red_Bot_T1", new Vector3(-10, 0, 40), 1);
            CreateTower("Red_Bot_T2", new Vector3(-15, 0, 30), 1);
            CreateTower("Red_Bot_T3", new Vector3(-20, 0, 20), 1);
        }

        void CreateTower(string name, Vector3 position, int teamId)
        {
            GameObject tower = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tower.name = name;
            tower.transform.position = position;
            tower.transform.localScale = new Vector3(2, 5, 2);
            tower.tag = "Tower";
            tower.layer = LayerMask.NameToLayer("Tower");
            
            Renderer renderer = tower.GetComponent<Renderer>();
            renderer.material.color = teamId == 0 ? Color.blue : Color.red;

            Gameplay.Tower towerScript = tower.AddComponent<Gameplay.Tower>();
            towerScript.teamId = teamId;
        }

        void CreateCrystals()
        {
            // Blue crystal
            GameObject blueCrystal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blueCrystal.name = "Blue_Crystal";
            blueCrystal.transform.position = new Vector3(-50, 2.5f, -50);
            blueCrystal.transform.localScale = new Vector3(5, 5, 5);
            blueCrystal.tag = "Crystal";
            blueCrystal.GetComponent<Renderer>().material.color = Color.cyan;
            
            Gameplay.Crystal blueCrystalScript = blueCrystal.AddComponent<Gameplay.Crystal>();
            blueCrystalScript.teamId = 0;

            // Red crystal
            GameObject redCrystal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            redCrystal.name = "Red_Crystal";
            redCrystal.transform.position = new Vector3(50, 2.5f, 50);
            redCrystal.transform.localScale = new Vector3(5, 5, 5);
            redCrystal.tag = "Crystal";
            redCrystal.GetComponent<Renderer>().material.color = Color.magenta;
            
            Gameplay.Crystal redCrystalScript = redCrystal.AddComponent<Gameplay.Crystal>();
            redCrystalScript.teamId = 1;
        }

        void CreateMinionSpawners()
        {
            // Blue team spawners
            CreateMinionSpawner("Blue_Top_Spawner", new Vector3(-50, 0, 0), 0, 0);
            CreateMinionSpawner("Blue_Mid_Spawner", new Vector3(-50, 0, -50), 0, 1);
            CreateMinionSpawner("Blue_Bot_Spawner", new Vector3(0, 0, -50), 0, 2);

            // Red team spawners
            CreateMinionSpawner("Red_Top_Spawner", new Vector3(50, 0, 0), 1, 0);
            CreateMinionSpawner("Red_Mid_Spawner", new Vector3(50, 0, 50), 1, 1);
            CreateMinionSpawner("Red_Bot_Spawner", new Vector3(0, 0, 50), 1, 2);
        }

        void CreateMinionSpawner(string name, Vector3 position, int teamId, int laneId)
        {
            GameObject spawner = new GameObject(name);
            spawner.transform.position = position;
            
            Gameplay.MinionSpawner spawnerScript = spawner.AddComponent<Gameplay.MinionSpawner>();
            spawnerScript.teamId = teamId;
            spawnerScript.laneId = laneId;

            // Create minion prefab reference would go here
            Debug.Log($"Created spawner: {name}");
        }
    }
}
