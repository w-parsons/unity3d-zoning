using System.Collections.Generic;
using UnityEngine;

/**
 * Implentation of a Union Find data structure, taking Rect objects.
 * More info on Union Find can be found here: https://en.wikipedia.org/wiki/Disjoint-set_data_structure
 * 
 * Author: William Parsons
 */
public class RectUnionFind
{
    // tracks each Rect and its corresponding root
    private Dictionary<Rect, int> id;

    // tracks the largest used root so far, allowing us to add new areas by themselves
    private int maxRoot;

    /*
     * Initialise the object.
     */
    public RectUnionFind()
    {
        id = new Dictionary<Rect, int>();
        maxRoot = 1;
    }

    /*
     * Get the number of Rects contained in the data structure.
     */
    public int size()
    {
        return id.Count;
    }

    /*
     * Get the list of Rects contained in the data structure.
     */
    public Rect[] getAllRects()
    {
        return new List<Rect>(id.Keys).ToArray();
    }

    /*
     * Get the list of Rects that make up the zone that the input Rect 'r' is in.
     */
    public Rect[] getAllRectsInAZone(Rect r)
    {
        List<Rect> l = new List<Rect>();

        foreach(KeyValuePair<Rect, int> pair in id)
        {
            if(getRoot(pair.Key) == getRoot(r))
            {
                l.Add(pair.Key);
            }
        }

        return l.ToArray();
    }

    /*
     * Gets each distinct zone and all of the Rects that make them up.
     */
    public Rect[][] getDistinctZoneRects()
    {
        List<int> areaCodes = new List<int>();
        foreach(KeyValuePair<Rect, int> pair in id)
        {
            if (!areaCodes.Contains(pair.Value)) areaCodes.Add(pair.Value);
        }

        List<Rect[]> l = new List<Rect[]>();
        foreach(int area in areaCodes)
        {
            List<Rect> temp = new List<Rect>();
            foreach (KeyValuePair<Rect, int> pair in id)
                if (area == pair.Value)
                    // this square belongs to this distinct area
                    temp.Add(pair.Key);
                
            l.Add(temp.ToArray());
        }

        return l.ToArray();
    }

    /*
     * Gets the number of distinct zones.
     */
    public int getNumOfDistinctZones()
    {
        List<int> uniqueKeys = new List<int>();

        foreach(KeyValuePair<Rect, int> pair in id)
            if (!uniqueKeys.Contains(pair.Value))
                uniqueKeys.Add(pair.Value);

        return uniqueKeys.Count;
    }

    /*
     * Gets the root of a given Rect.
     */
    public int getRoot(Rect i)
    {
        return id[i];
    }

    /*
     * Adds a new Rect to the structure, standing by itself.
     */
    public void add(Rect i)
    {
        id.Add(i, maxRoot);
        maxRoot++;
    }

    /*
     * Removes the union between two Rects.
     */
    public void removeUnion(Rect rectToIsolate)
    {
        id[rectToIsolate] = maxRoot;
        maxRoot++;
    }

    /*
     * Checks whether a union exists between two Rects.
     */
    public bool isConnected(Rect a, Rect b)
    {
        return getRoot(a) == getRoot(b);
    }

    /*
     * Performs a union between two Rects.
     */
    public void Union(Rect a, Rect b)
    {
        int aid = id[a];
        int bid = id[b];

        Dictionary<Rect, int> copy = new Dictionary<Rect, int>(id);
        foreach(KeyValuePair<Rect, int> pair in copy)
        {
            if (pair.Value == aid) id[pair.Key] = bid;
        }
    }

    /*
     * Checks if a given Rect is in the data structure.
     */
    public bool contains(Rect r)
    {
        return id.ContainsKey(r);
    }

    /*
     * Removes a Rect from the data structure.
     */
    public void remove(Rect r)
    {
        id.Remove(r);
    }
}