using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Net.Sockets;
using System.Threading;

public class ClientManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Text serverInfoText;
    public Text statusText;
    public Text playersText;
    public Button backButton;

    private TcpClient client;
    private NetworkStream stream;
    private Thread receiveThread;
    private bool isConnected = true;

    private void Start()
    {
        backButton.onClick.AddListener(GoBackToMenu);

        serverInfoText.text = $"Connected to: {GameSettings.ServerIP}:{GameSettings.ServerPort}";
        statusText.text = "Connecting...";

        ConnectToServer();
    }

    private void ConnectToServer()
    {
        try
        {
            client = new TcpClient();
            client.Connect(GameSettings.ServerIP, GameSettings.ServerPort);
            stream = client.GetStream();

            // 发送玩家名称
            byte[] data = System.Text.Encoding.UTF8.GetBytes(GameSettings.PlayerName);
            stream.Write(data, 0, data.Length);

            statusText.text = "Connected! Waiting for other players...";

            receiveThread = new Thread(new ThreadStart(ReceiveData));
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }
        catch (Exception e)
        {
            statusText.text = $"Connection failed: {e.Message}";
        }
    }

    private void ReceiveData()
    {
        byte[] buffer = new byte[1024];
        int bytesRead;

        try
        {
            while (isConnected && (bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                string message = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
                ProcessServerMessage(message);
            }
        }
        catch (Exception e)
        {
            if (isConnected)
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() => {
                    statusText.text = $"Disconnected: {e.Message}";
                });
            }
        }
    }

    private void ProcessServerMessage(string message)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() => {
            if (message.StartsWith("SERVER_FULL"))
            {
                statusText.text = "Server is full!";
                isConnected = false;
            }
            else if (message.StartsWith("PLAYER_JOINED:"))
            {
                string playerName = message.Substring("PLAYER_JOINED:".Length);
                playersText.text += $"\n{playerName} joined";
            }
            else if (message.StartsWith("PLAYER_LEFT:"))
            {
                string playerName = message.Substring("PLAYER_LEFT:".Length);
                playersText.text = playersText.text.Replace($"\n{playerName} joined", "");
            }
            else if (message.StartsWith("CHAT:"))
            {
                // 处理聊天消息（后续扩展）
                string[] parts = message.Split(':');
                if (parts.Length >= 3)
                {
                    string sender = parts[1];
                    string chatMessage = parts[2];
                    // 在这里显示聊天消息
                }
            }
        });
    }

    private void GoBackToMenu()
    {
        isConnected = false;

        try
        {
            stream?.Close();
            client?.Close();
            receiveThread?.Abort();
        }
        catch { }

        SceneManager.LoadScene("Menu");
    }

    private void OnApplicationQuit()
    {
        GoBackToMenu();
    }
}