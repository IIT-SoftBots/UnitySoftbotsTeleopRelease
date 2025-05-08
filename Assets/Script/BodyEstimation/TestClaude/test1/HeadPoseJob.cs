using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;


/// <summary>
/// Job that calculates the relative head pose in relation to the chest.
/// Implements IJob for Unity's job system and uses Burst compilation for performance.
/// </summary>
[BurstCompile]
public struct HeadPoseJob : IJob
{
    // Matrix to convert from world space to local chest space
    public float4x4 chestWorldToLocal;

    // Rotation quaternions for head and chest
    public quaternion headRotation;
    public quaternion chestRotation;

    // Output array to store the calculated rotation
    public NativeArray<quaternion> outputRotation;

    /// <summary>
    /// Converts a quaternion to Euler angles in degrees
    /// </summary>
    /// <param name="q">Input quaternion to convert</param>
    /// <returns>Float3 containing Euler angles in degrees (x=roll, y=pitch, z=yaw)</returns>
    private float3 ToEulerAngles(quaternion q)
    {
        float3 angles;

        // Calculate roll (x-axis rotation)
        // Uses the formula: roll = atan2(2(qw*qx + qy*qz), 1 - 2(qx^2 + qy^2))
        float sinr_cosp = 2 * (q.value.w * q.value.x + q.value.y * q.value.z);
        float cosr_cosp = 1 - 2 * (q.value.x * q.value.x + q.value.y * q.value.y);
        angles.x = math.atan2(sinr_cosp, cosr_cosp);

        // Calculate pitch (y-axis rotation)
        // Uses the formula: pitch = asin(2(qw*qy - qz*qx))
        // Handles edge cases when close to poles (pitch = ±90°)
        float sinp = 2 * (q.value.w * q.value.y - q.value.z * q.value.x);
        if (math.abs(sinp) >= 1)
            angles.y = math.PI / 2 * math.sign(sinp);
        else
            angles.y = math.asin(sinp);

        // Calculate yaw (z-axis rotation)
        // Uses the formula: yaw = atan2(2(qw*qz + qx*qy), 1 - 2(qy^2 + qz^2))
        float siny_cosp = 2 * (q.value.w * q.value.z + q.value.x * q.value.y);
        float cosy_cosp = 1 - 2 * (q.value.y * q.value.y + q.value.z * q.value.z);
        angles.z = math.atan2(siny_cosp, cosy_cosp);

        // Convert from radians to degrees
        return angles * (180f / math.PI);
    }

    /// <summary>
    /// Executes the job to calculate the final head rotation relative to the chest
    /// </summary>
    public void Execute()
    {
        // Calculate relative rotation between head and chest
        quaternion relativeRot = math.mul(math.inverse(chestRotation), headRotation);

        // Convert to Euler angles for easier manipulation
        float3 eulerAngles = ToEulerAngles(relativeRot);

        // Remap the angles to correct coordinate system:
        // - X becomes Y (pitch)
        // - Y becomes inverted Z (yaw)
        // - Z becomes X (roll)
        float3 correctedEuler = new float3(
            eulerAngles.y,    // Pitch
            360 - eulerAngles.z, // Inverted yaw
            eulerAngles.x     // Roll
        );

        // Convert back to quaternion and store the result
        outputRotation[0] = quaternion.EulerXYZ(math.radians(correctedEuler));
    }
}