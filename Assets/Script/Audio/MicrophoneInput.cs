using UnityEngine;
using UnityEngine.Android;
using System.Net;
using System.Net.Sockets;
using System;

public class MicrophoneInput : MonoBehaviour
{
    private AudioSource audioSource;
    private string microphoneDevice;
    private UdpClient udpClient;
    private IPEndPoint endPoint;
    private int sampleRate = 44100;
    private int chunkSize = 1024; // Dimensione del blocco (in byte)

    void Start()
    {
        // Richiedi il permesso per il microfono
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            Permission.RequestUserPermission(Permission.Microphone);

        // Verifica la disponibilità del microfono
        if (Microphone.devices.Length > 0)
        {
            microphoneDevice = Microphone.devices[0];
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = Microphone.Start(microphoneDevice, true, 10, sampleRate);
            //audioSource.loop = true;
            audioSource.Play();

            udpClient = new UdpClient();
            endPoint = new IPEndPoint(IPAddress.Parse("192.168.0.91"), 5000);
        }
        else
        {
            Debug.LogError("Nessun microfono trovato.");
        }
    }

    void Update()
    {
        if (Microphone.IsRecording(microphoneDevice))
        {
            float[] audioData = new float[audioSource.clip.samples * audioSource.clip.channels];
            audioSource.clip.GetData(audioData, 0);

            short[] shortData = new short[audioData.Length];
            for (int i = 0; i < audioData.Length; i++)
            {
                shortData[i] = (short)(audioData[i] * short.MaxValue);
            }

            byte[] byteData = new byte[shortData.Length * 2];
            Buffer.BlockCopy(shortData, 0, byteData, 0, byteData.Length);

            // Suddividi i dati in blocchi
            int offset = 0;
            while (offset < byteData.Length)
            {
                int size = Math.Min(chunkSize, byteData.Length - offset);
                byte[] chunk = new byte[size];
                Array.Copy(byteData, offset, chunk, 0, size);

                // Invia ogni blocco tramite UDP
                udpClient.Send(chunk, chunk.Length, endPoint);

                offset += size;
            }
        }
    }

    void OnApplicationQuit()
    {
        if (Microphone.IsRecording(microphoneDevice))
        {
            Microphone.End(microphoneDevice);
        }
        udpClient.Close();
    }
}
