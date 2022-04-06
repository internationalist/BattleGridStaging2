using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dismemberment : MonoBehaviour
{
    public List<GameObject> rigParts;
    public List<GameObject> regularMeshes;
    public List<GameObject> limbs;
    public bool decimate = false;
    public GameObject torsoBloodSplatter;
    Animator anim;
    List<int> tornPartsIdxs;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        tornPartsIdxs = new List<int>();
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
            int tornParts = Random.Range(1, rigParts.Count + 1);
            for(int i = 0; i < tornParts; i++)
            {
                int tornPartIdx = Random.Range(0, rigParts.Count);
                while (tornPartsIdxs.Contains(tornPartIdx))
                {
                    tornPartIdx = Random.Range(0, rigParts.Count);
                }
                tornPartsIdxs.Add(tornPartIdx);
                ParticleSystem blood = rigParts[tornPartIdx].GetComponentInChildren<ParticleSystem>();
                if(blood != null)
                {
                    blood.Play();
                }
                Destroy(rigParts[tornPartIdx].GetComponent<CharacterJoint>());
            }
            /*Destroy(rigParts[1].GetComponent<CharacterJoint>());
            Destroy(rigParts[3].GetComponent<CharacterJoint>());
            Destroy(rigParts[0].GetComponent<CharacterJoint>());*/
            anim.enabled = false;
            torsoBloodSplatter.SetActive(true);
        }
    }
}
