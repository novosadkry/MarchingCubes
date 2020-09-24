using System.Collections.Generic;
using UnityEngine;
using Voxel;

[RequireComponent(typeof(Camera))]
public class TerraformTool : MonoBehaviour
{
    public enum TerraformMode
    {
        Add,
        Subtract
    }
    
    public GridGenerator gridGenerator;

    public float maxDistance;
    public float minDistance;
    private Camera cam;

    public GameObject toolPrefab;
    private GameObject toolPrefabInstance;

    private float toolScale;
    public float toolStrength;
    public float toolTick;

    private float nextTick;
    
    // Used to prevent stack overflow
    private List<Vector3Int> ignoreGrids;

    void Awake()
    {
        ignoreGrids = new List<Vector3Int>();
        
        if (gridGenerator == null)
            Debug.LogError("GridGenenerator field cannot be null");

        cam = GetComponent<Camera>();
    }

    void Start()
    {
        toolPrefabInstance = Instantiate(toolPrefab, transform);
    }

    void Update()
    {
        toolScale += Input.mouseScrollDelta.y;
        toolScale = Mathf.Clamp(toolScale, 0f, 50f);

        toolPrefabInstance.transform.localScale = Vector3.one * toolScale;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        Vector3 toolPos = Physics.Raycast(ray, out RaycastHit hitInfo)
            ? hitInfo.point
            : ray.direction * maxDistance + ray.origin;

        bool isNear = Vector3.Distance(toolPos, ray.origin) - toolScale / 2 < minDistance;
        
        toolPrefabInstance.SetActive(!isNear);
        toolPrefabInstance.transform.position = toolPos;

        if (Input.GetMouseButton(0))
        {
            if (Time.time > nextTick)
            {
                nextTick = Time.time + toolTick;

                var terraformMode = Input.GetKey(KeyCode.LeftShift)
                    ? TerraformMode.Add
                    : TerraformMode.Subtract;

                Voxel.Grid grid = gridGenerator.GetGridFromWorldPosition(toolPos);

                if (!ReferenceEquals(grid, null))
                    Terraform(grid, toolPos, toolScale / 2, toolStrength, terraformMode);
                
                ignoreGrids.Clear();
            }
        }
    }

    public void Terraform(Voxel.Grid grid, Vector3 pos, float radius, float strength, TerraformMode mode)
    {
        ISet<Vector3Int> toRefresh = new HashSet<Vector3Int>();
        
        grid.ForeachCoordinate(cellPos =>
        {
            GridCell cell = grid.GetCell(cellPos);

            for (int i = 0; i < 8; i++)
            {
                Vector3 valuePos = (Vector3)grid.GridPosition * grid.GridScale + cell.GetValuePos(i);
                float distanceSqrt = (pos - valuePos).sqrMagnitude;

                if (distanceSqrt < radius * radius)
                {
                    switch (mode)
                    {
                        case TerraformMode.Add:
                            cell.Values[i] -= strength;
                            break;
                        case TerraformMode.Subtract:
                            cell.Values[i] += strength;
                            break;
                    }

                    if (cellPos.x == 0)
                        toRefresh.Add(grid.GridPosition + new Vector3Int(-1, 0, 0));
                    else if (cellPos.x == grid.Data.CellCount - 1)
                        toRefresh.Add(grid.GridPosition + new Vector3Int(1, 0, 0));
                    
                    if (cellPos.y == 0)
                        toRefresh.Add(grid.GridPosition + new Vector3Int(0, -1, 0));
                    else if (cellPos.y == grid.Data.CellCount - 1)
                        toRefresh.Add(grid.GridPosition + new Vector3Int(0, 1, 0));
                    
                    if (cellPos.z == 0)
                        toRefresh.Add(grid.GridPosition + new Vector3Int(0, 0, -1));
                    else if (cellPos.z == grid.Data.CellCount - 1)
                        toRefresh.Add(grid.GridPosition + new Vector3Int(0, 0, 1));
                }
            }
        });
        
        ignoreGrids.Add(grid.GridPosition);

        foreach (Vector3Int gridPos in toRefresh)
        {
            if (ignoreGrids.Contains(gridPos)) 
                continue;
            
            if (gridGenerator.Grids.TryGetValue(gridPos, out Voxel.Grid refreshGrid))
                Terraform(refreshGrid, pos, radius, strength, mode);
        }
        
        gridGenerator.RefreshGrid(grid.GridPosition);
    }
}
