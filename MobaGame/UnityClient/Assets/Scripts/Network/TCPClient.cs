using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace MobaGame.Network
{
    public class TCPClient : MonoBehaviour
    {
        [Header("Connection")]
        public string serverIP = "127.0.0.1";
        public int serverPort = 8888;

        private TcpClient client;
        private NetworkStream stream;
        private Thread receiveThread;
        private bool isConnected = false;

        public bool IsConnected => isConnected;

        void Start()
        {
            // Auto-connect on start (can be changed to manual)
            // ConnectToServer();
        }

        public void ConnectToServer()
        {
            try
            {
                client = new TcpClient();
                client.Connect(serverIP, serverPort);
                stream = client.GetStream();
                isConnected = true;

                receiveThread = new Thread(new ThreadStart(ReceiveData));
                receiveThread.IsBackground = true;
                receiveThread.Start();

                Debug.Log($"Connected to server at {serverIP}:{serverPort}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to connect to server: {e.Message}");
                isConnected = false;
            }
        }

        public void Disconnect()
        {
            if (isConnected)
            {
                isConnected = false;

                if (receiveThread != null && receiveThread.IsAlive)
                {
                    receiveThread.Abort();
                }

                if (stream != null)
                {
                    stream.Close();
                }

                if (client != null)
                {
                    client.Close();
                }

                Debug.Log("Disconnected from server");
            }
        }

        public void SendData(string data)
        {
            if (!isConnected || stream == null) return;

            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(data + "\n");
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error sending data: {e.Message}");
            }
        }

        private void ReceiveData()
        {
            byte[] buffer = new byte[4096];

            while (isConnected)
            {
                try
                {
                    if (stream.DataAvailable)
                    {
                        int bytesRead = stream.Read(buffer, 0, buffer.Length);
                        string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        
                        // Process on main thread
                        UnityMainThreadDispatcher.Instance.Enqueue(() => ProcessReceivedData(data));
                    }
                    Thread.Sleep(10);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error receiving data: {e.Message}");
                    isConnected = false;
                    break;
                }
            }
        }

        private void ProcessReceivedData(string data)
        {
            Debug.Log($"Received: {data}");
            
            // Parse and handle different message types
            if (data.StartsWith("CHAT:"))
            {
                string chatMessage = data.Substring(5);
                UI.ChatUI chatUI = FindObjectOfType<UI.ChatUI>();
                if (chatUI != null)
                {
                    chatUI.AddMessage(chatMessage);
                }
            }
            // Add more message type handlers as needed
        }

        void OnApplicationQuit()
        {
            Disconnect();
        }

        void OnDestroy()
        {
            Disconnect();
        }
    }
}
