using System.Collections.Generic;
using UnityEngine;

/*
 * Handles the drawing and identification of Rects sorted into zones based on their proximity to one another.
 * 
 * Author: William Parsons
 */
public class ZoneManager : MonoBehaviour
{
    // singleton
    [HideInInspector] public static ZoneManager instance;

    // properties from this material are copied to meshes when they are created
    public Material meshMaterial;

    // the union find structure
    private RectUnionFind uf;

    // track the objects used to draw each distinct zone
    private List<GameObject> meshChildren;

    // flags when meshes have been altered and need re-drawing
    private bool meshesNeedUpdating = true;

    // caches colours in use to avoid them changing every time the meshes are re-drawn
    private List<Color> colors;

    /*
     * Start is called before the first frame update
     */
    void Start()
    {
        instance = this;
        meshChildren = new List<GameObject>();
        colors = new List<Color>();
        uf = new RectUnionFind();
    }

    /*
     * Update is called once per frame.
     */
    void Update()
    {
        if (meshesNeedUpdating) drawMeshes();
    }

    /*
     * Creates a child object with a mesh for each distinct zone and draws its mesh.
     */
    private void drawMeshes()
    {
        // delete old meshs
        foreach (GameObject g in meshChildren) Destroy(g);
        meshChildren.Clear();

        // draw new meshs
        Rect[][] data = uf.getDistinctZoneRects();
        int i = 0;
        foreach(Rect[] points in data)
        {
            // make a random color
            Color c;

            if(i < colors.Count)
                c = colors[i];
            else
            {
                c = new Color(Random.value, Random.value, Random.value, .4f);
                colors.Add(c);
            }

            createMesh(points, "Mesh " + i, c);
            i++;
        }

        meshesNeedUpdating = false;
    }

    /*
     * Creates and draws a single mesh object from a group of Rects
     */
    private void createMesh(Rect[] rects, string name, Color col)
    {
        // set up new game object
        GameObject go = new GameObject(name);
        go.transform.parent = transform;
        go.transform.position = new Vector3(0, 0.01f, 0);

        MeshFilter mf = go.AddComponent<MeshFilter>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();

        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.material = new Material(meshMaterial);
        mr.material.SetColor("_BaseColor", col);

        meshChildren.Add(go);

        // Fill vertices and triangles
        Vector3[] vertices = new Vector3[rects.Length * 4];
        int[] triangles = new int[rects.Length * 6];

        int vertIndex = 0;
        int triIndex = 0;
        foreach (Rect r in rects)
        {
            vertices[vertIndex] = new Vector3(r.x, 0.5f, r.y);
            vertices[vertIndex + 1] = new Vector3(r.x + r.width, 0.5f, r.y);
            vertices[vertIndex + 2] = new Vector3(r.x, 0.5f, r.y + r.height);
            vertices[vertIndex + 3] = new Vector3(r.x + r.width, 0.5f, r.y + r.height);

            triangles[triIndex + 0] = vertIndex + 0;
            triangles[triIndex + 1] = vertIndex + 2;
            triangles[triIndex + 2] = vertIndex + 3;
            triangles[triIndex + 3] = vertIndex + 1;
            triangles[triIndex + 4] = vertIndex + 0;
            triangles[triIndex + 5] = vertIndex + 3;

            vertIndex += 4;
            triIndex += 6;
        }

        // draw mesh
        Mesh m = new Mesh();
        mf.mesh = m;
        m.vertices = vertices;
        m.triangles = triangles;
        m.RecalculateBounds();
        m.RecalculateNormals();
    }

    /*
     * Adds a Rect to the UnionFind structure and calculates which other Rects 
     * it should union with.
     */
    public void addRect(Rect r)
    {
        // very small Rects are not allowed
        if (getSizeOfRect(r) < 1) return;

        // add rect to union find
        uf.add(r);

        // union with any rects we are touching
        calculateUnions(r);

        // find any rects we are overlapping and subtract ourselves from them
        foreach (Rect rect in uf.getAllRects())
            if (areRectsTouching(rect, r) && rect != r)
            {
                if (getSizeOfRect(getOverlap(rect, r)) > 0)
                {
                    cutOutOverlap(rect, r);
                }
            }

        // signal that meshs need to be redrawn
        meshesNeedUpdating = true;

    }

    /*
     * Removes any rects that fall in the area of the delete Rect.
     */
    public void deleteRect(Rect deleteArea)
    {
        // find any rects we are overlapping and subtract ourselves from them
        foreach (Rect rect in uf.getAllRects())
            if (areRectsTouching(rect, deleteArea))
                if (getSizeOfRect(getOverlap(rect, deleteArea)) > 0)
                    cutOutOverlap(rect, deleteArea);

        // force any Rect affected by the delete operation to recalculate its unions
        foreach(Rect rect in uf.getAllRects())
            if(areRectsTouching(rect, deleteArea))
                recalculateUnions(uf.getAllRectsInAZone(rect));
    }

    /*
     * Cuts a given area from a Rect and replaces the existing Rect with 
     * multiple Rects making up the remainder.
     */
    private void cutOutOverlap(Rect rect, Rect deleteArea)
    {
        // fragment the original rect
        Rect[] fragmentsOfRect = cutoutRect(rect, deleteArea);

        // remove the original
        uf.remove(rect);

        // add all of the fragments
        foreach (Rect frag in fragmentsOfRect)
            addRect(frag);

        // union all of the fragments
        foreach (Rect frag in fragmentsOfRect)
            calculateUnions(frag);

        // update mesh
        meshesNeedUpdating = true;
    }

    /*
     * Unions the given Rect with any Rects it is touching.
     */
    private void calculateUnions(Rect r)
    {
        foreach (Rect rect in uf.getAllRects())
            if (areRectsTouching(rect, r) && rect != r)
                uf.Union(r, rect);

        meshesNeedUpdating = true;
    }

    /*
     * Forces the Rects in the input array to recalculate their unions.
     */
    private void recalculateUnions(Rect[] r)
    {
        foreach (Rect rect in r) uf.removeUnion(rect);
        foreach (Rect rect in r) calculateUnions(rect);
    }

    /**
     * Returns whether or not two Rects are touching.
     * Will return true if they are merely touching, or are overlapping.
     */
    private bool areRectsTouching(Rect rectA, Rect rectB)
    {
        // break if we are checking ourself
        if (rectA == rectB) return true;

        // detect merely touching as well as overlapping by modifying one rect to be slightly larger
        float g = Grid.instance.cellSize;
        Rect r = new Rect(rectA.x - (g/2), rectA.y - (g/2), rectA.width + g, rectA.height + g);

        if (r.Overlaps(rectB, true) || rectB.Overlaps(r, true)) {
            if (getSizeOfRect(getOverlap(r, rectB)) > 0.25) // prevents Rects that share a corner being treated as touching
                return true;
        }

        return false;
    }

    /*
     * Returns the Rect that results from the overlap of r1 and r2.
     */
    private Rect getOverlap(Rect r1, Rect r2)
    {
        Rect area = new Rect();

        if (r2.Overlaps(r1))
        {
            float x1 = Mathf.Min(r1.xMax, r2.xMax);
            float x2 = Mathf.Max(r1.xMin, r2.xMin);
            float y1 = Mathf.Min(r1.yMax, r2.yMax);
            float y2 = Mathf.Max(r1.yMin, r2.yMin);
            area.x = Mathf.Min(x1, x2);
            area.y = Mathf.Min(y1, y2);
            area.width = Mathf.Max(0.0f, x1 - x2);
            area.height = Mathf.Max(0.0f, y1 - y2);
        }

        
        return area;
    }

    /*
     * Returns the size in grid squares of the given rectangle.
     */
    private float getSizeOfRect(Rect r)
    {
        float size = r.width / Grid.instance.cellSize * (r.height / Grid.instance.cellSize);
        return size;
    }

    /*
     * Returns the total number of distinct zones made up by the drawn Rects.
     */
    public int getDistinctAreas()
    {
        return uf.getNumOfDistinctZones();
    }

    /*
     * Returns the unique ID of the area that the given Vector2 is contained by, or -1 if the point is not contained by any area.
     */
    public int getAreaPointIsIn(Vector2 point)
    {
        foreach (Rect r in uf.getAllRects())
            if (r.Contains(point)) return uf.getRoot(r);

        return -1;
    }

    /*
     * Returns the total number of grid squares taken up by all of the drawn Rects.
     */
    public int getSizeOfAllRects()
    {
        int s = 0;
        foreach (Rect r in uf.getAllRects()) s += (int)getSizeOfRect(r);
        return s;
    }

    /*
     * Cuts a Rect from another Rect, and returns an array of new Rects that make up the shape resulting from
     * initial Rect minus the cutout Rect.
     * 
     * -------------------------
     * |          A            |
     * |-----------------------|
     * |  B  |  cutout   |  C  |
     * |-----------------------|
     * |          D            |
     * -------------------------
     */
    private Rect[] cutoutRect(Rect shape, Rect cutout)
    {
        // if the cutout is of size 0
        if (cutout.width == 0 || cutout.height == 0) return new Rect[] { shape };

        // if the cutout is larger than the shape, cull all of it
        if (getSizeOfRect(getOverlap(shape, cutout)) >= getSizeOfRect(shape)) return new Rect[0];

        // store the results
        List<Rect> list = new List<Rect>();

        // the cutout is the overlap
        cutout = new Rect(getOverlap(shape, cutout));

        // calculate rectangle A
        float heightA = cutout.yMin - shape.yMin;
        if(heightA > 0)
        {
            Rect r = new Rect(
                shape.xMin,
                shape.yMin,
                shape.width,
                heightA);

            list.Add(r);
        }


        // calculate rectangle B
        float widthB = cutout.xMin - shape.xMin;
        if(widthB > 0)
        {
            Rect r = new Rect(
                shape.xMin,
                cutout.yMin,
                widthB,
                cutout.height);

            list.Add(r);
        }


        // calculate rectangle C
        float widthC = shape.xMax - cutout.xMax;
        if (widthC > 0)
        {
            Rect r = new Rect(
                cutout.xMax,
                cutout.yMin,
                widthC,
                cutout.height);

            list.Add(r);
        }

        // calculate rectangle D
        float heightD = shape.yMax - cutout.yMax;
        if (heightD > 0)
        {
            Rect r = new Rect(
                shape.xMin,
                cutout.yMax,
                shape.width,
                heightD);

            list.Add(r);
        }

        return list.ToArray();
    }
}