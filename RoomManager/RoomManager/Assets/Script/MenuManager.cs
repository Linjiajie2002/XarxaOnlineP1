using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Net;
using System.Net.Sockets;

public class MenuManager : MonoBehaviour
{
    [Header("UI Elements")]
    public InputField ipPortInput;
    public InputField playerNameInput;
    public Button createServerBtn;
    public Button joinClientBtn;
    public Text errorText;

    private void Start()
    {
        createServerBtn.onClick.AddListener(CreateServer);
        joinClientBtn.onClick.AddListener(JoinAsClient);

        // 设置默认IP和端口
        ipPortInput.text = "127.0.0.1:8888";
        playerNameInput.text = "Player" + UnityEngine.Random.Range(1, 100);
    }

    private void CreateServer()
    {
        if (string.IsNullOrEmpty(playerNameInput.text))
        {
            ShowError("Please enter your name");
            return;
        }

        try
        {
            string input = ipPortInput.text;
            string[] parts = input.Split(':');

            if (parts.Length != 2)
            {
                ShowError("Invalid format! Use IP:Port (e.g., 127.0.0.1:8888)");
                return;
            }

            string ip = parts[0];
            int port = int.Parse(parts[1]);

            // 验证端口是否为4位数
            if (port < 1000 || port > 9999)
            {
                ShowError("Port must be a 4-digit number (1000-9999)");
                return;
            }

            // 检查端口是否已被占用
            if (!GameSettings.TryRegisterPort(port))
            {
                ShowError($"Port {port} is already in use!");
                return;
            }

            GameSettings.ServerIP = ip;
            GameSettings.ServerPort = port;
            GameSettings.IsServer = true;
            GameSettings.PlayerName = playerNameInput.text;

            // 加载服务器场景
            SceneManager.LoadScene("Server");
        }
        catch (Exception e)
        {
            ShowError($"Error: {e.Message}");
        }
    }

    private void JoinAsClient()
    {
        if (string.IsNullOrEmpty(playerNameInput.text))
        {
            ShowError("Please enter your name");
            return;
        }

        try
        {
            string input = ipPortInput.text;
            string[] parts = input.Split(':');

            if (parts.Length != 2)
            {
                ShowError("Invalid format! Use IP:Port (e.g., 127.0.0.1:8888)");
                return;
            }

            string ip = parts[0];
            int port = int.Parse(parts[1]);

            // 测试连接
            if (TestConnection(ip, port))
            {
                GameSettings.ServerIP = ip;
                GameSettings.ServerPort = port;
                GameSettings.IsServer = false;
                GameSettings.PlayerName = playerNameInput.text;

                SceneManager.LoadScene("Client");
            }
            else
            {
                ShowError("Cannot connect to server! Server may be full or offline.");
            }
        }
        catch (Exception e)
        {
            ShowError($"Connection error: {e.Message}");
        }
    }

    private bool TestConnection(string ip, int port)
    {
        try
        {
            TcpClient client = new TcpClient();
            var result = client.BeginConnect(ip, port, null, null);
            bool success = result.AsyncWaitHandle.WaitOne(2000); // 2秒超时

            if (success)
            {
                client.EndConnect(result);
                client.Close();
                return true;
            }
            else
            {
                client.Close();
                return false;
            }
        }
        catch
        {
            return false;
        }
    }

    private void ShowError(string message)
    {
        errorText.text = message;
        errorText.gameObject.SetActive(true);
        Invoke("HideError", 3f);
    }

    private void HideError()
    {
        errorText.gameObject.SetActive(false);
    }
}