using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Burst;

// for explanation, see comments in Stroke2Mesh() method in EditorUtils.cs
[BurstCompile]
public struct CollectTrianglesForStrokesJob : IJobParallelFor
{
    [ReadOnly]
    public NativeArray<Vector3> uv3;

    [ReadOnly] public NativeArray<float> strokeIDs;

    [ReadOnly] public NativeArray<int> originalTriangles;

    [ReadOnly] public NativeArray<int> triangleCounts;

    // map a vertex index in mesh.vertices to the vertex index in newMesh.vertices array
    [ReadOnly] public NativeArray<int> vertexMap;

    // output: this contains all the triangles, in contiguous memory
    [NativeDisableParallelForRestriction]
    public NativeArray<int> triangles;

    // index is the nth stroke
    public void Execute(int index)
    {
        float targetStrokeID = strokeIDs[index];

        int startingIndex = 0;
        for (int i = 0; i < index; i++)
        {
            startingIndex += (triangleCounts[i]);
        }

        int triangleIndex = 0;
        // we iterate the vertices
        for (int i = 0; i < originalTriangles.Length; i += 3)
        {
            if (AreFloatsEqual(targetStrokeID, uv3[originalTriangles[i]].x))
            {
                triangles[startingIndex + triangleIndex] = vertexMap[originalTriangles[i]];
                triangles[startingIndex + triangleIndex + 1] = vertexMap[originalTriangles[i+1]];
                triangles[startingIndex + triangleIndex + 2] = vertexMap[originalTriangles[i+2]];
                triangleIndex += 3;
            }
        }

    }

    // todo: put this in a common place
    private static bool AreFloatsEqual(float a, float b, float epsilon = 0.00001f)
    {
        return Mathf.Abs(a - b) < epsilon;
    }


}
