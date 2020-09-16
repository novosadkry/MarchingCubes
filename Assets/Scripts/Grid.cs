using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    MeshCollider meshCollider;

    GridCell cell = new GridCell();

    public List<float> values;

    // Start is called before the first frame update
    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();

        for (int i = 0; i < values.Count; i++)
            cell.Values[i] = values[i];

        Mesh mesh = cell.ConstructMesh(0.5f);
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            for (int i = 0; i < values.Count; i++)
                cell.Values[i] = values[i];

            Mesh mesh = cell.ConstructMesh(0.5f);
            meshFilter.mesh = mesh;
            meshCollider.sharedMesh = mesh;
        }
    }
}
