using UnityEngine;

[RequireComponent(typeof(Camera))]
public class TerraformTool : MonoBehaviour
{
    public GridGenerator gridGenerator;

    public float maxDistance;
    private Camera cam;

    public GameObject toolPrefab;
    private GameObject toolPrefabInstance;

    private float toolScale;
    public float toolStrength;
    public float toolTick;

    private float nextTick = 0.0f;

    void Awake()
    {
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
        float toolScaleSqrt = toolScale * toolScale / 4;

        toolPrefabInstance.transform.localScale = Vector3.one * toolScale;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        Vector3 toolPos = Physics.Raycast(ray, out RaycastHit hitInfo)
            ? hitInfo.point
            : ray.direction * maxDistance + ray.origin;

        toolPrefabInstance.transform.position = toolPos;

        if (Input.GetMouseButton(0))
        {
            if (Time.time > nextTick)
            {
                nextTick = Time.time + toolTick;

                gridGenerator.ForeachCoordinate(gridPos =>
                {
                    Grid grid = gridGenerator.Grids[gridPos];

                    grid.ForeachCoordinate(cellPos =>
                    {
                        GridCell cell = grid.GetCell(cellPos);
                        bool refresh = false;

                        for (int i = 0; i < 8; i++)
                        {
                            Vector3 valuePos = grid.transform.position + cell.GetValuePos(i);

                            if ((toolPos - valuePos).sqrMagnitude < toolScaleSqrt)
                            {
                                if (Input.GetKey(KeyCode.LeftShift))
                                    cell.Values[i] -= toolStrength;
                                else
                                    cell.Values[i] += toolStrength;
                                
                                refresh = true;
                            }
                        }

                        if (refresh)
                            gridGenerator.RefreshChunk(gridPos);
                    });
                });
            }
        }
    }
}
