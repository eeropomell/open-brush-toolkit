using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SketchPlayback : MonoBehaviour
{

    public MeshFilter currentStroke;

    public List<MeshFilter> allStrokes;


    public bool playAnimation = true;

    private bool strokesAreReady = false;

    // this is the current clipEnd of the stroke we're animating
    // clipEnd is reset to 0 everytime a stroke has been fully revealed
    private float clipEnd = 0;
    public int currentStrokeIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        allStrokes = new List<MeshFilter>();
        List<MeshFilter> allMeshFilters = GetComponentsInChildren<MeshFilter>().ToList();
        foreach (MeshFilter mf in allMeshFilters)
        {
            MeshRenderer meshRenderer = mf.GetComponent<MeshRenderer>();

            Material mat_ = meshRenderer.sharedMaterial;

            meshRenderer.material = new Material(mat_);

            // this will hide the material at the beginning
            meshRenderer.material.SetFloat("_ClipEnd",.1f);
            meshRenderer.material.EnableKeyword("SHADER_SCRIPTING_ON");

            allStrokes.Add(mf);

        }

        strokesAreReady = true;
        currentStrokeIndex = 0;
    }

    private MeshFilter GetStrokeByVertex(MeshFilter[] allStrokes_, int vertex)
    {
        int currentVertexBase = 0;
        MeshFilter mf = new MeshFilter();
        for (int i = 0; i < allStrokes_.Length; i++)
        {
            mf = allStrokes_[i];
            Mesh mesh = mf.sharedMesh;
            if (currentVertexBase > vertex && vertex < currentVertexBase + mesh.vertexCount)
            {
                return mf;
            }
        }

        return null;
    }

    // how many vertices to reveal per frame
    public float vertexRevealSpeed = 1f;

    // Update is called once per frame
    void Update()
    {
        if (!strokesAreReady)
        {
            return;
        }
        if (currentStrokeIndex < allStrokes.Count && playAnimation)
        {
            currentStroke = allStrokes[currentStrokeIndex];


            MeshRenderer meshRenderer = currentStroke.GetComponent<MeshRenderer>();
            Material mat = meshRenderer.material;
            Mesh mesh = currentStroke.mesh;

            clipEnd += vertexRevealSpeed;

            mat.SetFloat("_ClipEnd",clipEnd);

            // we need to switch strokes when all vertices are revealed in this one
            if (clipEnd >= mesh.vertexCount)
            {
                currentStrokeIndex++;
                clipEnd = 0;
            }
        }
    }


}
