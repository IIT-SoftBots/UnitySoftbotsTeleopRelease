using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.Core;
using RosMessageTypes.Std; // Importa il namespace per i messaggi standard di ROS
using UnityEngine.XR;
using System.Collections;
using System.Collections.Generic;
using Oculus.Platform;
using TMPro;
using System.Net;

public class PlayPausePublisher : MonoBehaviour
{
    private ROSConnection ros;
    private MeshRenderer meshRenderer;
    private bool wasButtonPressed = false;
    private bool initialized = false;
    private string robot_namespace;
    public string Topic = "";
    private string buttonATopic;
    public static bool enable_out = false;
                                  // Riferimenti alle telecamere
    public Camera LeftEye;
    public Camera RightEye;
    public GameObject StereoCamera;
    //public GameObject PassThrough;

    void Start()
    {
        robot_namespace = SwitchRobotManager.robot_name;
    }

    void FixedUpdate()
    {

        // Setto il topic in base alla variabile booleana
        if (!initialized)
            SetTopic();


        bool buttonPressed = OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.RTouch);
        if (wasButtonPressed && !buttonPressed)
        {
            enable_out = !enable_out;
            PublishMessage();
            // Cambia la telecamera attiva
            SwitchCamera();
        }
        wasButtonPressed = buttonPressed;
    }

    void SetTopic()
    {
        buttonATopic = "/" + robot_namespace + Topic;


        if (!string.IsNullOrEmpty(buttonATopic))
        {

            // Ricordati di registrare nuovamente il publisher se cambi il topic
            ros = ROSConnection.GetOrCreateInstance();
            ros.RegisterPublisher<BoolMsg>(buttonATopic);
        }
        initialized = true;
    }

    void PublishMessage()
    {
        BoolMsg message = new BoolMsg(true);  // Invia sempre true quando premuto
        ros.Publish(buttonATopic, message);

        // Aspetta un frame
        StartCoroutine(PublishFalse());
    }

    IEnumerator PublishFalse()
    {
        yield return new WaitForEndOfFrame();
        BoolMsg message = new BoolMsg(false);
        ros.Publish(buttonATopic, message);
    }
    void SwitchCamera()
    {
        // Cambia l'attivazione delle telecamere
        if (LeftEye.enabled)
        {
            LeftEye.enabled = false;
            RightEye.enabled = false;
            StereoCamera.SetActive(true);
            //PassThrough.SetActive(true);
        }
        else
        {
            LeftEye.enabled = true;
            RightEye.enabled = true;
            StereoCamera.SetActive(false);
            //PassThrough.SetActive(false);

        }
    }
}

