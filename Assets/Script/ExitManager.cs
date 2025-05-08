using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.UnityRoboticsDemo;
using UnityEngine.SceneManagement;

public class ExitManager : MonoBehaviour
{
    private string robot_namespace = SwitchRobotManager.robot_name;
    //public CloseVideoStreamService closeVideoStreamService;
    public GameObject UIObject;
    public GameObject PausePainting;
    public GameObject CustomROSConnector;
    public GameObject VideoStreamReceiver;
    public RobotSpawner robotSpawner; // Aggiungi questa riga
    private bool wasButtonPressed = false;


    private void Update()
    {
        if (!PlayPausePublisher.enable_out)
        {
            bool buttonPressed = OVRInput.Get(OVRInput.Button.Two, OVRInput.Controller.RTouch);
            if (wasButtonPressed && !buttonPressed)
            {
                PausePainting.SetActive(false);
                CustomROSConnector.SetActive(false);
                VideoStreamReceiver.SetActive(false);

                // Chiama il metodo per riattivare tutti i robot
                if (robotSpawner != null)
                {
                    robotSpawner.ActivateAllRobots();
                }

                // Chiudi il videostream 
                //closeVideoStreamService.CallService();
                UIObject.SetActive(true);

            }
            wasButtonPressed = buttonPressed;
        }
    }


}
