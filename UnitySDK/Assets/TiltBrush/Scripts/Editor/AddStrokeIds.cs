using System.Collections.Generic;
using Unity.Jobs;
using Unity.Collections;
using UnityEditor;
using UnityEngine;

// Writes a new Vector3 to UV3/4 which contains:
// x: A unique ID for the stroke (or "island") that this vertex belongs to

public class AddStrokeIds
{
    [MenuItem("Tools/AddStrokeIds")]
    public static void DoAddStrokeIds()
    {
        var selected = Selection.transforms;
        AddStrokeIds_(selected);
    }

    public static bool AreFloatsEqual(float a, float b, float epsilon = 0.00001f)
    {
        return Mathf.Abs(a - b) < epsilon;
    }


    private static void AddStrokeIds_(Transform[] selected)
    {
        int currentStrokeIndex = 0;

        foreach (var tr in selected)
        {
            // get the mesh of the selection
            var mf = tr.GetComponent<MeshFilter>();
            var mesh = mf.sharedMesh;
            var uv2 = new List<Vector3>();

            mesh.GetUVs(2, uv2);

            float prevVertexTimestamp = -1;

            // The following lists have one entry per stroke
            var timestamps = new List<float>(); // Timestamps

            // iterate all vertices
            for (int vertIndex = 0; vertIndex < uv2.Count; vertIndex++)
            {

                float currentVertTimestamp = uv2[vertIndex].x;

                if (vertIndex == 0) // First vertex therefore first stroke
                {
                    timestamps.Add(currentVertTimestamp);
                }

                if (!AreFloatsEqual(currentVertTimestamp,prevVertexTimestamp))
                {
                    // New Stroke
                    if (vertIndex > 0) // Skip the first vertex
                    {
                        // Add new timestamp
                        timestamps.Add(currentVertTimestamp);
                        currentStrokeIndex++;
                    }
                }

                prevVertexTimestamp = currentVertTimestamp;
            }

            var dataUVs = new List<Vector3>();

            for (int i = 0; i < uv2.Count; i++)
            {
                float strokeIndex = uv2[i].x;
                if (timestamps.Count > i)
                {
                    dataUVs.Add(new Vector3(strokeIndex, timestamps[i], 0));
                }
                else
                {
                    dataUVs.Add(new Vector3(strokeIndex, 0, 0));
                }

            }



            mesh.SetUVs(3, dataUVs);
            mf.mesh = mesh;
        }
    }
}