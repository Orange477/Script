using UnityEngine;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

public class HandTrackingLauncher : MonoBehaviour
{
    private Process handTrackingProcess;
    private TcpClient tcpClient;
    private NetworkStream stream;
    private byte[] buffer = new byte[8192];
    private StringBuilder dataBuffer = new StringBuilder();

    void Start()
    {
        LaunchHandTracking();
        ConnectToPythonAsync("127.0.0.1", 5005);
    }

    void LaunchHandTracking()
    {
        string exePath = Path.Combine(Application.streamingAssetsPath, "Mediapipe/HandTracking.exe");
        if (!File.Exists(exePath))
        {
            UnityEngine.Debug.LogError("找不到 HandTracking.exe");
            return;
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = exePath,
            Arguments = "--host 127.0.0.1 --port 5005",
            UseShellExecute = false,
            CreateNoWindow = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        handTrackingProcess = Process.Start(startInfo);
        handTrackingProcess.OutputDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) UnityEngine.Debug.Log("[HandTracking] " + e.Data); };
        handTrackingProcess.BeginOutputReadLine();
        handTrackingProcess.ErrorDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) UnityEngine.Debug.LogError("[HandTracking] ERROR: " + e.Data); };
        handTrackingProcess.BeginErrorReadLine();
    }

    async void ConnectToPythonAsync(string host, int port)
    {
        tcpClient = new TcpClient();
        while (!tcpClient.Connected)
        {
            try
            {
                await tcpClient.ConnectAsync(host, port);
            }
            catch
            {
                await Task.Delay(1000);
            }
        }
        stream = tcpClient.GetStream();
        UnityEngine.Debug.Log("TCP 連線成功！");
        _ = ReceiveDataAsync();
    }

    async Task ReceiveDataAsync()
    {
        while (tcpClient.Connected)
        {
            int length = 0;
            try
            {
                length = await stream.ReadAsync(buffer, 0, buffer.Length);
            }
            catch
            {
                UnityEngine.Debug.LogError("連線讀取失敗");
                break;
            }

            if (length == 0)
            {
                UnityEngine.Debug.LogWarning("連線已關閉");
                break;
            }

            string received = Encoding.UTF8.GetString(buffer, 0, length);
            dataBuffer.Append(received);

            string dataStr = dataBuffer.ToString();
            int newlineIndex;
            while ((newlineIndex = dataStr.IndexOf('\n')) >= 0)
            {
                string jsonLine = dataStr.Substring(0, newlineIndex);
                dataBuffer.Remove(0, newlineIndex + 1);
                dataStr = dataBuffer.ToString();

                ProcessLandmarkData(jsonLine);
            }
        }
    }

    void ProcessLandmarkData(string json)
    {
        try
        {
            var hands = HandManager.JsonHelper.FromJson<HandManager.HandData>(json);
            if (hands != null && HandManager.instance != null)
            {
                HandManager.instance.EnqueueHandData(hands);
            }
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError("JSON 解析錯誤: " + e.Message);
        }
    }

    void OnApplicationQuit()
    {
        if (handTrackingProcess != null && !handTrackingProcess.HasExited)
        {
            handTrackingProcess.Kill();
        }
        if (tcpClient != null)
        {
            tcpClient.Close();
        }
    }
}
