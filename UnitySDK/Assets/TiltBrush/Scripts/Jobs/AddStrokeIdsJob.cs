using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Burst;
using Unity.Mathematics;



[BurstCompile]
public struct AddStrokeIdsJob : IJobParallelFor
{

    [ReadOnly] public NativeArray<Vector3> uv2;

    public NativeArray<float> allTimestamps;

    public void Execute(int index)
    {
        allTimestamps[index] = uv2[index].x;
    }
}


// we need to calculate the amount of vertices that each strokeID has
// we then store that in uv3.z
[BurstCompile]
public struct CalculateVertexCountForEachStrokeID : IJobParallelFor
{

    // this is where we read the timestamp (strokeID) data for each vertex
    [ReadOnly] public NativeArray<Vector3> uv2;

    [ReadOnly] public NativeArray<float> strokeIDs;

    // Output: Vector2 of (strokeID,vertexCount)
    public NativeArray<Vector2> strokeData;

    public void Execute(int index)
    {
        float strokeID = strokeIDs[index];

        float2 v = strokeData[index];
        v.x = strokeID;

        int vertexCount = 0;
        for (int i = 0; i < uv2.Length; i++)
        {
            if (AreFloatsEqual(uv2[i].x, strokeID))
            {
                vertexCount++;
            }
        }

        v.y = vertexCount;

        strokeData[index] = v;
    }

    private static bool AreFloatsEqual(float a, float b, float epsilon = 0.00001f)
    {
        return math.abs(a - b) < epsilon;
    }
}

// this job will take the timestamps set and uv2, and create uv3
[BurstCompile]
public struct AddStrokeIdsJob2 : IJobParallelFor
{

    [ReadOnly] public NativeArray<Vector3> uv2;

    [ReadOnly] public NativeArray<Vector2> strokeData;

    public NativeArray<Vector3> uv3;

    // we execute for each vertex
    public void Execute(int index)
    {
        float3 v = uv2[index];
        v.z = 0;
        if (index <= strokeData.Length - 1)
        {
            v.y = strokeData[index].x;
            v.z = strokeData[index].y;
        }
        else
        {
            v.y = 0;
        }

        uv3[index] = v;
    }


}








