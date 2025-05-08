using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using UnityEngine;

[BurstCompile]
public struct HeadPoseJobOnly : IJob
{
    public quaternion headRotation;
    public quaternion chestRotation;
    public NativeArray<quaternion> outputRotation;

    public void Execute()
    {
        // Calcolo rotazione relativa
        quaternion relativeRotation = math.mul(math.inverse(chestRotation), headRotation);

        // Conversione in Euler angles
        float3 eulerAngles = QuaternionToEulerAngles(relativeRotation);

        // Modifica degli angoli come nell'originale
        eulerAngles = new float3(
            eulerAngles.y,
            360 - eulerAngles.z,
            eulerAngles.x
        );

        // Ricostruzione del quaternione con gli angoli modificati
        outputRotation[0] = quaternion.Euler(math.radians(eulerAngles));
    }

    // Metodo personalizzato per convertire quaternione in angoli Euler
    private float3 QuaternionToEulerAngles(quaternion q)
    {
        float3 angles;

        // Conversione in gradi
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