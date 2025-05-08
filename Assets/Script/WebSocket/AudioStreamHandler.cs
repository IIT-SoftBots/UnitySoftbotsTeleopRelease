using UnityEngine;
using NativeWebSocket;
using System.Threading.Tasks;
using System;

public class AudioStreamHandler: MonoBehaviour
{
    private WebSocket websocket;
    private AudioClip micClip;
    private float[] micBuffer;
    private int micPosition = 0;
    private const int SAMPLE_RATE = 44100;
    private const int CHUNK_SIZE = 1024;

    async void OnEnable()
    {
        Debug.Log("Audio sender enabled, initializing WebSocket and microphone...");

        // Ottieni e modifica l'IP come nel VideoReceiver
        string ipAddress = UDPListener.Instance.GetRobotIPAddress(SwitchRobotManager.robot_name);
        string[] ipParts = ipAddress.Split('.');
        if (ipParts.Length == 4)
        {
            string lastPart = ipParts[3];
            string modifiedLastPart = lastPart.Substring(0, lastPart.Length - 1) + "1";
            ipParts[3] = modifiedLastPart;
            ipAddress = string.Join(".", ipParts);
        }
        else
        {
            Debug.LogError("Invalid IP address format!");
            return;
        }
        Debug.Log($"Modified IP address for audio: {ipAddress}");

        // Inizializza microfono
        micClip = Microphone.Start(null, true, 1, SAMPLE_RATE);
        micBuffer = new float[CHUNK_SIZE];

        // Crea e connetti il WebSocket
        websocket = new WebSocket("ws://" + ipAddress + ":8766");
        websocket.OnError += (e) => Debug.LogError($"Audio WebSocket Error: {e}");
        websocket.OnClose += (e) => Debug.Log("Audio connection closed");

        try
        {
            await websocket.Connect();
            Debug.Log("Audio WebSocket connected!");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error connecting Audio WebSocket: {e}");
        }
    }

    async void SendMicrophoneData()
    {
        if (!Microphone.IsRecording(null)) return;

        int pos = Microphone.GetPosition(null);
        if (pos < 0 || pos == micPosition) return;

        // Leggi dati dal microfono
        micClip.GetData(micBuffer, 0);

        // Converti in Int16 e poi in base64
        byte[] audioData = new byte[CHUNK_SIZE * 2];
        for (int i = 0; i < CHUNK_SIZE; i++)
        {
            short sample = (short)(micBuffer[i] * 32767f);
            byte[] bytes = BitConverter.GetBytes(sample);
            audioData[i * 2] = bytes[0];
            audioData[i * 2 + 1] = bytes[1];
        }

        string base64Audio = Convert.ToBase64String(audioData);

        // Invia al server
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            await websocket.SendText(JsonUtility.ToJson(new AudioMessage
            {
                type = "audio",
                data = base64Audio,
                timestamp = Time.time
            }));
        }

        micPosition = pos;
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket?.DispatchMessageQueue();
#endif
        SendMicrophoneData();
    }

    private async void OnDisable()
    {
        Debug.Log("Closing Audio WebSocket because object is being disabled...");
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            await websocket.Close();
        }

        if (Microphone.IsRecording(null))
        {
            Microphone.End(null);
        }
    }

    private async void OnApplicationQuit()
    {
        Debug.Log("Closing Audio WebSocket because application is quitting...");
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            await websocket.Close();
        }

        if (Microphone.IsRecording(null))
        {
            Microphone.End(null);
        }
    }
}

[Serializable]
public class AudioMessage
{
    public string type;
    public string data;
    public float timestamp;
}