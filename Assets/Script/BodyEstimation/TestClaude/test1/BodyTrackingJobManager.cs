using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using RosMessageTypes.Geometry;
using Unity.Robotics.ROSTCPConnector;

public class BodyTrackingJobManager : MonoBehaviour
{
    [Header("Transform References")]
    public Transform leftHandObject;
    public Transform rightHandObject;
    public Transform leftArmUpperObject;
    public Transform rightArmUpperObject;
    public Transform Chest;
    public Transform Head;
    public Transform Hips;

    [Header("Publishing Configuration")]
    [SerializeField] private float publishRate = 60f;
    private float publishInterval;
    private float lastPublishTime;

    private ROSConnection ros;
    private string headPoseTopic;
    private string leftWristTopic;
    private string rightWristTopic;

    // Native containers for job system
    private NativeArray<float4x4> worldToLocalMatrices;
    private NativeArray<float3> handPositions;
    private NativeArray<float3> upperArmPositions;
    private NativeArray<quaternion> handRotations;
    private NativeArray<quaternion> hipsRotation;
    private NativeArray<float> armLengths;
    private NativeArray<bool> isLeftSide;
    private NativeArray<float3> outputPositions;
    private NativeArray<quaternion> outputRotations;
    private NativeArray<quaternion> headOutputRotation;

    private void OnEnable()
    {
        InitializeROS();
        InitializeNativeArrays();
        publishInterval = 1f / publishRate;
    }

    private void InitializeROS()
    {
        string robotNamespace = SwitchRobotManager.robot_name;
        headPoseTopic = $"/{robotNamespace}/head_pose";
        leftWristTopic = $"/{robotNamespace}/hand_L_ref";
        rightWristTopic = $"/{robotNamespace}/hand_R_ref";

        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<PoseMsg>(headPoseTopic);
        ros.RegisterPublisher<PoseMsg>(leftWristTopic);
        ros.RegisterPublisher<PoseMsg>(rightWristTopic);
    }

    private void InitializeNativeArrays()
    {
        // Initialize arrays with length 2 for left and right sides
        worldToLocalMatrices = new NativeArray<float4x4>(2, Allocator.Persistent);
        handPositions = new NativeArray<float3>(2, Allocator.Persistent);
        upperArmPositions = new NativeArray<float3>(2, Allocator.Persistent);
        handRotations = new NativeArray<quaternion>(2, Allocator.Persistent);
        hipsRotation = new NativeArray<quaternion>(1, Allocator.Persistent);
        armLengths = new NativeArray<float>(2, Allocator.Persistent);
        isLeftSide = new NativeArray<bool>(2, Allocator.Persistent);
        outputPositions = new NativeArray<float3>(2, Allocator.Persistent);
        outputRotations = new NativeArray<quaternion>(2, Allocator.Persistent);
        headOutputRotation = new NativeArray<quaternion>(1, Allocator.Persistent);

        // Set constant values
        isLeftSide[0] = true;
        isLeftSide[1] = false;

        // Calculate initial arm lengths
        armLengths[0] = Vector3.Distance(leftHandObject.position, Chest.position);
        armLengths[1] = Vector3.Distance(rightHandObject.position, Chest.position);
    }

    private void OnDisable()
    {
        // Dispose all native arrays to prevent memory leaks
        worldToLocalMatrices.Dispose();
        handPositions.Dispose();
        upperArmPositions.Dispose();
        handRotations.Dispose();
        hipsRotation.Dispose();
        armLengths.Dispose();
        isLeftSide.Dispose();
        outputPositions.Dispose();
        outputRotations.Dispose();
        headOutputRotation.Dispose();
    }

    private void FixedUpdate()
    {
        if (!PlayPausePublisher.enable_out) return;

        float currentTime = Time.time;
        if (currentTime - lastPublishTime < publishInterval) return;

        UpdateJobData();
        ScheduleAndExecuteJobs();
        PublishResults();
        
        lastPublishTime = currentTime;
    }

    private void UpdateJobData()
    {
        // Update matrices and transforms
        float4x4 chestWorldToLocal = Chest.worldToLocalMatrix;
        worldToLocalMatrices[0] = chestWorldToLocal;
        worldToLocalMatrices[1] = chestWorldToLocal;

        // Update positions
        handPositions[0] = leftHandObject.position;
        handPositions[1] = rightHandObject.position;
        upperArmPositions[0] = leftArmUpperObject.position;
        upperArmPositions[1] = rightArmUpperObject.position;

        // Update rotations
        handRotations[0] = leftHandObject.rotation;
        handRotations[1] = rightHandObject.rotation;
        hipsRotation[0] = Hips.rotation;
    }

    private void ScheduleAndExecuteJobs()
    {
        // Create and schedule pose calculation job
        var poseJob = new PoseCalculationJob
        {
            worldToLocalMatrices = worldToLocalMatrices,
            handPositions = handPositions,
            upperArmPositions = upperArmPositions,
            handRotations = handRotations,
            hipsRotation = hipsRotation,
            armLengths = armLengths,
            isLeftSide = isLeftSide,
            outputPositions = outputPositions,
            outputRotations = outputRotations
        };

        // Create and schedule head pose job
        var headJob = new HeadPoseJob
        {
            chestWorldToLocal = Chest.worldToLocalMatrix,
            headRotation = Head.rotation,
            chestRotation = Chest.rotation,
            outputRotation = headOutputRotation
        };

        // Schedule jobs
        JobHandle poseHandle = poseJob.Schedule(2, 1);
        JobHandle headHandle = headJob.Schedule();

        // Wait for all jobs to complete
        JobHandle.CombineDependencies(poseHandle, headHandle).Complete();
    }

    private void PublishResults()
    {
        // Publish head pose
        var headPose = new PoseMsg
        {
            position = new PointMsg { x = 0, y = 0, z = 0 },
            orientation = ToQuaternionMsg(headOutputRotation[0])
        };
        ros.Publish(headPoseTopic, headPose);

        // Publish left and right poses
        PublishSidePose(0, leftWristTopic);
        PublishSidePose(1, rightWristTopic);
    }

    private void PublishSidePose(int index, string topic)
    {
        var pose = new PoseMsg
        {
            position = new PointMsg
            {
                x = outputPositions[index].x,
                y = outputPositions[index].y,
                z = outputPositions[index].z
            },
            orientation = ToQuaternionMsg(outputRotations[index])
        };
        ros.Publish(topic, pose);
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

    // Optional: visualize FPS
    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 200, 20), $"FPS: {1.0f / Time.deltaTime:F1}");
        GUI.Label(new Rect(10, 30, 200, 20), $"Publish Rate: {1.0f / publishInterval:F1} Hz");
    }
}