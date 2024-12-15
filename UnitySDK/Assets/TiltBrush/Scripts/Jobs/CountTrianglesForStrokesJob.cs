using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;


// for explanation, see comments in Stroke2Mesh() method in EditorUtils.cs
public struct CountTrianglesForStrokesJob : IJobParallelFor
{
    [ReadOnly]
    public NativeArray<int> triangles;

    [ReadOnly]
    public NativeArray<float> strokeIDs;

    public NativeArray<int> triangleCounts; // Output: The count of triangles for each stroke

    [ReadOnly]
    public NativeArray<Vector3> uv3;

    // index is the strokeID index
    public void Execute(int index)
    {

        float targetStrokeID = strokeIDs[index];
        for (int i = 0; i < triangles.Length; i += 3)
        {
            if (AreFloatsEqual(targetStrokeID, uv3[triangles[i]].x))
            {
                triangleCounts[index] += 3;
            }
        }

    }

    // todo: put this in a common place
    private static bool AreFloatsEqual(float a, float b, float epsilon = 0.00001f)
    {
        return Mathf.Abs(a - b) < epsilon;
    }
}