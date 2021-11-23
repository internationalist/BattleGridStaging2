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
            int totalParts = rigParts.Count;
            /*for(int k = 0; k < tornParts; k++)
            {
                int partNum = Random.Range(0, totalParts);
                GameObject rigPart = rigParts[partNum];
                Rigidbody rb = rigPart.GetComponent<Rigidbody>();
                rb.drag = 5f;

                Vector3 dmgPosition = GetWoundPoint(rigPart);
                GameObject marker = Instantiate(dmgMarker, dmgPosition, Quaternion.identity);
                marker.transform.parent = rigPart.transform;
                Destroy(rigPart.GetComponent<CharacterJoint>());
            }*/
            anim.enabled = false;
            torsoBloodSplatter.SetActive(true);
        }
    }

    private static Vector3 GetWoundPoint(GameObject rigPart)
    {
        CapsuleCollider cc = rigPart.GetComponent<CapsuleCollider>();
        Vector3 dmgPosition = Vector3.zero;
        if (cc != null)
        {
            switch (cc.direction)
            {
                case 0:
                    dmgPosition = rigPart.transform.position - rigPart.transform.right * (cc.height / 2);
                    break;
                case 1:
                    dmgPosition = rigPart.transform.position - rigPart.transform.up * (cc.height / 2);
                    break;
                case 2:
                    dmgPosition = rigPart.transform.position - rigPart.transform.forward * (cc.height / 2);
                    break;
            }
        }
        else
        {
            SphereCollider sc = rigPart.GetComponent<SphereCollider>();
            if (sc != null)
            {
                dmgPosition = rigPart.transform.position + rigPart.transform.up * (sc.radius);
            }
            else
            {
                BoxCollider bc = rigPart.GetComponent<BoxCollider>();
                dmgPosition = rigPart.transform.position;
            }

        }

        return dmgPosition;
    }
}
