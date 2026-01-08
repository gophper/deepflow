using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.Threading;

namespace MobaServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== MOBA Game Server ===");
            Console.WriteLine("Starting server...\n");

            TCPServer server = new TCPServer(8888);
            server.Start();

            Console.WriteLine("Press 'q' to quit server.");
            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.KeyChar == 'q' || key.KeyChar == 'Q')
                {
                    break;
                }
            }

            server.Stop();
            Console.WriteLine("Server stopped.");
        }
    }

    public class TCPServer
    {
        private TcpListener? listener;
        private List<ClientHandler> clients = new List<ClientHandler>();
        private int port;
        private bool isRunning = false;
        private Thread? acceptThread;

        public TCPServer(int port)
        {
            this.port = port;
        }

        public void Start()
        {
            try
            {
                listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                isRunning = true;

                Console.WriteLine($"Server started on port {port}");
                Console.WriteLine($"Waiting for connections...\n");

                acceptThread = new Thread(AcceptClients);
                acceptThread.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error starting server: {e.Message}");
            }
        }

        public void Stop()
        {
            isRunning = false;

            foreach (var client in clients)
            {
                client.Disconnect();
            }

            clients.Clear();

            if (listener != null)
            {
                listener.Stop();
            }

            if (acceptThread != null && acceptThread.IsAlive)
            {
                acceptThread.Join(1000);
            }
        }

        private void AcceptClients()
        {
            while (isRunning)
            {
                try
                {
                    if (listener?.Pending() == true)
                    {
                        TcpClient client = listener.AcceptTcpClient();
                        ClientHandler handler = new ClientHandler(client, this);
                        clients.Add(handler);
                        handler.Start();

                        Console.WriteLine($"Client connected. Total clients: {clients.Count}");
                    }
                    Thread.Sleep(100);
                }
                catch (Exception e)
                {
                    if (isRunning)
                    {
                        Console.WriteLine($"Error accepting client: {e.Message}");
                    }
                }
            }
        }

        public void BroadcastMessage(string message, ClientHandler? sender = null)
        {
            lock (clients)
            {
                foreach (var client in clients)
                {
                    if (client != sender && client.IsConnected)
                    {
                        client.SendMessage(message);
                    }
                }
            }
        }

        public void RemoveClient(ClientHandler client)
        {
            lock (clients)
            {
                clients.Remove(client);
                Console.WriteLine($"Client disconnected. Total clients: {clients.Count}");
            }
        }
    }

    public class ClientHandler
    {
        private TcpClient client;
        private NetworkStream stream;
        private TCPServer server;
        private Thread? receiveThread;
        private bool isConnected = false;
        private string playerId;

        public bool IsConnected => isConnected;

        public ClientHandler(TcpClient client, TCPServer server)
        {
            this.client = client;
            this.server = server;
            this.stream = client.GetStream();
            this.playerId = Guid.NewGuid().ToString().Substring(0, 8);
        }

        public void Start()
        {
            isConnected = true;
            receiveThread = new Thread(ReceiveData);
            receiveThread.Start();

            // Send welcome message
            SendMessage($"Welcome to MOBA Server! Your ID: {playerId}");
        }

        public void Disconnect()
        {
            isConnected = false;

            if (receiveThread != null && receiveThread.IsAlive)
            {
                receiveThread.Join(1000);
            }

            if (stream != null)
            {
                stream.Close();
            }

            if (client != null)
            {
                client.Close();
            }
        }

        public void SendMessage(string message)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message + "\n");
                stream.Write(data, 0, data.Length);
                stream.Flush();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error sending to client {playerId}: {e.Message}");
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
                        if (bytesRead > 0)
                        {
                            string data = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                            ProcessMessage(data);
                        }
                    }
                    Thread.Sleep(10);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error receiving from client {playerId}: {e.Message}");
                    isConnected = false;
                    break;
                }
            }

            server.RemoveClient(this);
        }

        private void ProcessMessage(string message)
        {
            Console.WriteLine($"[{playerId}] {message}");

            // Parse message type
            if (message.StartsWith("CHAT:"))
            {
                string chatMessage = message.Substring(5);
                string broadcastMessage = $"CHAT:[{playerId}] {chatMessage}";
                server.BroadcastMessage(broadcastMessage, this);
            }
            else if (message.StartsWith("MOVE:"))
            {
                // Broadcast player movement
                server.BroadcastMessage($"MOVE:{playerId}:{message.Substring(5)}", this);
            }
            else if (message.StartsWith("ATTACK:"))
            {
                // Broadcast player attack
                server.BroadcastMessage($"ATTACK:{playerId}:{message.Substring(7)}", this);
            }
            else if (message.StartsWith("SKILL:"))
            {
                // Broadcast skill usage
                server.BroadcastMessage($"SKILL:{playerId}:{message.Substring(6)}", this);
            }
            else
            {
                // Echo back unknown messages
                SendMessage($"Unknown command: {message}");
            }
        }
    }
}
