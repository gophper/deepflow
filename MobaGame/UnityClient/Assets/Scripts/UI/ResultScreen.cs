using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using MobaGame.Core;

namespace MobaGame.UI
{
    public class ResultScreen : MonoBehaviour
    {
        public TextMeshProUGUI resultTitleText;
        public TextMeshProUGUI statsText;
        public Button returnToLobbyButton;

        void Start()
        {
            DisplayResults();

            if (returnToLobbyButton != null)
            {
                returnToLobbyButton.onClick.AddListener(OnReturnToLobby);
            }
        }

        void DisplayResults()
        {
            if (GameManager.Instance == null) return;

            bool isVictory = GameManager.Instance.winningTeam == GameManager.Instance.localPlayerTeam;
            
            if (resultTitleText != null)
            {
                resultTitleText.text = isVictory ? "VICTORY!" : "DEFEAT";
                resultTitleText.color = isVictory ? Color.green : Color.red;
            }

            // Get player stats
            GameObject player = GameManager.Instance.localPlayer;
            if (player != null && statsText != null)
            {
                Gameplay.Hero hero = player.GetComponent<Gameplay.Hero>();
                if (hero != null)
                {
                    float gameTime = GameManager.Instance.gameTime;
                    int minutes = Mathf.FloorToInt(gameTime / 60f);
                    int seconds = Mathf.FloorToInt(gameTime % 60f);

                    statsText.text = $"Game Time: {minutes:00}:{seconds:00}\n" +
                        $"K/D/A: {hero.kills}/{hero.deaths}/{hero.assists}\n" +
                        $"Gold Earned: {hero.currentGold}\n" +
                        $"Final Level: {hero.currentLevel}";
                }
            }
        }

        void OnReturnToLobby()
        {
            SceneManager.LoadScene("LobbyScene");
        }
    }
}
