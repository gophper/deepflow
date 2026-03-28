using UnityEngine;
using UnityEngine.AI;

namespace MobaGame.Gameplay
{
    public class Minion : MonoBehaviour
    {
        [Header("Minion Configuration")]
        public int teamId; // 0 = Blue, 1 = Red
        public float maxHealth = 500f;
        public float currentHealth;
        public float attackDamage = 20f;
        public float attackRange = 2f;
        public float attackSpeed = 1f;
        public int goldReward = 20;
        public int expReward = 30;

        [Header("Path")]
        public Transform[] waypoints;
        private int currentWaypointIndex = 0;

        private NavMeshAgent navAgent;
        private GameObject currentTarget;
        private float lastAttackTime;

        void Start()
        {
            currentHealth = maxHealth;
            navAgent = GetComponent<NavMeshAgent>();
            
            if (navAgent == null)
            {
                navAgent = gameObject.AddComponent<NavMeshAgent>();
                navAgent.speed = 3.5f;
                navAgent.acceleration = 8f;
            }

            if (waypoints != null && waypoints.Length > 0)
            {
                navAgent.SetDestination(waypoints[0].position);
            }
        }

        void Update()
        {
            // Find nearby enemies
            FindTarget();

            if (currentTarget != null)
            {
                // Attack target
                AttackTarget();
            }
            else
            {
                // Move along path
                FollowPath();
            }
        }

        void FindTarget()
        {
            Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, 10f);
            float closestDistance = float.MaxValue;
            GameObject closestEnemy = null;

            foreach (var col in nearbyObjects)
            {
                // Check for enemy heroes
                Hero hero = col.GetComponent<Hero>();
                if (hero != null && hero.teamId != teamId)
                {
                    float distance = Vector3.Distance(transform.position, col.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestEnemy = col.gameObject;
                    }
                }

                // Check for enemy minions
                Minion minion = col.GetComponent<Minion>();
                if (minion != null && minion.teamId != teamId)
                {
                    float distance = Vector3.Distance(transform.position, col.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestEnemy = col.gameObject;
                    }
                }

                // Check for enemy towers
                Tower tower = col.GetComponent<Tower>();
                if (tower != null && tower.teamId != teamId)
                {
                    float distance = Vector3.Distance(transform.position, col.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestEnemy = col.gameObject;
                    }
                }
            }

            currentTarget = closestEnemy;
        }

        void AttackTarget()
        {
            if (currentTarget == null) return;

            float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
            
            if (distance <= attackRange)
            {
                // Stop moving and attack
                navAgent.isStopped = true;
                
                if (Time.time >= lastAttackTime + (1f / attackSpeed))
                {
                    // Perform attack
                    Hero targetHero = currentTarget.GetComponent<Hero>();
                    Minion targetMinion = currentTarget.GetComponent<Minion>();
                    Tower targetTower = currentTarget.GetComponent<Tower>();

                    if (targetHero != null)
                    {
                        targetHero.TakeDamage(attackDamage, null);
                    }
                    else if (targetMinion != null)
                    {
                        targetMinion.TakeDamage(attackDamage, null);
                    }
                    else if (targetTower != null)
                    {
                        targetTower.TakeDamage(attackDamage);
                    }

                    lastAttackTime = Time.time;
                }
            }
            else
            {
                // Move towards target
                navAgent.isStopped = false;
                navAgent.SetDestination(currentTarget.transform.position);
            }
        }

        void FollowPath()
        {
            if (waypoints == null || waypoints.Length == 0) return;

            navAgent.isStopped = false;

            // Check if reached current waypoint
            if (!navAgent.pathPending && navAgent.remainingDistance < 0.5f)
            {
                currentWaypointIndex++;
                if (currentWaypointIndex < waypoints.Length)
                {
                    navAgent.SetDestination(waypoints[currentWaypointIndex].position);
                }
            }
        }

        public void TakeDamage(float damage, GameObject attacker)
        {
            currentHealth -= damage;

            if (currentHealth <= 0)
            {
                Die(attacker);
            }
        }

        void Die(GameObject killer)
        {
            // Grant rewards
            if (killer != null)
            {
                Hero killerHero = killer.GetComponent<Hero>();
                if (killerHero != null)
                {
                    killerHero.AddGold(goldReward);
                    killerHero.AddExperience(expReward);
                }
            }

            Destroy(gameObject);
        }
    }
}
