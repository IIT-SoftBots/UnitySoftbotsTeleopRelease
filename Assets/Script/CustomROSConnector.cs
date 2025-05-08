using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using System;

/// <summary>
/// Gestisce la connessione ROS tra Unity e diversi robot nella rete.
/// </summary>
public class CustomROSConnector : MonoBehaviour
{
    // Riferimenti agli oggetti Unity
    [SerializeField] private GameObject UI;                  // Riferimento all'interfaccia utente
    [SerializeField] private RobotSpawner robotSpawner;     // Riferimento allo spawner dei robot

    // Variabili private per la gestione della connessione
    private string robotNamespace;                          // Nome del robot corrente
    private ROSConnection rosConnection;                    // Istanza della connessione ROS
    private bool isConnecting;                             // Flag per evitare connessioni multiple simultanee

    // Eventi
    /// <summary>
    /// Evento che viene triggherato quando cambia lo stato della connessione.
    /// Il bool indica se la connessione è avvenuta con successo (true) o è fallita (false)
    /// </summary>
    public event Action<bool> OnConnectionStatusChanged;

    /// <summary>
    /// Inizializza la connessione all'avvio del componente
    /// </summary>
    private void Start()
    {
        InitializeConnection();
    }

    /// <summary>
    /// Inizializza la connessione quando il componente viene abilitato
    /// </summary>
    private void OnEnable()
    {
        InitializeConnection();
    }

    /// <summary>
    /// Inizializza la connessione ROS e prepara l'ambiente
    /// </summary>
    private void InitializeConnection()
    {
        // Previene connessioni multiple simultanee
        if (isConnecting) return;

        // Disattiva l'UI e i robot esistenti
        UI?.SetActive(false);
        robotSpawner?.DeactivateAllRobots();

        // Ottiene il nome del robot da SwitchRobotManager
        robotNamespace = SwitchRobotManager.robot_name;
        // Ottiene o crea l'istanza di ROSConnection
        rosConnection = ROSConnection.GetOrCreateInstance();
        // Avvia la connessione al robot
        ConnectToRobot(robotNamespace);
    }

    /// <summary>
    /// Gestisce la connessione a un robot specifico
    /// </summary>
    /// <param name="robotName">Nome del robot a cui connettersi</param>
    private void ConnectToRobot(string robotName)
    {
        // Verifica che il nome del robot sia valido
        if (string.IsNullOrEmpty(robotName))
        {
            Debug.LogError("Robot name is null or empty");
            OnConnectionStatusChanged?.Invoke(false);
            return;
        }

        isConnecting = true;

        try
        {
            // Verifica che UDPListener sia disponibile
            if (UDPListener.Instance == null)
            {
                throw new Exception("UDPListener not available");
            }

            // Ottiene l'indirizzo IP del robot
            string ipAddress = UDPListener.Instance.GetRobotIPAddress(robotName);
            //string ipAddress = "10.240.20.24";
            if (string.IsNullOrEmpty(ipAddress))
            {
                throw new Exception($"IP address not found for robot {robotName}");
            }

            Debug.Log($"Attempting to connect to {robotName} at {ipAddress}");

            // Configura la connessione ROS
            rosConnection.RosIPAddress = ipAddress;
            rosConnection.RosPort = 10000;

            // Disconnette eventuali connessioni esistenti e si riconnette
            rosConnection.Disconnect();
            rosConnection.Connect();

            // Notifica il successo della connessione
            OnConnectionStatusChanged?.Invoke(true);
        }
        catch (Exception e)
        {
            // Gestisce e logga eventuali errori
            Debug.LogError($"Connection error: {e.Message}");
            OnConnectionStatusChanged?.Invoke(false);
        }
        finally
        {
            // Resetta il flag di connessione
            isConnecting = false;
        }
    }

    /// <summary>
    /// Gestisce la pulizia delle risorse quando il componente viene distrutto
    /// </summary>
    private void OnDestroy()
    {
        if (rosConnection != null)
        {
            rosConnection.Disconnect();
        }
    }

    /// <summary>
    /// Metodo pubblico per ritentare la connessione in caso di fallimento
    /// </summary>
    public void RetryConnection()
    {
        InitializeConnection();
    }
}