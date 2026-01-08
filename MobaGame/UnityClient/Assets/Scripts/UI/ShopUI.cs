using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MobaGame.Data;
using MobaGame.Gameplay;
using System.Collections.Generic;

namespace MobaGame.UI
{
    public class ShopUI : MonoBehaviour
    {
        [Header("Shop Data")]
        public ItemData[] availableItems;
        public Dictionary<HeroClass, int[]> recommendedBuilds = new Dictionary<HeroClass, int[]>();

        [Header("UI Elements")]
        public GameObject shopPanel;
        public Transform itemListContainer;
        public GameObject itemButtonPrefab;
        public Button openShopButton;
        public Button closeShopButton;

        [Header("Player")]
        public Hero playerHero;

        private List<ItemData> purchasedItems = new List<ItemData>();

        void Start()
        {
            InitializeRecommendedBuilds();

            if (shopPanel != null)
            {
                shopPanel.SetActive(false);
            }

            if (openShopButton != null)
            {
                openShopButton.onClick.AddListener(OpenShop);
            }

            if (closeShopButton != null)
            {
                closeShopButton.onClick.AddListener(CloseShop);
            }

            CreateItemButtons();
        }

        void InitializeRecommendedBuilds()
        {
            // Simple recommended builds (item IDs)
            recommendedBuilds[HeroClass.Tank] = new int[] { 2, 3, 8 };
            recommendedBuilds[HeroClass.Fighter] = new int[] { 0, 4, 2 };
            recommendedBuilds[HeroClass.Assassin] = new int[] { 0, 5, 6 };
            recommendedBuilds[HeroClass.Mage] = new int[] { 1, 9, 3 };
            recommendedBuilds[HeroClass.Marksman] = new int[] { 0, 4, 6 };
        }

        void CreateItemButtons()
        {
            if (availableItems == null || itemListContainer == null) return;

            foreach (ItemData item in availableItems)
            {
                GameObject buttonObj;
                if (itemButtonPrefab != null)
                {
                    buttonObj = Instantiate(itemButtonPrefab, itemListContainer);
                }
                else
                {
                    buttonObj = new GameObject($"ItemButton_{item.itemName}");
                    buttonObj.transform.SetParent(itemListContainer);
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

                buttonText.text = $"{item.itemName}\n{item.cost}G";
                button.onClick.AddListener(() => PurchaseItem(item));
            }
        }

        public void OpenShop()
        {
            if (shopPanel != null)
            {
                shopPanel.SetActive(true);
            }
        }

        public void CloseShop()
        {
            if (shopPanel != null)
            {
                shopPanel.SetActive(false);
            }
        }

        void PurchaseItem(ItemData item)
        {
            if (playerHero == null) return;

            if (playerHero.currentGold >= item.cost)
            {
                playerHero.currentGold -= item.cost;
                purchasedItems.Add(item);
                ApplyItemStats(item);
                Debug.Log($"Purchased {item.itemName}");
            }
            else
            {
                Debug.Log("Not enough gold!");
            }
        }

        void ApplyItemStats(ItemData item)
        {
            // In a full implementation, you would apply these stats to the hero
            // For MVP, we'll just track the purchase
            Debug.Log($"Applied stats from {item.itemName}");
        }

        void Update()
        {
            // Press B to toggle shop (for testing)
            if (Input.GetKeyDown(KeyCode.B))
            {
                if (shopPanel != null)
                {
                    shopPanel.SetActive(!shopPanel.activeSelf);
                }
            }
        }
    }
}
