using Oculus.Platform;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UDPListener : MonoBehaviour
{
    public static UDPListener Instance { get; private set; } // Singleton instance

    public int listenPort = 8888; // Porta su cui ascoltare
    private UdpClient udpClient;
    private Thread listenThread;
    private bool isListening = true;

    public struct RobotInfo
    {
        public DateTime LastMessageTime; // Ultimo momento in cui è stato ricevuto un messaggio dal robot
        public string IPAddress; // Indirizzo IP del robot

        public RobotInfo(DateTime lastMessageTime, string ipAddress)
        {
            LastMessageTime = lastMessageTime;
            IPAddress = ipAddress;
        }
    }


    private Dictionary<string, RobotInfo> robotInfoDict = new Dictionary<string, RobotInfo>();

    void Awake()
    {
        // Implementazione del pattern Singleton per assicurarsi che esista una sola istanza di UDPListener
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Rende persistente l'oggetto tra le scene
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Inizializza il client UDP e avvia il thread di ascolto
        udpClient = new UdpClient(listenPort);
        listenThread = new Thread(new ThreadStart(ListenForMessages));
        listenThread.IsBackground = true;
        listenThread.Start();
        // Chiama RemoveInactiveRobots ogni 20 secondi per rimuovere i robot inattivi
        InvokeRepeating("RemoveInactiveRobots", 20f, 20f);
    }



    private void ListenForMessages()
    {
        // Ascolta i messaggi in arrivo e li processa
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, listenPort);

        while (isListening)
        {

            try
            {
                byte[] receivedBytes = udpClient.Receive(ref endPoint);
                string message = Encoding.ASCII.GetString(receivedBytes);
                Debug.LogWarning("[UDPListener] Messaggio ricevuto: " + message);

                // Processa il messaggio ricevuto
                ProcessMessage(message, endPoint);
            }
            catch (SocketException e)
            {
                Debug.LogWarning("Errore durante la ricezione del messaggio: " + e.Message);
            }
        }
    }

    private void ProcessMessage(string message, IPEndPoint senderEndPoint)
    {
        // Elabora il messaggio ricevuto, estraendo il nome del robot e l'indirizzo IP del mittente
        string[] messageParts = message.Split(' ');
        if (messageParts.Length >= 5)
        {
            string robotName = messageParts[2];
            string senderIPAddress = messageParts[4].TrimEnd('!');

            // Aggiorna o aggiunge le informazioni del robot nel dizionario
            if (!robotInfoDict.ContainsKey(robotName))
            {
                robotInfoDict.Add(robotName, new RobotInfo(DateTime.Now, senderIPAddress));
            }
            else
            {
                robotInfoDict[robotName] = new RobotInfo(DateTime.Now, senderIPAddress);
            }

            // Invia una risposta al mittente
            string responseMessage = "Grazie";
            byte[] responseBytes = Encoding.ASCII.GetBytes(responseMessage);
            using (UdpClient sender = new UdpClient())
            {
                sender.Send(responseBytes, responseBytes.Length, senderEndPoint);
                //Debug.Log("[UDPListener] Messaggio inviato a " + senderIPAddress + ":8888:  " + responseMessage);
            }
        }
        else
        {
            Debug.LogError("[UDPListener] Formato del messaggio non riconosciuto.");
        }
    }

    public string GetRobotIPAddress(string robotName)
    {
        // Restituisce l'indirizzo IP di un robot specifico
        if (robotInfoDict.TryGetValue(robotName, out RobotInfo info))
        {
            return info.IPAddress;
        }
        else
        {
            return null; // Gestisci il caso in cui il robot non è presente
        }
    }

    public List<string> GetRobotNames()
    {
        // Restituisce una lista dei nomi dei robot attualmente noti
        return new List<string>(robotInfoDict.Keys);
    }

    private void RemoveInactiveRobots()
    {
        // Rimuove i robot inattivi che non hanno inviato messaggi negli ultimi 20 secondi
        List<string> robotsToRemove = new List<string>();

        foreach (var robot in robotInfoDict)
        {
            if ((DateTime.Now - robot.Value.LastMessageTime).TotalSeconds > 20)
            {
                robotsToRemove.Add(robot.Key);
            }
        }

        foreach (var robotName in robotsToRemove)
        {
            robotInfoDict.Remove(robotName);
        }
    }

    void OnApplicationQuit()
    {
        // Gestisce la chiusura dell'applicazione, interrompendo il thread di ascolto e chiudendo il client UDP
        Debug.LogWarning("[UDPListener] Quit");
        isListening = false;
        udpClient.Close();
        listenThread.Abort();
    }
}
