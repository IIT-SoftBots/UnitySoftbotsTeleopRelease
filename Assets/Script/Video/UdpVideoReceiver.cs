using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class UdpVideoReceiver : MonoBehaviour
{
    public int port = 5000; // Porta UDP su cui ricevere il flusso video
    public Renderer videoRenderer; // Renderer del GameObject su cui vuoi riprodurre il video

    private UdpClient udpClient;
    private Thread receiveThread;
    private Texture2D videoTexture;
    private bool isReceiving = false;

    private int width = 1280;  // Larghezza dell'immagine
    private int height = 720;  // Altezza dell'immagine

    void Start()
    {
        // Inizializza la texture con le dimensioni appropriate
        videoTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);

        // Avvia il thread di ricezione
        StartReceiving();
    }

    void StartReceiving()
    {
        udpClient = new UdpClient(port);
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
        isReceiving = true;
    }

    void ReceiveData()
    {
        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, port);
        while (isReceiving)
        {
            try
            {
                // Ricevi i dati UDP
                byte[] data = udpClient.Receive(ref remoteEndPoint);

                // Se riceviamo dati, aggiorniamo la texture
                if (data != null && data.Length > 0)
                {
                    UpdateTexture(data);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Errore nella ricezione del flusso UDP: " + e.Message);
            }
        }
    }

    void UpdateTexture(byte[] imageData)
    {
        // Copia i dati RGBA direttamente nella texture
        videoTexture.LoadRawTextureData(imageData);
        videoTexture.Apply();

        // Assegna la texture al materiale del renderer
        videoRenderer.material.mainTexture = videoTexture;
    }

    void OnApplicationQuit()
    {
        isReceiving = false;
        if (receiveThread != null && receiveThread.IsAlive)
        {
            receiveThread.Abort();
        }

        if (udpClient != null)
        {
            udpClient.Close();
        }
    }
}
