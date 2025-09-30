using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class TCPUDP : MonoBehaviour
{
    int port = 1234;
    Socket serverSocket;
    Socket clientSocket;
    EndPoint clientEndPoint;
    bool connected = false;
    byte[] buffer = new byte[1024];

    float contador = 0f;

    void Start()
    {
        // Crear socket servidor
        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, port);

        try
        {
            serverSocket.Bind(ipep);
            serverSocket.Listen(10);
            Debug.Log("Servidor escuchando en puerto " + port);
        }
        catch (Exception e)
        {
            Debug.LogError("Error al iniciar servidor: " + e.Message);
        }
    }

    void Update()
    {
        contador += Time.deltaTime;

        if (contador >= 5f)
        {
            if (!connected)
            {
                try
                {
                    if (serverSocket.Poll(1000, SelectMode.SelectRead)) // Ver si hay cliente
                    {
                        clientSocket = serverSocket.Accept();
                        clientEndPoint = clientSocket.RemoteEndPoint;
                        connected = true;
                        Debug.Log("Cliente conectado: " + clientEndPoint.ToString());
                    }
                }
                catch (Exception e)
                {
                    Debug.Log("Fallo al aceptar cliente: " + e.Message);
                }
            }
            else
            {
                try
                {
                    if (clientSocket.Available > 0)
                    {
                        int recv = clientSocket.Receive(buffer);
                        string msg = Encoding.ASCII.GetString(buffer, 0, recv);
                        Debug.Log("Recibido: " + msg);

                        // Enviar respuesta
                        string respuesta = "Servidor dice: " + msg;
                        byte[] data = Encoding.ASCII.GetBytes(respuesta);
                        clientSocket.Send(data);
                    }
                }
                catch (Exception e)
                {
                    Debug.Log("Error en comunicación: " + e.Message);
                    connected = false;
                }
            }

            contador = 0f;
        }
    }

    void OnApplicationQuit()
    {
        if (clientSocket != null)
            clientSocket.Close();
        if (serverSocket != null)
            serverSocket.Close();
    }
}
