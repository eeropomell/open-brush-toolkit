using System.Collections.Generic;
using System.Linq;
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

                NativeArray<Vector3> NativeUV2 = new NativeArray<Vector3>(uv2.ToArray(), Allocator.TempJob);
                NativeArray<float> NativeAllTimestamps = new NativeArray<float>(uv2.Count, Allocator.TempJob);

                var strokeJob = new AddStrokeIdsJob()
                {
                    uv2 = NativeUV2,
                    allTimestamps = NativeAllTimestamps
                };

                JobHandle jobHandle = strokeJob.Schedule(uv2.Count, 64);

                jobHandle.Complete();

                HashSet<float> NativeTimestamps = new HashSet<float>(NativeAllTimestamps.ToArray());

                NativeAllTimestamps.Dispose();

                var nativeTimestampsArray = NativeTimestamps.ToArray();

                NativeArray<float> NativeStrokeIDs = new NativeArray<float>(nativeTimestampsArray, Allocator.TempJob);
                NativeArray<Vector2> strokeData = new NativeArray<Vector2>(NativeTimestamps.Count, Allocator.TempJob);

                var StrokeJob3 = new CalculateVertexCountForEachStrokeID()
                {
                    uv2= NativeUV2,
                    strokeIDs = NativeStrokeIDs,
                    strokeData = strokeData
                }.Schedule(NativeTimestamps.Count, 1);

                StrokeJob3.Complete();
                NativeStrokeIDs.Dispose();

                NativeArray<Vector3> NativeUV3 = new NativeArray<Vector3>(uv2.Count, Allocator.TempJob);

                var StrokeJob2 = new AddStrokeIdsJob2()
                {
                    strokeData = strokeData,
                    uv3 = NativeUV3,
                    uv2 = NativeUV2
                };

                JobHandle jobHandle2 = StrokeJob2.Schedule(uv2.Count, 64);

                jobHandle2.Complete();

                mesh.SetUVs(3, NativeUV3.ToArray());
                mf.mesh = mesh;

                NativeUV3.Dispose();
                strokeData.Dispose();
                NativeUV2.Dispose();

            }
    }
}






