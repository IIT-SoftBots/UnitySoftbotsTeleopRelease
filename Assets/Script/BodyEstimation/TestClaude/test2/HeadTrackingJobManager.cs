using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using RosMessageTypes.Geometry;
using Unity.Robotics.ROSTCPConnector;

public class HeadTrackingJobManager : MonoBehaviour
{
    [Header("Transform References")]
    public Transform Chest;
    public Transform Head;

    [Header("Publishing Configuration")]

    private ROSConnection ros;
    private string headPoseTopic;

    // Native container for job system
    private NativeArray<quaternion> headOutputRotation;

    private void OnEnable()
    {
        InitializeROS();
        InitializeNativeArrays();
    }

    private void InitializeROS()
    {
        string robotNamespace = SwitchRobotManager.robot_name;
        headPoseTopic = $"/{robotNamespace}/head_pose";

        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<PoseMsg>(headPoseTopic);
    }

    private void InitializeNativeArrays()
    {
        headOutputRotation = new NativeArray<quaternion>(1, Allocator.Persistent);
    }

    private void OnDisable()
    {
        headOutputRotation.Dispose();
    }

    private void FixedUpdate()
    {
        if (!PlayPausePublisher.enable_out) return;


        ScheduleAndExecuteJob();
        PublishResults();

    }

    private void ScheduleAndExecuteJob()
    {
        var headJob = new HeadPoseJobOnly
        {
            headRotation = Head.rotation,
            chestRotation = Chest.rotation,
            outputRotation = headOutputRotation
        };

        headJob.Schedule().Complete();
    }

    private void PublishResults()
    {
        var headPose = new PoseMsg
        {
            position = new PointMsg { x = 0, y = 0, z = 0 },
            orientation = ToQuaternionMsg(headOutputRotation[0])
        };
        ros.Publish(headPoseTopic, headPose);
    }

    private QuaternionMsg ToQuaternionMsg(quaternion q)
    {
        return new QuaternionMsg
        {
            x = q.value.x,
            y = q.value.y,
            z = q.value.z,
            w = q.value.w
        };
    }

}