using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ApplicableDamage
{
    public int damageAmt;
    public bool critical;
    public CoverMeta cm;

    public ApplicableDamage(int damageAmt, bool critical, CoverMeta cm)
    {
        this.damageAmt = damageAmt;
        this.critical = critical;
        this.cm = cm;
    }
}
