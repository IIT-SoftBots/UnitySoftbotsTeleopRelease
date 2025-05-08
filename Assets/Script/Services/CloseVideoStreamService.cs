using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Alterego;  // Usa il namespace corretto per il tuo servizio
using RosMessageTypes.UnityRoboticsDemo;
using UnityEngine.SceneManagement;

public class CloseVideoStreamService : MonoBehaviour
{
    ROSConnection ros;
    private string robot_namespace;
    public string serviceName = "close_gstreamer"; // Sostituisci con il nome del tuo servizio
    private string FullServiceName;
    public GameObject LeftVideoPlane;
    public GameObject RightVideoPlane;
    void Start()
    {
        
        robot_namespace = SwitchRobotManager.robot_name;
        FullServiceName = "/" + robot_namespace + serviceName;
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterRosService<CloseGStreamerRequest, CloseGStreamerResponse>(FullServiceName);
    }

    public void CallService()
    {

        // Crea la richiesta del servizio senza argomenti
        var request = new CloseGStreamerRequest(); // Assicurati che YourServiceRequest non richieda argomenti

        // Chiama il servizio
        if (!PlayPausePublisher.enable_out)
        {
            LeftVideoPlane.SetActive(false);
            RightVideoPlane.SetActive(false);
            ros.SendServiceMessage<LaunchGStreamerResponse>(FullServiceName, request, ServiceResponseHandler);
        }
       
    }

    void ServiceResponseHandler(LaunchGStreamerResponse response)
    {
        // Gestisci la risposta del servizio
        Debug.Log("Response received: " + response.success);
    }
}
