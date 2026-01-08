using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace MobaGame.UI
{
    public class LobbyScreen : MonoBehaviour
    {
        public TextMeshProUGUI welcomeText;
        public Button quickMatchButton;
        public Button exitButton;

        void Start()
        {
            string playerName = PlayerPrefs.GetString("PlayerName", "Player");
            if (welcomeText != null)
            {
                welcomeText.text = $"Welcome, {playerName}!";
            }

            if (quickMatchButton != null)
            {
                quickMatchButton.onClick.AddListener(OnQuickMatchClick);
            }

            if (exitButton != null)
            {
                exitButton.onClick.AddListener(OnExitClick);
            }
        }

        void OnQuickMatchClick()
        {
            // Go to hero selection
            SceneManager.LoadScene("HeroSelectionScene");
        }

        void OnExitClick()
        {
            Application.Quit();
        }
    }
}
