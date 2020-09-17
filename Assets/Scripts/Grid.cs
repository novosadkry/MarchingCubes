using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Grid : MonoBehaviour
{
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;

    [SerializeField]
    private Vector3 gridPosition;
    public Vector3 GridPosition 
    { 
        get => gridPosition; 
        set => gridPosition = value; 
    }

    [SerializeField]
    private float gridScale;
    public float GridScale
    {
        get => gridScale;
        set => gridScale = value;
    }

    [SerializeField]
    private int cellCount;
    public int CellCount
    {
        get => cellCount;
        set => cellCount = value;
    }

    [Range(0.0f, 1.0f)]
    public float surfaceLevel;

    [Header("Debug")]
    public bool showCubes;

    private GridCell[,,] cells;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();

        GenerateGridValues();

        Mesh mesh = ConstructMesh(surfaceLevel);
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            GenerateGridValues();

            Mesh mesh = ConstructMesh(surfaceLevel);
            meshFilter.mesh = mesh;
            meshCollider.sharedMesh = mesh;
        }

        if (showCubes)
        {
            for (int x = 0; x < cells.GetUpperBound(0) + 1; x++)
            {
                for (int y = 0; y < cells.GetUpperBound(1) + 1; y++)
                {
                    for (int z = 0; z < cells.GetUpperBound(2) + 1; z++)
                    {
                        var cell = cells[x, y, z];

                        for (int i = 0; i < 12; i++)
                        {
                            var edge = GridCell.edges[i];

                            Debug.DrawLine(edge.A * cell.Scale + cell.ScaledPosition + GridPosition, edge.B * cell.Scale + cell.ScaledPosition + GridPosition);
                        }
                    }
                }
            }
        }
    }

    public void GenerateGridValues()
    {
        FastNoise noiseGen = new FastNoise(257746);

        cells = new GridCell[CellCount, CellCount, CellCount];

        for (int x = 0; x < cells.GetUpperBound(0) + 1; x++)
        {
            for (int y = 0; y < cells.GetUpperBound(1) + 1; y++)
            {
                for (int z = 0; z < cells.GetUpperBound(2) + 1; z++)
                {
                    GridCell cell = new GridCell()
                    {
                        Scale = GridScale / CellCount,
                        Position = new Vector3(x, y, z)
                    };

                    for (int i = 0; i < 8; i++)
                    {
                        Vector3 valuePos = GridPosition + cell.GetValuePos(i);

                        float noise = noiseGen.GetPerlin(valuePos.x, valuePos.y, valuePos.z);
                        noise = (noise + 1) / 2;
                        
                        cell.Values[i] = noise;
                    }

                    cells[x, y, z] = cell;
                }
            }
        }
    }

    public Mesh ConstructMesh(float surfaceLevel)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        int vertexCount = 0;

        for (int x = 0; x < cells.GetUpperBound(0) + 1; x++)
        {
            for (int y = 0; y < cells.GetUpperBound(1) + 1; y++)
            {
                for (int z = 0; z < cells.GetUpperBound(2) + 1; z++)
                {
                    GridCell cell = cells[x, y, z];

                    int cubeIndex = cell.GetCubeIndex(surfaceLevel);
                    int[] tri = Table.triangulation[cubeIndex];

                    foreach (int edgeIndex in tri)
                    {
                        if (edgeIndex == -1)
                            continue;

                        GridCell.Edge edge = GridCell.edges[edgeIndex];
                        vertices.Add((edge.Midpoint * cell.Scale) + cell.ScaledPosition);
                        triangles.Add(vertexCount++);
                    }

                    foreach (int edgeIndex in tri.Reverse())
                    {
                        if (edgeIndex == -1)
                            continue;

                        GridCell.Edge edge = GridCell.edges[edgeIndex];
                        vertices.Add((edge.Midpoint * cell.Scale) + cell.ScaledPosition);
                        triangles.Add(vertexCount++);
                    }
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        return mesh;
    }
}
