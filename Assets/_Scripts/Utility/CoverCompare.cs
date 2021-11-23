using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverCompare 
{
    private Vector3 compareWith;
    bool desc;

    public CoverCompare(Vector3 compareWith)
    {
        this.compareWith = compareWith;
    }

    public CoverCompare(Vector3 compareWith, bool descendingorder)
    {
        this.compareWith = compareWith;
        this.desc = descendingorder;
    }

    public int Compare(CoverFramework a, CoverFramework b)
    {
        float distanceA = Mathf.Round(Vector3.Distance(a.transform.position, compareWith));
        float distanceB = Mathf.Round(Vector3.Distance(b.transform.position, compareWith));

        if (distanceA == distanceB)
        {
            return 0;
        }
        else if (distanceA > distanceB)
        {
            if(desc)
            {
                return -1;
            } else
            {
                return 1;
            }
            
        }
        else
        {
            if (desc)
            {
                return 1;
            } else
            {
                return -1;
            }
        }
    }
}
