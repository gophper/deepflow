using UnityEngine;
using UnityEngine.SceneManagement;

namespace MobaGame.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game State")]
        public bool isGameStarted = false;
        public bool isGameOver = false;
        public int winningTeam = -1;
        public float gameTime = 0f;

        [Header("Players")]
        public GameObject localPlayer;
        public int localPlayerTeam = 0;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Update()
        {
            if (isGameStarted && !isGameOver)
            {
                gameTime += Time.deltaTime;
            }
        }

        public void StartGame()
        {
            isGameStarted = true;
            isGameOver = false;
            gameTime = 0f;
        }

        public void EndGame(int winnerTeam)
        {
            if (isGameOver) return;

            isGameOver = true;
            winningTeam = winnerTeam;
            
            Debug.Log($"Game Over! Team {winnerTeam} wins!");

            // Show result screen after delay
            Invoke("ShowResultScreen", 3f);
        }

        void ShowResultScreen()
        {
            SceneManager.LoadScene("ResultScene");
        }

        public void ReturnToLobby()
        {
            SceneManager.LoadScene("LobbyScene");
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}
