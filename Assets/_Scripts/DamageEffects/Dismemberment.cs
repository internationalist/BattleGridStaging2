using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dismemberment : MonoBehaviour
{
    public List<GameObject> rigParts;
    public List<GameObject> regularMeshes;
    public List<GameObject> limbs;
    public bool decimate = false;
    public int minParts;
    public int maxParts;
    public GameObject torsoBloodSplatter;
    Animator anim;


    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(decimate)
        {
            foreach(GameObject limb in limbs)
            {
                limb.SetActive(true);
            }
            foreach(GameObject regularMesh in regularMeshes)
            {
                regularMesh.SetActive(false);
            }
            
            decimate = false;
            //Determine number of parts to decimate. This can be between configurable boundaries.
            int tornParts = Random.Range(minParts, maxParts);
            Destroy(rigParts[4].GetComponent<CharacterJoint>());
            int totalParts = rigParts.Count;
            anim.enabled = false;
            torsoBloodSplatter.SetActive(true);
        }
    }
}
