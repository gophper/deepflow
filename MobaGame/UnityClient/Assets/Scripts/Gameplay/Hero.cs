using UnityEngine;
using MobaGame.Data;

namespace MobaGame.Gameplay
{
    public class Hero : MonoBehaviour
    {
        [Header("Hero Configuration")]
        public HeroData heroData;
        public int teamId; // 0 = Blue Team, 1 = Red Team
        public int playerId;

        [Header("Current Stats")]
        public float currentHealth;
        public float currentMana;
        public int currentLevel = 1;
        public int currentGold = 500;
        public int currentExp = 0;

        [Header("Combat Stats")]
        public int kills = 0;
        public int deaths = 0;
        public int assists = 0;

        // Calculated stats
        private float maxHealth;
        private float maxMana;
        private float attackDamage;
        private float magicPower;
        private float armor;
        private float magicResist;
        private float moveSpeed;
        private float attackSpeed;
        private float attackRange;

        // Components
        private CharacterController characterController;
        
        // Skill cooldowns
        private float skill1Cooldown = 0f;
        private float skill2Cooldown = 0f;
        private float ultimateCooldown = 0f;
        private float lastAttackTime = 0f;

        // Respawn
        private bool isDead = false;
        private float respawnTimer = 0f;
        private const float RESPAWN_TIME = 10f;

        void Start()
        {
            characterController = GetComponent<CharacterController>();
            if (characterController == null)
            {
                characterController = gameObject.AddComponent<CharacterController>();
                characterController.radius = 0.5f;
                characterController.height = 2f;
            }

            InitializeStats();
        }

        void Update()
        {
            if (isDead)
            {
                HandleRespawn();
                return;
            }

            // Regeneration
            RegenerateHealthMana();

            // Update cooldowns
            if (skill1Cooldown > 0) skill1Cooldown -= Time.deltaTime;
            if (skill2Cooldown > 0) skill2Cooldown -= Time.deltaTime;
            if (ultimateCooldown > 0) ultimateCooldown -= Time.deltaTime;
        }

        public void InitializeStats()
        {
            if (heroData == null) return;

            maxHealth = heroData.maxHealth + (heroData.healthPerLevel * (currentLevel - 1));
            maxMana = heroData.maxMana + (heroData.manaPerLevel * (currentLevel - 1));
            attackDamage = heroData.attackDamage + (heroData.attackDamagePerLevel * (currentLevel - 1));
            magicPower = heroData.magicPower;
            armor = heroData.armor + (heroData.armorPerLevel * (currentLevel - 1));
            magicResist = heroData.magicResist + (heroData.magicResistPerLevel * (currentLevel - 1));
            moveSpeed = heroData.moveSpeed;
            attackSpeed = heroData.attackSpeed;
            attackRange = heroData.attackRange;

            currentHealth = maxHealth;
            currentMana = maxMana;
        }

        public void Move(Vector3 direction)
        {
            if (isDead || characterController == null) return;

            Vector3 move = direction.normalized * moveSpeed * Time.deltaTime;
            characterController.Move(move);

            // Rotate to face movement direction
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }

        public void BasicAttack(GameObject target)
        {
            if (isDead) return;
            if (Time.time < lastAttackTime + (1f / attackSpeed)) return;

            if (target == null) return;

            float distance = Vector3.Distance(transform.position, target.transform.position);
            if (distance > attackRange) return;

            // Deal damage
            Hero targetHero = target.GetComponent<Hero>();
            Minion targetMinion = target.GetComponent<Minion>();
            Tower targetTower = target.GetComponent<Tower>();

            if (targetHero != null)
            {
                targetHero.TakeDamage(attackDamage, this);
            }
            else if (targetMinion != null)
            {
                targetMinion.TakeDamage(attackDamage, this.gameObject);
            }
            else if (targetTower != null)
            {
                targetTower.TakeDamage(attackDamage);
            }

            lastAttackTime = Time.time;
        }

        public void UseSkill1(Vector3 targetPosition)
        {
            if (isDead || heroData.normalSkill1 == null) return;
            if (skill1Cooldown > 0) return;
            if (currentMana < heroData.normalSkill1.manaCost) return;

            currentMana -= heroData.normalSkill1.manaCost;
            skill1Cooldown = heroData.normalSkill1.cooldown;

            // Simple damage skill implementation
            Collider[] hits = Physics.OverlapSphere(targetPosition, heroData.normalSkill1.areaOfEffect);
            foreach (var hit in hits)
            {
                Hero enemy = hit.GetComponent<Hero>();
                if (enemy != null && enemy.teamId != teamId)
                {
                    enemy.TakeDamage(heroData.normalSkill1.damage, this);
                }
            }
        }

        public void UseSkill2(Vector3 targetPosition)
        {
            if (isDead || heroData.normalSkill2 == null) return;
            if (skill2Cooldown > 0) return;
            if (currentMana < heroData.normalSkill2.manaCost) return;

            currentMana -= heroData.normalSkill2.manaCost;
            skill2Cooldown = heroData.normalSkill2.cooldown;

            // Simple damage skill implementation
            Collider[] hits = Physics.OverlapSphere(targetPosition, heroData.normalSkill2.areaOfEffect);
            foreach (var hit in hits)
            {
                Hero enemy = hit.GetComponent<Hero>();
                if (enemy != null && enemy.teamId != teamId)
                {
                    enemy.TakeDamage(heroData.normalSkill2.damage, this);
                }
            }
        }

        public void UseUltimate(Vector3 targetPosition)
        {
            if (isDead || heroData.ultimateSkill == null) return;
            if (ultimateCooldown > 0) return;
            if (currentMana < heroData.ultimateSkill.manaCost) return;

            currentMana -= heroData.ultimateSkill.manaCost;
            ultimateCooldown = heroData.ultimateSkill.cooldown;

            // Simple damage skill implementation
            Collider[] hits = Physics.OverlapSphere(targetPosition, heroData.ultimateSkill.areaOfEffect);
            foreach (var hit in hits)
            {
                Hero enemy = hit.GetComponent<Hero>();
                if (enemy != null && enemy.teamId != teamId)
                {
                    enemy.TakeDamage(heroData.ultimateSkill.damage, this);
                }
            }
        }

        public void TakeDamage(float damage, Hero attacker)
        {
            if (isDead) return;

            // Apply armor reduction
            float damageReduction = armor / (armor + 100);
            float actualDamage = damage * (1 - damageReduction);

            currentHealth -= actualDamage;

            if (currentHealth <= 0)
            {
                Die(attacker);
            }
        }

        private void Die(Hero killer)
        {
            isDead = true;
            deaths++;
            respawnTimer = RESPAWN_TIME;

            // Grant gold to killer
            if (killer != null)
            {
                killer.kills++;
                killer.AddGold(300); // Kill reward
            }

            // Hide the hero
            GetComponent<Renderer>().enabled = false;
            GetComponent<Collider>().enabled = false;
        }

        private void HandleRespawn()
        {
            respawnTimer -= Time.deltaTime;
            if (respawnTimer <= 0)
            {
                Respawn();
            }
        }

        private void Respawn()
        {
            isDead = false;
            currentHealth = maxHealth;
            currentMana = maxMana;

            // Move to spawn point
            Vector3 spawnPoint = teamId == 0 ? new Vector3(-50, 0, -50) : new Vector3(50, 0, 50);
            transform.position = spawnPoint;

            // Show the hero
            GetComponent<Renderer>().enabled = true;
            GetComponent<Collider>().enabled = true;
        }

        private void RegenerateHealthMana()
        {
            if (heroData == null) return;

            currentHealth = Mathf.Min(currentHealth + heroData.healthRegen * Time.deltaTime, maxHealth);
            currentMana = Mathf.Min(currentMana + heroData.manaRegen * Time.deltaTime, maxMana);
        }

        public void AddGold(int amount)
        {
            currentGold += amount;
        }

        public void AddExperience(int amount)
        {
            currentExp += amount;
            // Simple leveling (every 100 exp)
            if (currentExp >= currentLevel * 100)
            {
                LevelUp();
            }
        }

        private void LevelUp()
        {
            currentLevel++;
            InitializeStats();
        }

        // Getters for cooldowns
        public float GetSkill1Cooldown() => skill1Cooldown;
        public float GetSkill2Cooldown() => skill2Cooldown;
        public float GetUltimateCooldown() => ultimateCooldown;
        public float GetMaxHealth() => maxHealth;
        public float GetMaxMana() => maxMana;
    }
}
