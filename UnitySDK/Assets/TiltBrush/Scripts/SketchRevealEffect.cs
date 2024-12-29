using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

[ExecuteAlways]
public class SketchRevealEffect : MonoBehaviour
{

    [SerializeField]
    private List<MeshFilter> allStrokes;


    public float totalVertexCount = 0;
    public float totalSketchTime = 0f;

    public float sketchStartTime = 0f;
    public float sketchEndTime = 0f;

    private float prevT;
    [Range(0,1)]
    public float t;


    // Start is called before the first frame update
    void Start()
    {
        allStrokes = new List<MeshFilter>();
        List<MeshFilter> allMeshFilters = GetComponentsInChildren<MeshFilter>().ToList();
        totalVertexCount = 0;
        totalSketchTime = 0f;
        foreach (MeshFilter mf in allMeshFilters)
        {
            MeshRenderer meshRenderer = mf.GetComponent<MeshRenderer>();

            Material mat_ = meshRenderer.sharedMaterial;

            meshRenderer.material = new Material(mat_);

            // this will hide the material at the beginning
            meshRenderer.sharedMaterial.SetFloat("_ClipEnd",.1f);
            meshRenderer.sharedMaterial.EnableKeyword("SHADER_SCRIPTING_ON");

            totalVertexCount += mf.sharedMesh.vertexCount;

            allStrokes.Add(mf);
        }

        OrderStrokes();


        totalSketchTime = GetSketchLastTimestamp(allStrokes.ToArray()) - GetSketchFirstTimestamp(allStrokes.ToArray());
        sketchStartTime = GetSketchFirstTimestamp(allStrokes.ToArray());
        sketchEndTime = GetSketchLastTimestamp(allStrokes.ToArray());

        prevT = -1f;
    }

    public float CalculateTotalSketchTime()
    {
        return (GetSketchLastTimestamp(allStrokes.ToArray()) - GetSketchFirstTimestamp(allStrokes.ToArray())) / 1000;
    }

    private float GetSketchFirstTimestamp(MeshFilter[] allStrokes_)
    {
        Mesh mesh = allStrokes_[0].sharedMesh;
        return mesh.uv3[0].x;
    }

    private float GetSketchLastTimestamp(MeshFilter[] allStrokes_)
    {
        Mesh mesh = allStrokes_[allStrokes_.Length - 1].sharedMesh;
        return mesh.uv3[0].y;
    }

    private float GetStrokeID(MeshFilter stroke)
    {
        Mesh mesh = stroke.sharedMesh;
        if (mesh == null)
        {
            return -1;
        } else if (mesh.uv3[0].x == 0.0f)
        {
            Debug.LogError("uv3[0].x == 0 for: " + mesh.name);
            return -1;
        }
        Vector2[] uv2 = mesh.uv3;
        Assert.AreNotEqual(0.0,uv2[0].x);
        return uv2[0].x;
    }

    public void OrderStrokes()
    {

        allStrokes.Sort((x, y) =>
        {
            float xStrokeID = GetStrokeID(x);
            float yStrokeID = GetStrokeID(y);
            if (AreFloatsEqual(xStrokeID, yStrokeID))
            {
                return 0;
            }
            if (xStrokeID < yStrokeID)
            {
                return -1;
            } else if (xStrokeID > yStrokeID)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        });

    }

    private static bool AreFloatsEqual(float a, float b, float epsilon = 0.00001f)
    {
        return Mathf.Abs(a - b) < epsilon;
    }

    public void UpdateStrokesArray()
    {
        Start();
    }


    private void HideAllStrokes(MeshFilter[] allStrokes_)
    {
        MeshFilter mf = new MeshFilter();

        for (int i = 0; i < allStrokes_.Length; i++)
        {
            try
            {
                mf = allStrokes_[i];
                MeshRenderer meshRenderer = mf.GetComponent<MeshRenderer>();
                Material mat = meshRenderer.sharedMaterial;
                Mesh mesh = mf.sharedMesh;

                mat.SetFloat("_ClipEnd", .1f);
            }
            catch (MissingReferenceException exception)
            {
                ; // this exception is caused if the mesh is deleted
                // it's expected to happen and doesn't need to be logged
            }
            catch (Exception exception)
            {
                Debug.LogError($"{exception}");
            }
        }
    }

    private void ShowAllStrokes(MeshFilter[] allStrokes_)
    {
        MeshFilter mf = new MeshFilter();

        for (int i = 0; i < allStrokes_.Length; i++)
        {
            try
            {
                mf = allStrokes_[i];
                MeshRenderer meshRenderer = mf.GetComponent<MeshRenderer>();
                Material mat = meshRenderer.sharedMaterial;
                Mesh mesh = mf.sharedMesh;

                mat.SetFloat("_ClipEnd", 0);
            }
            catch (MissingReferenceException exception)
            {
                ; // this exception is caused if the mesh is deleted
                // it's expected to happen and doesn't need to be logged
            }
            catch (Exception exception)
            {
                Debug.LogError($"{exception}");
            }
        }
    }


    private void ShowVerticesUpTo(MeshFilter[] allStrokes_, int vertex)
    {
        int vertexBase = 0;
        MeshFilter mf = new MeshFilter();
        MeshRenderer meshRenderer = new MeshRenderer();
        Material mat;
        bool hide = false;
        // we iterate the strokes, until we reach 'vertex'
        // make each of the strokes on the journey visible
        for (int i = 0; i < allStrokes_.Length; i++)
        {
            try
            {
                mf = allStrokes_[i];
                if (hide)
                {
                    meshRenderer = mf.GetComponent<MeshRenderer>();
                    mat = meshRenderer.sharedMaterial;

                    // make it visible
                    mat.SetFloat("_ClipEnd", 0.1f);
                    continue;
                }

                if (mf.sharedMesh.vertexCount + vertexBase < vertex)
                {
                    vertexBase += mf.sharedMesh.vertexCount;

                    meshRenderer = mf.GetComponent<MeshRenderer>();
                    mat = meshRenderer.sharedMaterial;

                    // make it visible
                    mat.SetFloat("_ClipEnd", 0);

                    continue;
                }

                // do it once, and return

                // we've already shown 'vertexBase' amount vertices
                // for this stroke, we show the first 'vertex - vertexBase + 1' vertices
                int localVertexIndex = vertex - vertexBase + 1;

                meshRenderer = mf.GetComponent<MeshRenderer>();
                mat = meshRenderer.sharedMaterial;

                // make it visible
                mat.SetFloat("_ClipEnd", localVertexIndex);

                hide = true;
            }
            catch (MissingReferenceException exception)
            {
                ; // this exception is caused if the mesh is deleted
                // it's expected to happen and doesn't need to be logged
            }
            catch (Exception exception)
            {
                Debug.LogError($"{exception}");
            }
        }
    }


    // Update is called once per frame
    void Update()
    {
        // if we're in Edit mode, use t in [0,1] to drive the playback
        if (!Application.IsPlaying(gameObject))
        {
            if (AreFloatsEqual(t, prevT))
            {
                return;
            }
            prevT = t;
            // if t == 0, hide all
            // if t == 1, show all
            int vertex = (int)(t * totalVertexCount);

            if (vertex == 0)
            {
                HideAllStrokes(allStrokes.ToArray());
            } else if (t == 1.0)
            {
                ShowAllStrokes(allStrokes.ToArray());
            }
            else
            {
                ShowVerticesUpTo(allStrokes.ToArray(),vertex);
            }

        }



    }
}



