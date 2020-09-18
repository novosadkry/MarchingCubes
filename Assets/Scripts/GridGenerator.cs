using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    public GameObject chunkObject;

    [Range(0.0f, 1.0f)]
    public float surfaceLevel;

    public float gridScale;
    public int cellCount;

    public Vector3Int size;

    private List<Grid> grids = new List<Grid>();
    private Queue<Grid> refreshQueue = new Queue<Grid>();

    void Start()
    {
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                for (int k = 0; k < size.z; k++)
                {
                    GameObject o = Instantiate(chunkObject, transform);

                    Grid grid = o.GetComponent<Grid>();

                    grid.surfaceLevel = surfaceLevel;
                    grid.GridScale = gridScale;
                    grid.CellCount = cellCount;

                    grid.GridPosition = new Vector3(i, j, k);
                    grid.transform.position = grid.GridPosition * grid.GridScale;

                    grid.GenerateGridValues();
                    grid.ConstructMesh();

                    grids.Add(grid);
                }
            }
        }

        StartCoroutine(RefreshGrids());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            foreach (var grid in grids)
            {
                if (!refreshQueue.Contains(grid))
                    refreshQueue.Enqueue(grid);
            }
        }
    }

    IEnumerator RefreshGrids()
    {
        while (true)
        {
            if (refreshQueue.Count > 0)
            {
                var grid = refreshQueue.Dequeue();

                grid.surfaceLevel = surfaceLevel;
                grid.CellCount = cellCount;
                grid.GridScale = gridScale;

                grid.transform.position = grid.GridPosition * grid.GridScale;

                grid.GenerateGridValues();
                grid.ConstructMesh();
            }

            yield return new WaitForEndOfFrame();
        }
    }
}
