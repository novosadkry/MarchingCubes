using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Grid : MonoBehaviour
{
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;

    [SerializeField]
    private Vector3Int gridPosition;
    public Vector3Int GridPosition 
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

    [SerializeField]
    private int seed;
    public int Seed 
    { 
        get => seed; 
        set => seed = value; 
    }

    [SerializeField]
    private float maxHeight;
    public float MaxHeight
    {
        get => maxHeight;
        set => maxHeight = value;
    }

    [SerializeField]
    private float frequency;
    public float Frequency
    {
        get => frequency;
        set => frequency = value;
    }

    [Range(0.0f, 1.0f)]
    public float surfaceLevel;

    [Header("Debug")]
    public bool showCubes;

    private GridCell[,,] cells;

    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();
    }

    void Update()
    {
        if (showCubes)
        {
            ForeachCoordinate((pos) =>
            {
                var cell = GetCell(pos);

                for (int i = 0; i < 12; i++)
                {
                    var edge = GridCell.edges[i];

                    Debug.DrawLine(
                        edge.A * cell.Scale + (Vector3)cell.Position * cell.Scale + transform.position,
                        edge.B * cell.Scale + (Vector3)cell.Position * cell.Scale + transform.position
                    );
                }
            });
        }
    }

    public void GenerateGridValues()
    {
        FastNoise noiseGen = new FastNoise(Seed);

        cells = new GridCell[CellCount, CellCount, CellCount];

        ForeachCoordinate((pos) =>
        {
            GridCell cell = new GridCell()
            {
                Scale = GridScale / CellCount,
                Position = pos
            };

            for (int i = 0; i < 8; i++)
            {
                Vector3 valuePos = (Vector3)GridPosition * GridScale + cell.GetValuePos(i);
                
                float noise = noiseGen.GetPerlin(
                    valuePos.x / Frequency,
                    valuePos.z / Frequency
                );

                noise = (noise + 1) / 2;
                
                float height = noise * MaxHeight - valuePos.y;
                float value = 1 - height / MaxHeight;

                cell.Values[i] = value;
            }

            SetCell(pos, cell);
        });
    }

    public void ConstructMesh()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        int vertexCount = 0;

        ForeachCoordinate((pos) => 
        {
            GridCell cell = GetCell(pos);

            int cubeIndex = cell.GetCubeIndex(surfaceLevel);
            int[] tri = Table.triangulation[cubeIndex];

            foreach (int edgeIndex in tri)
            {
                if (edgeIndex == -1)
                    continue;

                GridCell.Edge edge = GridCell.edges[edgeIndex];

                float valueA = cell.Values[GridCell.vertices.IndexOf(edge.A)];
                float valueB = cell.Values[GridCell.vertices.IndexOf(edge.B)];

                Vector3 p = edge.InterpolateMidpoint(valueA, valueB, surfaceLevel);

                vertices.Add(p * cell.Scale + (Vector3)cell.Position * cell.Scale);
                triangles.Add(vertexCount++);
            }

            foreach (int edgeIndex in tri.Reverse())
            {
                if (edgeIndex == -1)
                    continue;

                GridCell.Edge edge = GridCell.edges[edgeIndex];

                float valueA = cell.Values[GridCell.vertices.IndexOf(edge.A)];
                float valueB = cell.Values[GridCell.vertices.IndexOf(edge.B)];

                Vector3 p = edge.InterpolateMidpoint(valueA, valueB, surfaceLevel);

                vertices.Add(p * cell.Scale + (Vector3)cell.Position * cell.Scale);
                triangles.Add(vertexCount++);
            }
        });

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

    public GridCell GetCell(Vector3Int cellPos)
    {
        return cells[cellPos.x, cellPos.y, cellPos.z];
    }

    private void SetCell(Vector3Int cellPos, GridCell cell)
    {
        cells[cellPos.x, cellPos.y, cellPos.z] = cell;
    }

    public void ForeachCoordinate(Action<Vector3Int> action)
    {
        for (int x = 0; x < cells.GetUpperBound(0) + 1; x++)
        {
            for (int y = 0; y < cells.GetUpperBound(1) + 1; y++)
            {
                for (int z = 0; z < cells.GetUpperBound(2) + 1; z++)
                {
                    action(new Vector3Int(x, y, z));
                }
            }
        }
    }
}
