using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MobaGame.Gameplay;

namespace MobaGame.UI
{
    public class GameHUD : MonoBehaviour
    {
        [Header("Hero Stats")]
        public Hero playerHero;
        public Slider healthBar;
        public Slider manaBar;
        public TextMeshProUGUI levelText;
        public TextMeshProUGUI goldText;

        [Header("Skills")]
        public Image skill1Icon;
        public Image skill2Icon;
        public Image ultimateIcon;
        public TextMeshProUGUI skill1CooldownText;
        public TextMeshProUGUI skill2CooldownText;
        public TextMeshProUGUI ultimateCooldownText;

        [Header("Scoreboard")]
        public TextMeshProUGUI kdaText;
        public TextMeshProUGUI gameTimeText;

        void Update()
        {
            if (playerHero == null) return;

            UpdateStats();
            UpdateSkillCooldowns();
            UpdateScoreboard();
        }

        void UpdateStats()
        {
            // Health bar
            if (healthBar != null)
            {
                healthBar.maxValue = playerHero.GetMaxHealth();
                healthBar.value = playerHero.currentHealth;
            }

            // Mana bar
            if (manaBar != null)
            {
                manaBar.maxValue = playerHero.GetMaxMana();
                manaBar.value = playerHero.currentMana;
            }

            // Level
            if (levelText != null)
            {
                levelText.text = $"Lv.{playerHero.currentLevel}";
            }

            // Gold
            if (goldText != null)
            {
                goldText.text = $"{playerHero.currentGold}G";
            }
        }

        void UpdateSkillCooldowns()
        {
            // Skill 1
            float skill1CD = playerHero.GetSkill1Cooldown();
            if (skill1CooldownText != null)
            {
                skill1CooldownText.text = skill1CD > 0 ? skill1CD.ToString("F1") : "";
            }
            if (skill1Icon != null)
            {
                skill1Icon.color = skill1CD > 0 ? new Color(0.5f, 0.5f, 0.5f, 1f) : Color.white;
            }

            // Skill 2
            float skill2CD = playerHero.GetSkill2Cooldown();
            if (skill2CooldownText != null)
            {
                skill2CooldownText.text = skill2CD > 0 ? skill2CD.ToString("F1") : "";
            }
            if (skill2Icon != null)
            {
                skill2Icon.color = skill2CD > 0 ? new Color(0.5f, 0.5f, 0.5f, 1f) : Color.white;
            }

            // Ultimate
            float ultCD = playerHero.GetUltimateCooldown();
            if (ultimateCooldownText != null)
            {
                ultimateCooldownText.text = ultCD > 0 ? ultCD.ToString("F1") : "";
            }
            if (ultimateIcon != null)
            {
                ultimateIcon.color = ultCD > 0 ? new Color(0.5f, 0.5f, 0.5f, 1f) : Color.white;
            }
        }

        void UpdateScoreboard()
        {
            if (kdaText != null)
            {
                kdaText.text = $"K/D/A: {playerHero.kills}/{playerHero.deaths}/{playerHero.assists}";
            }

            if (gameTimeText != null)
            {
                float time = Core.GameManager.Instance != null ? Core.GameManager.Instance.gameTime : 0f;
                int minutes = Mathf.FloorToInt(time / 60f);
                int seconds = Mathf.FloorToInt(time % 60f);
                gameTimeText.text = $"{minutes:00}:{seconds:00}";
            }
        }
    }
}
