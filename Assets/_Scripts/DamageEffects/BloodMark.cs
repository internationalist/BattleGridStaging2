using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodMark : MonoBehaviour
{
    public enum PartType {chest, limb};

    public PartType type;
    public List<GameObject> marks;
    List<GameObject> instantiatedMarks;
    bool spent = false;

    // Start is called before the first frame update
    void Start()
    {
        instantiatedMarks = new List<GameObject>();
    }

    private void Update()
    {
        if(!spent)
        {
            spent = true;
            //Vector3 markPoint = other.ClosestPoint(transform.position);
            Vector3 markPoint = transform.root.position;
            markPoint = new Vector3(markPoint.x, .09f, markPoint.z);
            int numOfBloodSplatter = 0;
            switch (type)
            {
            case PartType.chest:
                numOfBloodSplatter = Random.Range(1, 2);
                RandomizeBlood(markPoint, numOfBloodSplatter);
                break;
            case PartType.limb:
                numOfBloodSplatter = Random.Range(1, 2);
                RandomizeBlood(markPoint, numOfBloodSplatter);
                break;
            }
            //StartCoroutine(Cleanup());
            //CleanupImmediately();
        }
    }

    private void RandomizeBlood(Vector3 markPoint, int numOfBloodSplatter)
    {
        for (int i = 0; i < numOfBloodSplatter; i++)
        {
            //Random blood mark
            int idx = Random.Range(0, marks.Count);

            //Randomize the rotation
            Quaternion randRotation = Quaternion.Euler(90, Random.Range(0, 361), 0);
            GameObject bloodMark = Instantiate(marks[idx], markPoint, Quaternion.identity);
            bloodMark.transform.rotation *= randRotation;

            //Randomize the scale
            float scaleIncrease = Random.Range(0, 0.3f);
            //bloodMark.transform.localScale += Vector3.one * scaleIncrease;
            instantiatedMarks.Add(bloodMark);
        }
    }

    private IEnumerator Cleanup()
    {
        yield return null;
        foreach (GameObject mark in instantiatedMarks)
        {
            Debug.LogFormat("Cleaning up blood mark {0}", mark.name);
            Destroy(mark, 2f);
        }
        Destroy(this, 2.5f);
    }

    private void CleanupImmediately()
    {
        foreach (GameObject mark in instantiatedMarks)
        {
            Debug.LogFormat("Cleaning up blood mark {0}", mark.name);
            Destroy(mark);
        }
        Destroy(this);
    }
}
