using UnityEngine;

/*
 * Grid class with functions to return positions that conform to the grid.
 * 
 * Adapted from https://unity3d.college/2017/10/08/simple-unity3d-snap-grid-system/
 */
public class Grid : MonoBehaviour
{
    // the width and height property of each grid cell
    public float cellSize;

    // singleton object
    [HideInInspector] public static Grid instance;

    private void Awake()
    {
        instance = this;
    }

    /*
     * Finds the nearest point to the input point that falls on the corner of a Grid square.
     */
    public Vector3 GetNearestPointOnGridCorner(Vector3 position)
    {
        position -= transform.position;

        int xCount = Mathf.RoundToInt(position.x / cellSize);
        int zCount = Mathf.RoundToInt(position.z / cellSize);

        Vector3 result = new Vector3(
            (float)xCount * cellSize,
            position.y,
            (float)zCount * cellSize);

        result += transform.position;

        return result;
    }
}
