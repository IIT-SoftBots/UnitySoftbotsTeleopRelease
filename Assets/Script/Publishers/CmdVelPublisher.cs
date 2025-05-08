using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.Core;
using RosMessageTypes.UnityRoboticsDemo;
using System;
using RosMessageTypes.Geometry;
using static OVRInput;
using System.Runtime.CompilerServices;
using System.Collections.Specialized;

public class CmdVelPublisher : MonoBehaviour
{
    ROSConnection ros;
    private string robot_namespace;
    public string Topic = "";
    private string CmdVelTopic;

    // Publish every N seconds
    [SerializeField]
    private double m_PublishRateHz = 100f;

    double PublishPeriodSeconds => 1.0f / m_PublishRateHz;
    double m_LastPublishTimeSeconds;
    bool ShouldPublishMessage => Clock.NowTimeInSeconds > m_LastPublishTimeSeconds + PublishPeriodSeconds;
    // Used to determine how much time has elapsed since the last message was published
    private float timeElapsed;
    private bool initialized = false;
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
                SetTopic();

            PublishMessage();
        }
    }

    void SetTopic()
    {
        CmdVelTopic = "/" + robot_namespace + Topic;    

        if (!string.IsNullOrEmpty(CmdVelTopic))
        {
            // Ricordati di registrare nuovamente il publisher se cambi il topic
            ros = ROSConnection.GetOrCreateInstance();
            ros.RegisterPublisher<TwistMsg>(CmdVelTopic);

        }
        initialized = true;
    }

    void PublishMessage()
    {
        double linear_x = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y;
        double angular_z = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).x;
        Vector3Msg linear = new Vector3Msg(linear_x, 0, 0);
        Vector3Msg angular = new Vector3Msg(0, 0, -angular_z);

        TwistMsg CmdVel_msg = new TwistMsg(linear, angular);
        ros.Publish(CmdVelTopic, CmdVel_msg);

        m_LastPublishTimeSeconds = Clock.FrameStartTimeInSeconds;
    }
}
