using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    public GameObject chunkObject;

    [Space]
    public int seed;

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

        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                for (int k = 0; k < size.z; k++)
                {
                    refreshQueue.Enqueue(new Vector3Int(i, j, k));
                }
            }
        }

        StartCoroutine(RefreshGrids());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            for (int i = 0; i < size.x; i++)
            {
                for (int j = 0; j < size.y; j++)
                {
                    for (int k = 0; k < size.z; k++)
                    {
                        refreshQueue.Enqueue(new Vector3Int(i, j, k));
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
