using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace MobaGame.UI
{
    public class LoginScreen : MonoBehaviour
    {
        public TMP_InputField usernameInput;
        public Button loginButton;
        public TextMeshProUGUI statusText;

        void Start()
        {
            if (loginButton != null)
            {
                loginButton.onClick.AddListener(OnLoginClick);
            }
        }

        void OnLoginClick()
        {
            string username = usernameInput != null ? usernameInput.text : "Player";
            
            if (string.IsNullOrEmpty(username))
            {
                if (statusText != null)
                    statusText.text = "Please enter a username";
                return;
            }

            // Save username
            PlayerPrefs.SetString("PlayerName", username);
            PlayerPrefs.Save();

            // Go to lobby
            SceneManager.LoadScene("LobbyScene");
        }
    }
}
