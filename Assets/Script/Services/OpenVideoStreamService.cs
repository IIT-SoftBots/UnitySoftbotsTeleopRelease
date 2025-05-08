using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Alterego; // Namespace del servizio ROS
using System.Net;

public class OpenVideoStreamService : MonoBehaviour
{
    ROSConnection ros;
    private bool serviceRegistered = false; // Per evitare duplicazioni
    private string robot_namespace;
    public string serviceName = "launch_gstreamer"; // Nome del servizio
    private string FullServiceName;
    public GameObject LeftVideoPlane;
    public GameObject RightVideoPlane;

    private void OnEnable()
    {
        ros = ROSConnection.GetOrCreateInstance();

        if (!serviceRegistered)
        {
            robot_namespace = SwitchRobotManager.robot_name; // Nome del robot
            FullServiceName = "/" + robot_namespace + serviceName;
            ros.RegisterRosService<LaunchGStreamerRequest, LaunchGStreamerResponse>(FullServiceName);
            serviceRegistered = true;
        }

        CallService();
    }

    void CallService()
    {
        // Rileva automaticamente l'indirizzo IP locale
        string localIPAddress = GetLocalIPAddress();

        if (string.IsNullOrEmpty(localIPAddress))
        {
            Debug.LogError("Unable to detect local IP address.");
            return;
        }

        // Creazione della richiesta
        var request = new LaunchGStreamerRequest
        {
            ip_address = localIPAddress // Imposta l'indirizzo IP nella richiesta
        };

        // Attiva i piani video prima di chiamare il servizio
        LeftVideoPlane.SetActive(true);
        RightVideoPlane.SetActive(true);

        // Chiama il servizio
        ros.SendServiceMessage<LaunchGStreamerResponse>(FullServiceName, request, ServiceResponseHandler);
    }

    void ServiceResponseHandler(LaunchGStreamerResponse response)
    {
        if (response.success)
        {
            Debug.Log("GStreamer pipeline launched successfully.");
        }
        else
        {
            Debug.LogError("Failed to launch GStreamer pipeline.");
        }
    }

    /// <summary>
    /// Ottiene l'indirizzo IP locale della macchina.
    /// </summary>
    /// <returns>Indirizzo IP locale come stringa.</returns>
    private string GetLocalIPAddress()
    {
        foreach (var networkInterface in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
        {
            if (networkInterface.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return networkInterface.ToString();
            }
        }
        return null; // Nessun indirizzo IP valido trovato
    }
}
