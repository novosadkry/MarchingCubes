using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Grid : MonoBehaviour
{
    [Serializable]
    public class GridData
    {
        [field: SerializeField]
        public int Seed { get; set; }
        
        [field: SerializeField]
        public float MaxHeight { get; set; }
        
        [field: SerializeField]
        public float Frequency { get; set; }
        
        [field: SerializeField]
        public float SurfaceLevel { get; set; }
        
        [field: SerializeField]
        public int CellCount { get; set; }
        
        [field: SerializeField]
        public Gradient ColorGradient { get; set; }
    }
    
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;
    
    [field: SerializeField]
    public Vector3Int GridPosition { get; set; }
        
    [field: SerializeField]
    public float GridScale { get; set; }
    
    [field: Space]
    [field: SerializeField]
    public GridData Data { get; set; }

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
            ForeachCoordinate(pos =>
            {
                var cell = GetCell(pos);

                for (int i = 0; i < 12; i++)
                {
                    var edge = GridCell.Edges[i];

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
        FastNoise noiseGen = new FastNoise(Data.Seed);

        cells = new GridCell[Data.CellCount, Data.CellCount, Data.CellCount];

        ForeachCoordinate(pos =>
        {
            GridCell cell = new GridCell
            {
                Scale = GridScale / Data.CellCount,
                Position = pos
            };

            for (int i = 0; i < 8; i++)
            {
                Vector3 valuePos = (Vector3)GridPosition * GridScale + cell.GetValuePos(i);
                
                float noise = noiseGen.GetPerlin(
                    valuePos.x / Data.Frequency,
                    valuePos.z / Data.Frequency
                );

                noise = (noise + 1) / 2;
                
                float height = noise * Data.MaxHeight - valuePos.y;
                float value = 1 - height / Data.MaxHeight;

                cell.Values[i] = value;
            }

            SetCell(pos, cell);
        });
    }

    public void ConstructMesh()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Color> colors = new List<Color>();

        int vertexCount = 0;

        ForeachCoordinate(pos => 
        {
            GridCell cell = GetCell(pos);

            int cubeIndex = cell.GetCubeIndex(Data.SurfaceLevel);
            int[] tri = Table.triangulation[cubeIndex];

            foreach (int edgeIndex in tri)
            {
                if (edgeIndex == -1)
                    continue;

                GridCell.Edge edge = GridCell.Edges[edgeIndex];

                float valueA = cell.Values[GridCell.Vertices.IndexOf(edge.A)];
                float valueB = cell.Values[GridCell.Vertices.IndexOf(edge.B)];

                Vector3 p = edge.InterpolateMidpoint(valueA, valueB, Data.SurfaceLevel);
                Vector3 pPos = p * cell.Scale + (Vector3)cell.Position * cell.Scale;

                vertices.Add(pPos);
                triangles.Add(vertexCount++);
                colors.Add(Data.ColorGradient.Evaluate((pPos + GridPosition).y / Data.MaxHeight));
            }

            foreach (int edgeIndex in tri.Reverse())
            {
                if (edgeIndex == -1)
                    continue;

                GridCell.Edge edge = GridCell.Edges[edgeIndex];

                float valueA = cell.Values[GridCell.Vertices.IndexOf(edge.A)];
                float valueB = cell.Values[GridCell.Vertices.IndexOf(edge.B)];

                Vector3 p = edge.InterpolateMidpoint(valueA, valueB, Data.SurfaceLevel);
                Vector3 pPos = p * cell.Scale + (Vector3)cell.Position * cell.Scale;

                vertices.Add(pPos);
                triangles.Add(vertexCount++);
                colors.Add(Data.ColorGradient.Evaluate((pPos + (Vector3)GridPosition * GridScale).y / Data.MaxHeight));
            }
        });

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.colors = colors.ToArray();
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

    public bool HasPosition(Vector3 pos)
    {
        Vector3 p1 = (Vector3)GridPosition * GridScale;
        Vector3 p2 = p1 + new Vector3(GridScale, GridScale, GridScale);
        
        return p1.x <= pos.x && pos.x <= p2.x
            && p1.y <= pos.y && pos.y <= p2.y
            && p1.z <= pos.z && pos.z <= p2.z;
    }
    
    public GridCell GetCell(Vector3Int cellPos)
    {
        return cells[cellPos.x, cellPos.y, cellPos.z];
    }

    private void SetCell(Vector3Int cellPos, GridCell cell)
    {
        cells[cellPos.x, cellPos.y, cellPos.z] = cell;
    }

    public bool IsEmpty
    {
        get
        {
            bool isEmpty = true;

            ForeachCoordinate(pos =>
            {
                GridCell cell = GetCell(pos);

                if (!cell.IsEmpty)
                    isEmpty = false;
            });

            return isEmpty;
        }
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
