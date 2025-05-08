using UnityEngine;
using NativeWebSocket;
using System.Threading.Tasks;
using System;

public class VideoReceiver : MonoBehaviour
{
    private WebSocket websocket;
    private Texture2D leftTexture;
    private Texture2D rightTexture;

    [SerializeField]
    private Material leftEyeMaterial;
    [SerializeField]
    private Material rightEyeMaterial;

    async void OnEnable()
    {
        Debug.Log("Object enabled, initializing WebSocket and textures...");

        string ipAddress = UDPListener.Instance.GetRobotIPAddress(SwitchRobotManager.robot_name);

        // Sostituisci l'ultimo elemento dell'indirizzo IP con "1"
        string[] ipParts = ipAddress.Split('.');
        if (ipParts.Length == 4)
        {
            string lastPart = ipParts[3];
            // Cambia l'ultima cifra dell'ultima parte dell'IP in "1"
            string modifiedLastPart = lastPart.Substring(0, lastPart.Length - 1) + "1";
            ipParts[3] = modifiedLastPart;
            // Ricostruiamo l'IP modificato
            ipAddress = string.Join(".", ipParts);
        }
        else
        {
            Debug.LogError("Invalid IP address format!");
            return;
        }

        Debug.Log($"Modified IP address: {ipAddress}");

        // Crea nuove texture per entrambi gli occhi
        leftTexture = new Texture2D(1280, 720);
        rightTexture = new Texture2D(1280, 720);

        // Assegna le texture ai materiali
        leftEyeMaterial.mainTexture = leftTexture;
        rightEyeMaterial.mainTexture = rightTexture;

        // Crea e connetti il WebSocket
        websocket = new WebSocket("ws://" + ipAddress + ":8765");
        websocket.OnMessage += HandleMessage;
        websocket.OnError += (e) => Debug.LogError($"Error: {e}");
        websocket.OnClose += (e) => Debug.Log("Connection closed");

        try
        {
            await websocket.Connect();
            Debug.Log("WebSocket connected!");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error connecting WebSocket: {e}");
        }
    }


    void HandleMessage(byte[] data)
    {
        try
        {
            string jsonString = System.Text.Encoding.UTF8.GetString(data);
            StereoMessage message = JsonUtility.FromJson<StereoMessage>(jsonString);

            if (message.type == "stereo_frame")
            {
                // Aggiorna entrambe le texture
                byte[] leftImageData = Convert.FromBase64String(message.left);
                byte[] rightImageData = Convert.FromBase64String(message.right);

                leftTexture.LoadImage(leftImageData);
                rightTexture.LoadImage(rightImageData);

                leftTexture.Apply();
                rightTexture.Apply();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error processing message: {e}");
        }
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket?.DispatchMessageQueue();
#endif
    }

    private async void OnDisable()
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            Debug.Log("Closing WebSocket because object is being disabled...");
            await websocket.Close();
        }
    }

    private async void OnApplicationQuit()
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            Debug.Log("Closing WebSocket because application is quitting...");
            await websocket.Close();
        }
    }
}

[Serializable]
public class StereoMessage
{
    public string type;
    public double timestamp;
    public string left;
    public string right;
}
