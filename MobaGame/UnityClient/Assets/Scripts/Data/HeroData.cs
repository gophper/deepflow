using UnityEngine;

namespace MobaGame.Data
{
    public enum HeroClass
    {
        Tank,
        Fighter,
        Assassin,
        Mage,
        Marksman
    }

    public enum AttackType
    {
        Melee,
        Ranged
    }

    public enum SkillType
    {
        NormalSkill,
        Ultimate,
        Passive
    }

    [CreateAssetMenu(fileName = "NewHero", menuName = "MOBA/Hero Data")]
    public class HeroData : ScriptableObject
    {
        [Header("Basic Info")]
        public string heroName;
        public HeroClass heroClass;
        public AttackType attackType;

        [Header("Stats")]
        public float maxHealth = 1000f;
        public float maxMana = 500f;
        public float healthRegen = 5f;
        public float manaRegen = 3f;
        public float moveSpeed = 5f;
        public float attackDamage = 50f;
        public float magicPower = 0f;
        public float armor = 30f;
        public float magicResist = 30f;
        public float attackSpeed = 1f;
        public float attackRange = 2f;

        [Header("Skills")]
        public SkillData normalSkill1;
        public SkillData normalSkill2;
        public SkillData ultimateSkill;
        public PassiveData passive;

        [Header("Level Scaling")]
        public float healthPerLevel = 85f;
        public float manaPerLevel = 30f;
        public float attackDamagePerLevel = 3f;
        public float armorPerLevel = 3f;
        public float magicResistPerLevel = 1.25f;
    }

    [CreateAssetMenu(fileName = "NewSkill", menuName = "MOBA/Skill Data")]
    public class SkillData : ScriptableObject
    {
        public string skillName;
        public SkillType skillType;
        public float cooldown = 5f;
        public float manaCost = 50f;
        public float damage = 100f;
        public float range = 5f;
        public float areaOfEffect = 0f;
        public bool isProjectile = false;
        public float projectileSpeed = 10f;

        [TextArea(3, 5)]
        public string description;
    }

    [CreateAssetMenu(fileName = "NewPassive", menuName = "MOBA/Passive Data")]
    public class PassiveData : ScriptableObject
    {
        public string passiveName;
        public PassiveEffectType effectType;
        public float value;

        [TextArea(3, 5)]
        public string description;
    }

    public enum PassiveEffectType
    {
        BonusAttackDamage,
        BonusArmor,
        BonusMagicResist,
        BonusMoveSpeed,
        BonusAttackSpeed,
        BonusHealthRegen,
        BonusManaRegen
    }
}
