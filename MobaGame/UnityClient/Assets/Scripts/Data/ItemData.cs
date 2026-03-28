using UnityEngine;

namespace MobaGame.Data
{
    public enum ItemType
    {
        PhysicalAttack,
        MagicPower,
        Defense,
        MagicResist,
        AttackSpeed,
        CriticalChance,
        Lifesteal,
        MovementSpeed,
        Health,
        Mana
    }

    [CreateAssetMenu(fileName = "NewItem", menuName = "MOBA/Item Data")]
    public class ItemData : ScriptableObject
    {
        public string itemName;
        public int itemId;
        public ItemType itemType;
        public int cost;
        public Sprite icon;

        [Header("Stats")]
        public float attackDamage;
        public float magicPower;
        public float armor;
        public float magicResist;
        public float attackSpeed;
        public float criticalChance;
        public float lifesteal;
        public float moveSpeed;
        public float bonusHealth;
        public float bonusMana;

        [TextArea(3, 5)]
        public string description;
    }
}
