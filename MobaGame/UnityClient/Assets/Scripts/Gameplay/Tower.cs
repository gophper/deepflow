using UnityEngine;

namespace MobaGame.Gameplay
{
    public class Tower : MonoBehaviour
    {
        [Header("Tower Configuration")]
        public int teamId; // 0 = Blue, 1 = Red
        public float maxHealth = 3000f;
        public float currentHealth;
        public float attackDamage = 150f;
        public float attackRange = 8f;
        public float attackSpeed = 0.8f;
        public int goldReward = 150;
        public bool isInhibitor = false;

        private GameObject currentTarget;
        private float lastAttackTime;

        void Start()
        {
            currentHealth = maxHealth;
        }

        void Update()
        {
            FindTarget();

            if (currentTarget != null)
            {
                AttackTarget();
            }
        }

        void FindTarget()
        {
            Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, attackRange);
            float closestDistance = float.MaxValue;
            GameObject closestEnemy = null;
            GameObject priorityMinion = null;

            foreach (var col in nearbyObjects)
            {
                float distance = Vector3.Distance(transform.position, col.transform.position);

                // Priority 1: Enemy minions
                Minion minion = col.GetComponent<Minion>();
                if (minion != null && minion.teamId != teamId)
                {
                    if (priorityMinion == null || distance < Vector3.Distance(transform.position, priorityMinion.transform.position))
                    {
                        priorityMinion = col.gameObject;
                    }
                }

                // Priority 2: Enemy heroes (only if no minions)
                Hero hero = col.GetComponent<Hero>();
                if (hero != null && hero.teamId != teamId)
                {
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestEnemy = col.gameObject;
                    }
                }
            }

            // Prioritize minions over heroes
            if (priorityMinion != null)
            {
                currentTarget = priorityMinion;
            }
            else if (closestEnemy != null)
            {
                currentTarget = closestEnemy;
            }
            else
            {
                currentTarget = null;
            }
        }

        void AttackTarget()
        {
            if (currentTarget == null) return;

            if (Time.time >= lastAttackTime + (1f / attackSpeed))
            {
                // Look at target
                transform.LookAt(currentTarget.transform);

                // Perform attack
                Hero targetHero = currentTarget.GetComponent<Hero>();
                Minion targetMinion = currentTarget.GetComponent<Minion>();

                if (targetHero != null)
                {
                    targetHero.TakeDamage(attackDamage, null);
                }
                else if (targetMinion != null)
                {
                    targetMinion.TakeDamage(attackDamage, null);
                }

                lastAttackTime = Time.time;
            }
        }

        public void TakeDamage(float damage)
        {
            currentHealth -= damage;

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        void Die()
        {
            // Find nearby heroes to grant rewards
            Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, 15f);
            foreach (var col in nearbyObjects)
            {
                Hero hero = col.GetComponent<Hero>();
                if (hero != null && hero.teamId != teamId)
                {
                    hero.AddGold(goldReward);
                }
            }

            Destroy(gameObject);
        }
    }
}
