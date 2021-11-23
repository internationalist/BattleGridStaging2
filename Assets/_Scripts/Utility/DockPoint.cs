using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DockPoint
{
    public Vector3 position;
    public bool isOccupied;
    public int controllerID;

    private DockPoint(Vector3 location)
    {
        this.position = location;
    }

    public static DockPoint Instance(Vector3 location)
    {
        return new DockPoint(location);
    }
}
