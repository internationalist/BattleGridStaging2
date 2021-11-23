using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletTrail : MonoBehaviour
{
    public float speed;
    public Vector3 end;
    public bool activate;
    private float journeyLength;
    float startTime;
    // Start is called before the first frame update
    void Start()
    {
        startTime = Time.time;
        // Calculate the journey length.
        journeyLength = Vector3.Distance(transform.position, end);
    }

    // Update is called once per frame
    void Update()
    {
        if(activate)
        {
            float distCovered = (Time.time - startTime) * speed;

            // Fraction of journey completed equals current distance divided by total distance.
            float fractionOfJourney = distCovered / journeyLength;

            // Set our position as a fraction of the distance between the markers.
            transform.position = Vector3.Lerp(transform.position, end, fractionOfJourney);

            if(fractionOfJourney >= 1)
            {
                GameObject.Destroy(this.gameObject);
            } else if(fractionOfJourney > .05f)
            {
                transform.parent = null;
            }
        }
    }
}
