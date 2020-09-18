using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    public GameObject chunkObject;

    [Space]
    public int seed;
    public float frequency;

    [Range(0.0f, 1.0f)]
    public float surfaceLevel;

    public float gridScale;
    public int cellCount;

    public Vector3Int size;

    private Dictionary<Vector3Int, Grid> grids;
    private Queue<Vector3Int> refreshQueue;

    void Awake()
    {
        grids = new Dictionary<Vector3Int, Grid>();
        refreshQueue = new Queue<Vector3Int>();

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                for (int z = 0; z < size.z; z++)
                {
                    refreshQueue.Enqueue(new Vector3Int(x, y, z));
                }
            }
        }

        StartCoroutine(RefreshGrids());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    for (int z = 0; z < size.z; z++)
                    {
                        refreshQueue.Enqueue(new Vector3Int(x, y, z));
                    }
                }
            }
        }
    }

    IEnumerator RefreshGrids()
    {
        while (true)
        {
            if (refreshQueue.Count > 0)
            {
                Vector3Int gridPosition = refreshQueue.Dequeue();

                if (grids.ContainsKey(gridPosition))
                {
                    Grid grid = grids[gridPosition];

                    grid.Seed = seed;
                    grid.Frequency = frequency;
                    grid.surfaceLevel = surfaceLevel;
                    grid.CellCount = cellCount;
                    grid.GridScale = gridScale;

                    grid.transform.position = (Vector3)grid.GridPosition * grid.GridScale;

                    grid.GenerateGridValues();
                    grid.ConstructMesh();
                }

                else
                    AddChunk(gridPosition);
            }

            yield return new WaitForEndOfFrame();
        }
    }

    public void AddChunk(int x, int y, int z)
    {
        AddChunk(new Vector3Int(x, y, z));
    }

    public void AddChunk(Vector3Int gridPosition)
    {
        GameObject gridObject = Instantiate(chunkObject, transform);
        Grid grid = gridObject.GetComponent<Grid>();

        grid.Seed = seed;
        grid.Frequency = frequency;
        grid.surfaceLevel = surfaceLevel;
        grid.GridScale = gridScale;
        grid.CellCount = cellCount;

        grid.GridPosition = gridPosition;
        grid.transform.position = (Vector3)grid.GridPosition * grid.GridScale;

        grid.GenerateGridValues();
        grid.ConstructMesh();

        grids.Add(grid.GridPosition, grid);
    }
}
