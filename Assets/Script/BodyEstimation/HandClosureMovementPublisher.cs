using RosMessageTypes.Std;
using Unity.Robotics.Core;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;

public class HandClosureMovementPublisher : MonoBehaviour
{
    ROSConnection ros;
    public OVRHand leftHand;
    public OVRHand rightHand;

    private string RightHandTopic = "/right_hand_closure";
    private string LefHandTopic = "/left_hand_closure";

    private bool initialized = false;

    private string robot_namespace;

    private void Start()
    {
        robot_namespace = SwitchRobotManager.robot_name;
    }

    void Update()
    {
        if (PlayPausePublisher.enable_out)
        {
            // Setto il topic in base alla variabile booleana
            if (!initialized)
                SetTopic();


            if (leftHand.IsTracked)
            {
                float leftHandGrabStrength = Mathf.Clamp(GetHandGrabStrength(leftHand), 0f, 1f);

                //Debug.Log("Left Hand Grab Strength: " + leftHandGrabStrength);
                ros.Publish(LefHandTopic, new Float64Msg(leftHandGrabStrength));


            }

            if (rightHand.IsTracked)
            {
                float rightHandGrabStrength = Mathf.Clamp(GetHandGrabStrength(rightHand), 0f, 1f);
                ros.Publish(RightHandTopic, new Float64Msg(rightHandGrabStrength));
            }

        }


    }

    void SetTopic()
    {

        RightHandTopic = "/" + robot_namespace + RightHandTopic;
        LefHandTopic = "/" + robot_namespace + LefHandTopic;
        // Ricordati di registrare nuovamente il publisher se cambi il topic
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<Float64Msg>(RightHandTopic);
        ros.RegisterPublisher<Float64Msg>(LefHandTopic);
        initialized = true;

    }
    float GetHandGrabStrength(OVRHand hand)
    {
        // Valori da 0 (mano completamente aperta) a 1 (mano completamente chiusa)
        float grabStrength = hand.GetFingerPinchStrength(OVRHand.HandFinger.Index) * 0.5f;
        grabStrength += hand.GetFingerPinchStrength(OVRHand.HandFinger.Middle) * 0.5f;
        grabStrength += hand.GetFingerPinchStrength(OVRHand.HandFinger.Ring) * 0.5f;
        grabStrength += hand.GetFingerPinchStrength(OVRHand.HandFinger.Pinky) * 0.5f;
        grabStrength += hand.GetFingerPinchStrength(OVRHand.HandFinger.Thumb) * 0.5f;

        return grabStrength;
    }
}
