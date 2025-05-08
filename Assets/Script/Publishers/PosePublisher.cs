using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;
using static OVRInput;
using RosMessageTypes.UnityRoboticsDemo;
using System;
using Unity.Robotics.Core;
using System.Runtime.CompilerServices;
using System.Collections.Specialized;

public class PosePublisher : MonoBehaviour
{
    ROSConnection ros;
    private string robot_namespace;
    private string PoseTopic;
    public GameObject sphere;

    [SerializeField]
    private double m_PublishRateHz = 100f;

    double PublishPeriodSeconds => 1.0f / m_PublishRateHz;
    double m_LastPublishTimeSeconds;
    //bool ShouldPublishMessage => Clock.NowTimeInSeconds > m_LastPublishTimeSeconds + PublishPeriodSeconds;

    private bool initialized = false;
    private Vector3 position;
    private Quaternion orientation;
    private OVRCameraRig cameraRig;

    public enum PublishOption
    {
        None,
        RightHand,
        LeftHand,
        Head
    }
    public PublishOption publishOption = PublishOption.None;
    void Start()
    {
        robot_namespace = SwitchRobotManager.robot_name;
        // Trova l'OVRCameraRig nella scena
        cameraRig = FindObjectOfType<OVRCameraRig>();
    }

    private void Update()
    {
        if (IsSphereGreen())
        {
            // Setto il topic in base alla variabile booleana
            if (!initialized) 
                SetTopic();

            PublishMessage();
        }
    }


    bool IsSphereGreen()
    {
        MeshRenderer meshRenderer = sphere.GetComponent<MeshRenderer>();
        return meshRenderer.material.color == Color.green;
    }


    // Metodo per cambiare il topic e assicurarsi che solo un booleano sia true
    void SetTopic()
    {
        switch (publishOption)
        {
            case PublishOption.RightHand:
                PoseTopic = "/" + robot_namespace + "/hand_R_ref";
                break;
            case PublishOption.LeftHand:
                PoseTopic = "/" + robot_namespace + "/hand_L_ref";
                break;
            case PublishOption.Head:
                PoseTopic = "/" + robot_namespace + "/head_pose";
                break;
            default:
                PoseTopic = ""; // Nessun topic selezionato
                break;
        }

        if (!string.IsNullOrEmpty(PoseTopic))
        {
            // Ricordati di registrare nuovamente il publisher se cambi il topic
            ros = ROSConnection.GetOrCreateInstance();
            ros.RegisterPublisher<PoseMsg>(PoseTopic);
        }
        initialized = true;
    }
    void PublishMessage()
    {

        switch (publishOption)
        {
            case PublishOption.RightHand:
                // Ottieni posizione e rotazione del controller destro
                Transform rightController = cameraRig.rightControllerAnchor;
                position = rightController.position;
                orientation = rightController.rotation;

                break;
            case PublishOption.LeftHand:
                // Ottieni posizione e rotazione del controller sinistro
                Transform leftController = cameraRig.leftControllerAnchor;
                position = leftController.position;
                orientation = leftController.rotation;

                break;
            case PublishOption.Head:
                // Get the center eye anchor, which represents the headset
                Transform centerEye = cameraRig.centerEyeAnchor;
                position = centerEye.position;
                orientation = centerEye.rotation;
                break;
        }

        PointMsg positionMsg = new PointMsg(position.x, position.y, position.z);
        QuaternionMsg orientationMsg = new QuaternionMsg(orientation.x, orientation.y, orientation.z, orientation.w);
        PoseMsg poseMsg = new PoseMsg(positionMsg, orientationMsg);

        ros.Publish(PoseTopic, poseMsg);

        //m_LastPublishTimeSeconds = Clock.FrameStartTimeInSeconds;
    }

}
