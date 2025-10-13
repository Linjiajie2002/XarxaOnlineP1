using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class ServerManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Text serverInfoText;
    public Text clientsText;
    public Button backButton;

    private TcpListener tcpListener;
    private Thread listenerThread;
    private bool isRunning = true;
    private List<ClientHandler> clients = new List<ClientHandler>();
    private const int MAX_CLIENTS = 5;

    private void Start()
    {
        backButton.onClick.AddListener(GoBackToMenu);

        // 显示服务器信息
        serverInfoText.text = $"Server: {GameSettings.ServerIP}:{GameSettings.ServerPort}\n" +
                             $"Host: {GameSettings.PlayerName}";

        StartServer();
    }

    private void StartServer()
    {
        try
        {
            IPAddress ipAddress = IPAddress.Parse(GameSettings.ServerIP);
            tcpListener = new TcpListener(ipAddress, GameSettings.ServerPort);
            tcpListener.Start();

            listenerThread = new Thread(new ThreadStart(ListenForClients));
            listenerThread.IsBackground = true;
            listenerThread.Start();

            Debug.Log($"Server started on {GameSettings.ServerIP}:{GameSettings.ServerPort}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to start server: {e.Message}");
        }
    }

    private void ListenForClients()
    {
        while (isRunning)
        {
            try
            {
                if (tcpListener.Pending())
                {
                    TcpClient client = tcpListener.AcceptTcpClient();

                    if (clients.Count < MAX_CLIENTS)
                    {
                        ClientHandler clientHandler = new ClientHandler(client, this);
                        clients.Add(clientHandler);
                        Thread clientThread = new Thread(new ThreadStart(clientHandler.HandleClient));
                        clientThread.IsBackground = true;
                        clientThread.Start();

                        UpdateClientsUI();
                    }
                    else
                    {
                        // 服务器已满，拒绝连接
                        NetworkStream stream = client.GetStream();
                        byte[] response = System.Text.Encoding.UTF8.GetBytes("SERVER_FULL");
                        stream.Write(response, 0, response.Length);
                        client.Close();
                    }
                }
                Thread.Sleep(100);
            }
            catch (Exception e)
            {
                if (isRunning)
                    Debug.LogError($"Error in listener thread: {e.Message}");
            }
        }
    }

    public void RemoveClient(ClientHandler client)
    {
        if (clients.Contains(client))
        {
            clients.Remove(client);
            UpdateClientsUI();
        }
    }

    public void BroadcastMessage(string message, ClientHandler excludeClient = null)
    {
        foreach (var client in clients)
        {
            if (client != excludeClient)
            {
                client.SendMessage(message);
            }
        }
    }

    private void UpdateClientsUI()
    {
        // 在主线程更新UI
        UnityMainThreadDispatcher.Instance().Enqueue(() => {
            clientsText.text = $"Connected Clients: {clients.Count}/{MAX_CLIENTS}\n";
            foreach (var client in clients)
            {
                clientsText.text += $"- {client.ClientName}\n";
            }
        });
    }

    private void GoBackToMenu()
    {
        isRunning = false;
        GameSettings.UnregisterPort(GameSettings.ServerPort);

        if (tcpListener != null)
            tcpListener.Stop();

        foreach (var client in clients)
        {
            client.Disconnect();
        }

        SceneManager.LoadScene("Menu");
    }

    private void OnApplicationQuit()
    {
        GoBackToMenu();
    }
}

public class ClientHandler
{
    private TcpClient client;
    private NetworkStream stream;
    private ServerManager server;
    public string ClientName { get; private set; }

    public ClientHandler(TcpClient client, ServerManager server)
    {
        this.client = client;
        this.stream = client.GetStream();
        this.server = server;
    }

    public void HandleClient()
    {
        try
        {
            byte[] buffer = new byte[1024];
            int bytesRead;

            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                string message = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);

                if (string.IsNullOrEmpty(ClientName))
                {
                    // 第一个消息是客户端名称
                    ClientName = message;
                    server.BroadcastMessage($"PLAYER_JOINED:{ClientName}");
                }
                else
                {
                    // 处理其他消息（后续可用于聊天）
                    server.BroadcastMessage($"CHAT:{ClientName}:{message}");
                }

                server.UpdateClientsUI();
            }
        }
        catch (Exception e)
        {
            Debug.Log($"Client disconnected: {e.Message}");
        }
        finally
        {
            Disconnect();
        }
    }

    public void SendMessage(string message)
    {
        try
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }
        catch
        {
            Disconnect();
        }
    }

    public void Disconnect()
    {
        try
        {
            if (!string.IsNullOrEmpty(ClientName))
            {
                server.BroadcastMessage($"PLAYER_LEFT:{ClientName}");
            }

            stream?.Close();
            client?.Close();
            server.RemoveClient(this);
        }
        catch { }
    }
}