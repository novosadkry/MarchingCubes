using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Voxel
{
    public class GridGenerator : MonoBehaviour
    {
        [Space]
        public int seed;
    
        [Space]
        public float maxHeight;
        public float frequency;

        [Range(0.0f, 1.0f)]
        public float surfaceLevel;

        [Space]
        public float gridScale;
        public int cellCount;
        public Vector3Int size;

        [Space]
        public Gradient colorGradient;

        public Dictionary<Vector3Int, Grid> Grids { get; private set; }
        private Queue<Vector3Int> refreshQueue;

        private GameObjectPool gridObjectPool;

        void Awake()
        {
            Grids = new Dictionary<Vector3Int, Grid>();
            refreshQueue = new Queue<Vector3Int>();
            gridObjectPool = GetComponent<GameObjectPool>();

            ForeachCoordinate(pos => refreshQueue.Enqueue(pos));

            StartCoroutine(RefreshGrids());
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.G))
                ForeachCoordinate(pos => refreshQueue.Enqueue(pos));
        }

        IEnumerator RefreshGrids()
        {
            while (true)
            {
                if (refreshQueue.Count > 0)
                {
                    Vector3Int gridPosition = refreshQueue.Dequeue();

                    if (Grids.ContainsKey(gridPosition))
                        UpdateGrid(gridPosition);
                    else
                        AddGrid(gridPosition);
                }

                yield return new WaitForEndOfFrame();
            }
        }

        public void RefreshGrid(int x, int y, int z)
        {
            RefreshGrid(new Vector3Int(x, y, z));
        }

        public void RefreshGrid(Vector3Int gridPos)
        {
            if (!refreshQueue.Contains(gridPos))
                refreshQueue.Enqueue(gridPos);
        }

        public void AddGrid(int x, int y, int z)
        {
            AddGrid(new Vector3Int(x, y, z));
        }

        public void AddGrid(Vector3Int gridPos)
        {
            GameObject gridObject = gridObjectPool.GetPooled();
            gridObject.SetActive(true);
        
            Grid grid = gridObject.GetComponent<Grid>();

            Grid.GridData data = new Grid.GridData
            {
                Seed = seed,
                MaxHeight = maxHeight,
                Frequency = frequency,
                SurfaceLevel = surfaceLevel,
                CellCount = cellCount,
                ColorGradient = colorGradient
            };

            grid.GridPosition = gridPos;
            grid.GridScale = gridScale;
            grid.Data = data;
        
            grid.transform.position = (Vector3)grid.GridPosition * grid.GridScale;

            grid.GenerateGridValues();
            grid.ConstructMesh();

            Grids.Add(grid.GridPosition, grid);
        }

        public void UpdateGrid(Vector3Int gridPos)
        {
            Grid grid = Grids[gridPos];

            Grid.GridData data = new Grid.GridData
            {
                Seed = seed,
                MaxHeight = maxHeight,
                Frequency = frequency,
                SurfaceLevel = surfaceLevel,
                CellCount = cellCount,
                ColorGradient = colorGradient
            };

            grid.GridPosition = gridPos;
            grid.GridScale = gridScale;
            grid.Data = data;
        
            grid.transform.position = (Vector3)grid.GridPosition * grid.GridScale;

            //grid.GenerateGridValues();
            grid.ConstructMesh();
        }

        public Grid GetGridFromWorldPosition(Vector3 pos)
        {
            Grid grid = null;

            ForeachCoordinate(gridPos =>
            {
                Grid g = Grids[gridPos];

                if (g.HasPosition(pos))
                {
                    grid = g;
                    return;
                }
            });

            return grid;
        }

        public void ForeachCoordinate(Action<Vector3Int> action)
        {
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    for (int z = 0; z < size.z; z++)
                    {
                        action(new Vector3Int(x, y, z));
                    }
                }
            }
        }
    }
}
