using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.Core;
using RosMessageTypes.UnityRoboticsDemo;
using System;
using RosMessageTypes.Geometry;
using static OVRInput;
using System.Runtime.CompilerServices;
using System.Collections.Specialized;
using RosMessageTypes.Std;

public class HandClosurePublisher : MonoBehaviour
{
    ROSConnection ros;
    private string robot_namespace;
    private string Topic;
    // Publish every N seconds
    [SerializeField]
    private double m_PublishRateHz = 100f;

    double PublishPeriodSeconds => 1.0f / m_PublishRateHz;
    double m_LastPublishTimeSeconds;
    double triggerValue = 0.0;

    private bool initialized = false;
    bool ShouldPublishMessage => Clock.NowTimeInSeconds > m_LastPublishTimeSeconds + PublishPeriodSeconds;
    public enum PublishOption
    {
        None,
        RightHand,
        LeftHand
    }
    public PublishOption publishOption = PublishOption.None;
    private void OnEnable()
    {
        // start the ROS connection
        robot_namespace = SwitchRobotManager.robot_name;
        m_LastPublishTimeSeconds = Clock.time + PublishPeriodSeconds;
    }

    private void Update()
    {
        if (ShouldPublishMessage && PlayPausePublisher.enable_out)
        {

            // Setto il topic in base alla variabile booleana
            if (!initialized)
            {
                SetTopic();
            }


            PublishMessage();
        }
    }
    // Metodo per cambiare il topic e assicurarsi che solo un booleano sia true
    void SetTopic()
    {
        switch (publishOption)
        {
            case PublishOption.RightHand:
                Topic = "/" + robot_namespace + "/right_hand_closure";
                break;
            case PublishOption.LeftHand:
                Topic = "/" + robot_namespace + "/left_hand_closure";
                break;
            default:
                Topic = ""; // Nessun topic selezionato
                break;
        }

        if (!string.IsNullOrEmpty(Topic))
        {
            // Ricordati di registrare nuovamente il publisher se cambi il topic
            ros = ROSConnection.GetOrCreateInstance();
            ros.RegisterPublisher<Float64Msg>(Topic);
        }
        initialized = true;
    }

    void PublishMessage()
    {

        // Determina quale trigger leggere in base a publishOption
        switch (publishOption)
        {
            case PublishOption.RightHand:
                triggerValue = OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger);
                break;
            case PublishOption.LeftHand:
                triggerValue = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger);
                break;
            case PublishOption.None:
                triggerValue = 0.0;
                break;
            default:
                // Potresti voler gestire questo caso in modo diverso
                return; 
        }

        // Crea e pubblica il messaggio
        Float64Msg msg = new Float64Msg(triggerValue);
        ros.Publish(Topic, msg);

        m_LastPublishTimeSeconds = Clock.FrameStartTimeInSeconds;
    }
}