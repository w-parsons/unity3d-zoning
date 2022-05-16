using UnityEngine;

/**
 * Allows the user to draw a mesh to the terrain with their mouse, which is then
 * sent to the ZoneManager to be compiled into a zone. Also supports deleting of existing zones.
 * 
 * Author: William Parsons
 */
public class DrawZone : MonoBehaviour
{
    // Components of the visible Mesh
    private MeshFilter meshFilter;
    private Mesh mesh;
    private int[] triangles;
    private Vector3[] boxPoints;

    // Terrain properties
    private Grid grid;
    private const float TERRAIN_Y = 0.51f;
    private int layerMask;

    // Drawing mode
    public enum DrawingMode { Draw, Delete };
    private DrawingMode mode = DrawingMode.Draw;

    // Start is called before the first frame update
    void Start()
    {
        // set up mesh
        boxPoints = new Vector3[4];
        mesh = new Mesh();
        meshFilter = GetComponent<MeshFilter>();

        // get grid reference
        grid = GameObject.Find("Grid").GetComponent<Grid>();

        // set up mask
        layerMask = 1 << 8;
    }

    // Update is called once per frame
    void Update()
    {
        createBoxOnInput();
    }

    /*
     * Creates a mesh if the correct inputs are made
     */
    private void createBoxOnInput()
    {
        // begin drawing
        if (Input.GetMouseButtonDown(0))
        {
            // originate box at grid point closest to click position
            boxPoints[0] = grid.GetNearestPointOnGridCorner(getTerrainPositionFromMouse());
        }

        // update remaining 3 points as the box mouse moves
        if (Input.GetMouseButton(0))
        {
            boxPoints[3] = grid.GetNearestPointOnGridCorner(getTerrainPositionFromMouse());
            boxPoints[1] = new Vector3(boxPoints[3].x, TERRAIN_Y, boxPoints[0].z);
            boxPoints[2] = new Vector3(boxPoints[0].x, TERRAIN_Y, boxPoints[3].z);

            generateMesh(boxPoints);
        }

        // when mouse is released, send mesh data to ZoneManager
        if (Input.GetMouseButtonUp(0))
        {
            float x = Mathf.Min(new float[] { boxPoints[0].x, boxPoints[1].x, boxPoints[2].x, boxPoints[3].x });
            float y = Mathf.Min(new float[] { boxPoints[0].z, boxPoints[1].z, boxPoints[2].z, boxPoints[3].z });
            float w = Vector3.Distance(boxPoints[0], boxPoints[1]);
            float h = Vector3.Distance(boxPoints[0], boxPoints[2]);

            if (mode == DrawingMode.Draw) ZoneManager.instance.addRect(new Rect(x, y, w, h));
            else if (mode == DrawingMode.Delete) ZoneManager.instance.deleteMesh(new Rect(x, y, w, h));
            
            mesh.Clear();
        }
    }

    /*
     * Paints a mesh on the terrain.
     */
    private void generateMesh(Vector3[] verts)
    {
        // choose triangles based on direction box has been dragged to avoid backwards culling issue
        if ((verts[3].x > verts[0].x && verts[3].z > verts[0].z)
            || (verts[0].x > verts[3].x && verts[0].z > verts[3].z))
        {
            triangles = new int[]
            {
                0,2,3,
                1,0,3
            };
        }
        else
        {
            triangles = new int[]
            {
                0,1,2,
                1,3,2
            };
        }

        // generate mesh
        mesh.Clear();
        mesh.vertices = verts;
        mesh.triangles = triangles;
        mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 2000);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }

    /*
     * Gets the terrain coordinates of the mouse position.
     */
    private Vector3 getTerrainPositionFromMouse()
    {
        Vector3 clickPos = new Vector3();

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            clickPos = hit.point;
            clickPos.y = TERRAIN_Y;
        }

        return clickPos;
    }

    /*
     * Accessor to manipulate drawing mode, used in UI.
     */
    public void setDrawingMode(DrawingMode m)
    {
        mode = m;
    }
}
