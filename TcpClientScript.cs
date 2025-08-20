using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System;

public class TcpClientScript : MonoBehaviour
{
    private TcpClient client;
    private NetworkStream stream;
    private Thread receiveThread;
    private bool isRunning = false;

    public bool IsConnected => client != null && client.Connected;

    public event Action<string> OnDataReceived; // �����ƨƥ�

    public void Connect(string host, int port)
    {
        if (IsConnected)
        {
            Debug.LogWarning("�w�g�s�u�A�L�ݭ��Ƴs�u");
            return;
        }

        try
        {
            client = new TcpClient(host, port);
            stream = client.GetStream();
            isRunning = true;

            receiveThread = new Thread(ReceiveLoop);
            receiveThread.IsBackground = true;
            receiveThread.Start();

            Debug.Log("���\�s�u�� HandTracking.exe�I");
            
        }
        catch (SocketException e)
        {
            Debug.LogError("TCP �s�u����: " + e.Message);
        }
    }

    private void ReceiveLoop()
    {
        byte[] buffer = new byte[8192];
        try
        {
            while (isRunning)
            {
                if (stream.DataAvailable)
                {
                    int length = stream.Read(buffer, 0, buffer.Length);
                    if (length > 0)
                    {
                        string data = Encoding.UTF8.GetString(buffer, 0, length);
                        OnDataReceived?.Invoke(data);
                    }
                }
                else
                {
                    Thread.Sleep(10);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("������Ƶo�Ϳ��~: " + e.Message);
        }
    }

    private void OnApplicationQuit()
    {
        isRunning = false;
        stream?.Close();
        client?.Close();
        receiveThread?.Join();
    }
}
