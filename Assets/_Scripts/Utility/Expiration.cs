using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Expiration : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
       StartCoroutine(DestroyAfter()); 
    }
    public int delayInSecs;

    IEnumerator DestroyAfter()
    {
        yield return new WaitForSeconds(delayInSecs);
        Destroy(this.gameObject);
    }
}
