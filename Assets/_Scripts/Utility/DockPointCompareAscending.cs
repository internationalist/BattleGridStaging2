using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DockPointCompareAscending: DockPointCompare
{
    public DockPointCompareAscending(Vector3 compareWith)
    {
        this.compareWith = compareWith;
    }
    private Vector3 compareWith;

    public override int Compare(DockPoint a, DockPoint b)
    {
        float distanceA = Mathf.Round(Vector3.Distance(a.position, compareWith));
        float distanceB = Mathf.Round(Vector3.Distance(b.position, compareWith));

        if(distanceA == distanceB)
        {
            return 0;
        } else if(distanceA > distanceB)
        {
            return 1;
        } else
        {
            return -1;
        }
    }
}
