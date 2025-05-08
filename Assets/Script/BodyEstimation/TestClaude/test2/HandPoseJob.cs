
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct HandPoseJob : IJob
{
    public float3 handPosition;
    public float3 upperArmPosition;
    public quaternion handRotation;
    public float4x4 chestMatrix;
    public quaternion hipsRotation;
    public float armLength;
    public bool isLeft;

    public NativeArray<float3> outputPosition;
    public NativeArray<quaternion> outputRotation;

    public void Execute()
    {
        // Calculate relative position
        float3 wristPos = math.mul(chestMatrix, new float4(handPosition, 1f)).xyz;
        float3 upperArmPos = math.mul(chestMatrix, new float4(upperArmPosition, 1f)).xyz;
        float3 relativePos = (wristPos - upperArmPos) / armLength;

        // Convert to ROS coordinate system
        outputPosition[0] = new float3(
            relativePos.y,
            relativePos.z,
            -relativePos.x
        );

        // Calculate rotation
        quaternion wristRot = math.mul(math.inverse(hipsRotation), handRotation);
        float3 euler = QuaternionToEulerAngles(wristRot);

        if (isLeft)
        {
            euler.x = (-euler.x + 360) % 360;
            euler.y = (-euler.y + 360) % 360;
        }
        else
        {
            euler.x = (-euler.x + 360) % 360;
            euler.z = (-euler.z + 360) % 360;
        }

        outputRotation[0] = quaternion.Euler(math.radians(euler));
    }

    private float3 QuaternionToEulerAngles(quaternion q)
    {
        float3 angles;
        float sqw = q.value.w * q.value.w;
        float sqx = q.value.x * q.value.x;
        float sqy = q.value.y * q.value.y;
        float sqz = q.value.z * q.value.z;

        angles.x = math.degrees(math.atan2(2f * (q.value.x * q.value.w + q.value.y * q.value.z), 1f - 2f * (sqx + sqy)));
        angles.y = math.degrees(math.asin(2f * (q.value.w * q.value.y - q.value.z * q.value.x)));
        angles.z = math.degrees(math.atan2(2f * (q.value.z * q.value.w + q.value.x * q.value.y), 1f - 2f * (sqy + sqz)));

        return angles;
    }
}