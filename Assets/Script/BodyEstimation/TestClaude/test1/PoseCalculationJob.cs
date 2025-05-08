// PoseCalculationJob.cs
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using UnityEngine;

[BurstCompile]
public struct PoseCalculationJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<float4x4> worldToLocalMatrices;
    [ReadOnly] public NativeArray<float3> handPositions;
    [ReadOnly] public NativeArray<float3> upperArmPositions;
    [ReadOnly] public NativeArray<quaternion> handRotations;
    [ReadOnly] public NativeArray<quaternion> hipsRotation;
    [ReadOnly] public NativeArray<float> armLengths;
    [ReadOnly] public NativeArray<bool> isLeftSide;

    public NativeArray<float3> outputPositions;
    public NativeArray<quaternion> outputRotations;

    private float3 ToEulerAngles(quaternion q)
    {
        float3 angles;

        // roll (x-axis rotation)
        float sinr_cosp = 2 * (q.value.w * q.value.x + q.value.y * q.value.z);
        float cosr_cosp = 1 - 2 * (q.value.x * q.value.x + q.value.y * q.value.y);
        angles.x = math.atan2(sinr_cosp, cosr_cosp);

        // pitch (y-axis rotation)
        float sinp = 2 * (q.value.w * q.value.y - q.value.z * q.value.x);
        if (math.abs(sinp) >= 1)
            angles.y = math.PI / 2 * math.sign(sinp);
        else
            angles.y = math.asin(sinp);

        // yaw (z-axis rotation)
        float siny_cosp = 2 * (q.value.w * q.value.z + q.value.x * q.value.y);
        float cosy_cosp = 1 - 2 * (q.value.y * q.value.y + q.value.z * q.value.z);
        angles.z = math.atan2(siny_cosp, cosy_cosp);

        return angles * (180f / math.PI);
    }

    public void Execute(int index)
    {
        float4x4 worldToLocal = worldToLocalMatrices[index];

        // Transform positions to local space
        float3 wristPos = math.mul(worldToLocal, new float4(handPositions[index], 1f)).xyz;
        float3 upperArmPos = math.mul(worldToLocal, new float4(upperArmPositions[index], 1f)).xyz;

        // Calculate relative position
        float3 relativePos = (wristPos - upperArmPos) / armLengths[index];

        // Convert to ROS coordinates
        float3 rosCoords = new float3(
            relativePos.y,
            relativePos.z,
            -relativePos.x
        );

        // Calculate rotation
        quaternion wristRot = math.mul(math.inverse(hipsRotation[0]), handRotations[index]);
        float3 eulerAngles = ToEulerAngles(wristRot);

        // Apply side-specific rotation corrections
        if (isLeftSide[index])
        {
            eulerAngles.x = math.fmod(-eulerAngles.x + 360, 360);
            eulerAngles.y = math.fmod(-eulerAngles.y + 360, 360);
        }
        else
        {
            eulerAngles.x = math.fmod(-eulerAngles.x + 360, 360);
            eulerAngles.z = math.fmod(-eulerAngles.z + 360, 360);
        }

        outputPositions[index] = rosCoords;
        outputRotations[index] = quaternion.EulerXYZ(math.radians(eulerAngles));
    }
}