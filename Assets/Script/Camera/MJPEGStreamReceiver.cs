using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;

public class MJPEGStreamReceiver : MonoBehaviour
{
    public RawImage rawImage; // Reference to the RawImage to display the video
    private Texture2D texture;
    private UdpClient udpClient;
    public int port = 5000; // Port to receive the stream

    private void Start()
    {
        // Initialize the texture and assign it to the RawImage
        texture = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        rawImage.texture = texture;

        // Set up the UDP client to listen on the specified port
        udpClient = new UdpClient(port);

        // Start listening for the UDP stream
        StartCoroutine(ReceiveStream());
    }
    private List<byte> packetBuffer = new List<byte>();

    private IEnumerator ReceiveStream()
    {
        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, port); // Listen from any sender
        //Debug.Log("Starting to listen for MJPEG stream on port " + port);

        while (true)
        {
            if (udpClient.Available > 0)
            {
                var receivedBytes = udpClient.Receive(ref remoteEndPoint);
                packetBuffer.AddRange(receivedBytes);  // Aggiungi il pacchetto al buffer

                // Log per verificare i dati ricevuti
                //Debug.Log("Received packet of size: " + receivedBytes.Length);
                //Debug.Log("Buffer size: " + packetBuffer.Count);

                // Verifica i marcatori solo quando hai un buffer abbastanza grande
                int start = IndexOf(packetBuffer.ToArray(), new byte[] { 0xFF, 0xD8 });
                int end = IndexOf(packetBuffer.ToArray(), new byte[] { 0xFF, 0xD9 });

                // Se trovi entrambi i marcatori, estrai il frame JPEG
                if (start >= 0 && end >= 0)
                {
                    byte[] jpegData = new byte[end - start + 2];
                    packetBuffer.CopyTo(start, jpegData, 0, jpegData.Length);

                    // Rimuovi i dati processati dal buffer
                    packetBuffer.RemoveRange(0, end + 2);

                    // Carica i dati dell'immagine nella texture
                    texture.LoadImage(jpegData);
                    //Debug.Log("Loaded JPEG image of size: " + jpegData.Length);
                }
            }
            else
            {
                //Debug.Log("No packets available, waiting...");
                yield return new WaitForSeconds(0.1f); // Pausa per non sovraccaricare il thread
            }
        }
    }



    private int IndexOf(byte[] source, byte[] pattern)
    {
        for (int i = 0; i < source.Length - pattern.Length; i++)
        {
            bool match = true;
            for (int j = 0; j < pattern.Length; j++)
            {
                if (source[i + j] != pattern[j])
                {
                    match = false;
                    break;
                }
            }
            if (match)
                return i;
        }
        return -1;
    }


    private void OnApplicationQuit()
    {
        // Close the UDP client when the application quits
        udpClient.Close();
    }
}
