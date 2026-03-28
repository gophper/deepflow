using UnityEngine;
using UnityEngine.SceneManagement;

namespace MobaGame.Core
{
    public class SceneLoader : MonoBehaviour
    {
        public static void LoadLoginScene()
        {
            SceneManager.LoadScene("LoginScene");
        }

        public static void LoadLobbyScene()
        {
            SceneManager.LoadScene("LobbyScene");
        }

        public static void LoadHeroSelectionScene()
        {
            SceneManager.LoadScene("HeroSelectionScene");
        }

        public static void LoadGameScene()
        {
            SceneManager.LoadScene("GameScene");
        }

        public static void LoadResultScene()
        {
            SceneManager.LoadScene("ResultScene");
        }
    }
}
