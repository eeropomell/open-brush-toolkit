using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Burst;
using Unity.Mathematics;


[BurstCompile]
public struct CreateVertexArraysJob : IJobParallelFor
{

    [ReadOnly] public NativeArray<float> strokeIDs;

    [ReadOnly] public NativeArray<Vector3> uv3;

    [NativeDisableParallelForRestriction]
    public NativeArray<int> vertexMap;

    public void Execute(int index)
    {
        float strokeId = strokeIDs[index];

        int startingVertexIndex = 0;

        bool lookingForStartingIndex = true;


        for (int i = 0; i < uv3.Length; i++)
        {
            if (lookingForStartingIndex)
            {
                if (AreFloatsEqual(uv3[i].x, strokeId))
                {
                    startingVertexIndex = i;
                    lookingForStartingIndex = false;
                }
            }
            else
            {
                // we're done
                if (!AreFloatsEqual(uv3[i].x, strokeId))
                {
                    break;
                }

                vertexMap[i] = i - startingVertexIndex;
            }
        }


    }

    private static bool AreFloatsEqual(float a, float b, float epsilon = 0.00001f)
    {
        return math.abs(a - b) < epsilon;
    }
}