using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using MobaGame.Data;

namespace MobaGame.UI
{
    public class HeroSelectionScreen : MonoBehaviour
    {
        [Header("Hero Data")]
        public HeroData[] availableHeroes;

        [Header("UI Elements")]
        public Transform heroButtonsContainer;
        public GameObject heroButtonPrefab;
        public TextMeshProUGUI heroNameText;
        public TextMeshProUGUI heroDescriptionText;
        public Button confirmButton;

        private HeroData selectedHero;
        private int selectedHeroIndex = -1;

        void Start()
        {
            CreateHeroButtons();

            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(OnConfirmClick);
                confirmButton.interactable = false;
            }
        }

        void CreateHeroButtons()
        {
            if (availableHeroes == null || heroButtonsContainer == null) return;

            for (int i = 0; i < availableHeroes.Length; i++)
            {
                int index = i; // Capture for closure
                HeroData hero = availableHeroes[i];

                GameObject buttonObj;
                if (heroButtonPrefab != null)
                {
                    buttonObj = Instantiate(heroButtonPrefab, heroButtonsContainer);
                }
                else
                {
                    buttonObj = new GameObject($"HeroButton_{hero.heroName}");
                    buttonObj.transform.SetParent(heroButtonsContainer);
                    buttonObj.AddComponent<Image>();
                    buttonObj.AddComponent<Button>();
                }

                Button button = buttonObj.GetComponent<Button>();
                TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText == null)
                {
                    GameObject textObj = new GameObject("Text");
                    textObj.transform.SetParent(buttonObj.transform);
                    buttonText = textObj.AddComponent<TextMeshProUGUI>();
                }

                buttonText.text = hero.heroName;
                button.onClick.AddListener(() => OnHeroSelected(index));
            }
        }

        void OnHeroSelected(int index)
        {
            selectedHeroIndex = index;
            selectedHero = availableHeroes[index];

            if (heroNameText != null)
            {
                heroNameText.text = selectedHero.heroName;
            }

            if (heroDescriptionText != null)
            {
                heroDescriptionText.text = $"Class: {selectedHero.heroClass}\n" +
                    $"Attack Type: {selectedHero.attackType}\n" +
                    $"HP: {selectedHero.maxHealth} | Mana: {selectedHero.maxMana}\n" +
                    $"Attack Damage: {selectedHero.attackDamage} | Move Speed: {selectedHero.moveSpeed}";
            }

            if (confirmButton != null)
            {
                confirmButton.interactable = true;
            }
        }

        void OnConfirmClick()
        {
            if (selectedHero == null) return;

            // Save selected hero
            PlayerPrefs.SetInt("SelectedHeroIndex", selectedHeroIndex);
            PlayerPrefs.Save();

            // Load game scene
            SceneManager.LoadScene("GameScene");
        }
    }
}
