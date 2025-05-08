using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using RosMessageTypes.Geometry;
using Unity.Robotics.ROSTCPConnector;

public class HandTrackingJobManager : MonoBehaviour
{
    [Header("Transform References")]
    public Transform Chest;
    public Transform Hips;
    public Transform leftHandObject;
    public Transform rightHandObject;
    public Transform leftArmUpperObject;
    public Transform rightArmUpperObject;

    private ROSConnection ros;
    private string leftWristTopic;
    private string rightWristTopic;

    // Native containers
    private NativeArray<float3> leftHandPosition;
    private NativeArray<float3> rightHandPosition;
    private NativeArray<quaternion> leftHandRotation;
    private NativeArray<quaternion> rightHandRotation;

    private void OnEnable()
    {
        InitializeROS();
        InitializeNativeArrays();
    }

    private void InitializeROS()
    {
        string robotNamespace = SwitchRobotManager.robot_name;
        leftWristTopic = $"/{robotNamespace}/hand_L_ref";
        rightWristTopic = $"/{robotNamespace}/hand_R_ref";
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<PoseMsg>(leftWristTopic);
        ros.RegisterPublisher<PoseMsg>(rightWristTopic);
    }

    private void InitializeNativeArrays()
    {
        leftHandPosition = new NativeArray<float3>(1, Allocator.Persistent);
        rightHandPosition = new NativeArray<float3>(1, Allocator.Persistent);
        leftHandRotation = new NativeArray<quaternion>(1, Allocator.Persistent);
        rightHandRotation = new NativeArray<quaternion>(1, Allocator.Persistent);
    }

    private void OnDisable()
    {
        leftHandPosition.Dispose();
        rightHandPosition.Dispose();
        leftHandRotation.Dispose();
        rightHandRotation.Dispose();
    }

    private void FixedUpdate()
    {
        if (!PlayPausePublisher.enable_out) return;

        ScheduleAndExecuteJobs();
        PublishResults();
    }

    private void ScheduleAndExecuteJobs()
    {
        float leftArmLength = math.distance(leftHandObject.position, Chest.position);
        float rightArmLength = math.distance(rightHandObject.position, Chest.position);

        var leftHandJob = new HandPoseJob
        {
            handPosition = leftHandObject.position,
            upperArmPosition = leftArmUpperObject.position,
            handRotation = leftHandObject.rotation,
            chestMatrix = Chest.worldToLocalMatrix,
            hipsRotation = Hips.rotation,
            armLength = leftArmLength,
            isLeft = true,
            outputPosition = leftHandPosition,
            outputRotation = leftHandRotation
        };

        var rightHandJob = new HandPoseJob
        {
            handPosition = rightHandObject.position,
            upperArmPosition = rightArmUpperObject.position,
            handRotation = rightHandObject.rotation,
            chestMatrix = Chest.worldToLocalMatrix,
            hipsRotation = Hips.rotation,
            armLength = rightArmLength,
            isLeft = false,
            outputPosition = rightHandPosition,
            outputRotation = rightHandRotation
        };

        var leftHandle = leftHandJob.Schedule();
        var rightHandle = rightHandJob.Schedule();

        JobHandle.CombineDependencies(leftHandle, rightHandle).Complete();
    }

    private void PublishResults()
    {
        PublishHandPose(leftWristTopic, leftHandPosition[0], leftHandRotation[0]);
        PublishHandPose(rightWristTopic, rightHandPosition[0], rightHandRotation[0]);
    }

    private void PublishHandPose(string topic, float3 position, quaternion rotation)
    {
        var poseMsg = new PoseMsg
        {
            position = new PointMsg
            {
                x = position.x,
                y = position.y,
                z = position.z
            },
            orientation = new QuaternionMsg
            {
                x = rotation.value.x,
                y = rotation.value.y,
                z = rotation.value.z,
                w = rotation.value.w
            }
        };
        ros.Publish(topic, poseMsg);
    }
}