using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace MobaGame.UI
{
    public class ChatUI : MonoBehaviour
    {
        [Header("UI Elements")]
        public GameObject chatPanel;
        public TMP_InputField chatInput;
        public TextMeshProUGUI chatText;
        public ScrollRect chatScrollRect;
        public Button sendButton;

        private List<string> chatMessages = new List<string>();
        private const int MAX_MESSAGES = 50;

        void Start()
        {
            if (sendButton != null)
            {
                sendButton.onClick.AddListener(SendMessage);
            }

            if (chatInput != null)
            {
                chatInput.onSubmit.AddListener((text) => SendMessage());
            }
        }

        void Update()
        {
            // Press Enter to focus chat
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (chatInput != null && !chatInput.isFocused)
                {
                    chatInput.ActivateInputField();
                }
            }
        }

        public void SendMessage()
        {
            if (chatInput == null || string.IsNullOrEmpty(chatInput.text)) return;

            string playerName = PlayerPrefs.GetString("PlayerName", "Player");
            string message = $"{playerName}: {chatInput.text}";

            AddMessage(message);
            
            // TODO: Send to server for multiplayer
            // Network.ChatManager.SendChatMessage(chatInput.text);

            chatInput.text = "";
            chatInput.ActivateInputField();
        }

        public void AddMessage(string message)
        {
            chatMessages.Add(message);

            if (chatMessages.Count > MAX_MESSAGES)
            {
                chatMessages.RemoveAt(0);
            }

            UpdateChatDisplay();
        }

        void UpdateChatDisplay()
        {
            if (chatText == null) return;

            chatText.text = string.Join("\n", chatMessages);

            // Scroll to bottom
            if (chatScrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                chatScrollRect.verticalNormalizedPosition = 0f;
            }
        }

        public void ToggleChat()
        {
            if (chatPanel != null)
            {
                chatPanel.SetActive(!chatPanel.activeSelf);
            }
        }
    }
}
