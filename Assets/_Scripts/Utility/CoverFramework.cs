using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class CoverFramework : MonoBehaviour
{
    public GameObject shelterMarkerPrefab;
    public GameObject cornerMarkerPrefab;
    public Collider coverCollider;
    public List<DockPoint> coverDockPoints;
    public List<Vector3> cornerPoints;
    public List<DockPoint> flankPoints;
    public List<ParticleSystem> ricochetEffects;

    [Range(0, 1)]
    public float damageReductionPct;
    public enum TYPE {none, full, half};

    public TYPE coverType;

    public MeshRenderer renderer;

    public float flankOffset = 3f;

    public AudioSource auSrc;

    public Ricochet ricochet;

    // Start is called before the first frame update
    void Start()
    {
        coverCollider = GetComponent<Collider>();
        Vector3 size = coverCollider.bounds.size;
        Vector3 center = coverCollider.bounds.center;
        CalculateCoverDocks(size, center);
        auSrc = GetComponent<AudioSource>();
    }

    private void CalculateCoverDocks(Vector3 size, Vector3 center)
    {
        float maxX = size.x + .5f;
        float maxZ = size.z + .5f;
        float minX = -.5f, minZ = -.5f;

        int xInvert = 1, zInvert = 1;

        if (center.x < transform.position.x)
        {
            xInvert = -1;
        }

        if (center.z < transform.position.z)
        {
            zInvert = -1;
        }

        float xIdx = minX + 1, zIdx = minZ + 1;

        CalculateCorners(maxX, maxZ, minX, minZ, xInvert, zInvert);

        bool foundXAxisPoints = false;
        bool foundZAxisPoints = false;

        while (xIdx <= maxX - 1)
        {
            foundXAxisPoints = true;
            Vector3 pos = new Vector3(transform.position.x + xIdx * xInvert, transform.position.y, transform.position.z + maxZ * zInvert);
            if(GeneralUtils.InsideNavMesh(pos, GameManager.universalAgent))
            {
                #if MARKER
                Instantiate(shelterMarkerPrefab, pos + Vector3.up * .1f, Quaternion.identity);
                #endif
                coverDockPoints.Add(DockPoint.Instance(pos));
            }


            //calculate flank
            Vector3 flankPos = new Vector3(transform.position.x + xIdx * xInvert, transform.position.y, transform.position.z + (maxZ + flankOffset) * zInvert);
            if (GeneralUtils.InsideNavMesh(flankPos, GameManager.universalAgent))
            {
                #if MARKER
                Instantiate(cornerMarkerPrefab, flankPos + Vector3.up * .1f, Quaternion.identity);
                #endif
                flankPoints.Add(DockPoint.Instance(flankPos));
            }

            pos = new Vector3(transform.position.x + xIdx * xInvert, transform.position.y, transform.position.z + minZ * zInvert);
            if (GeneralUtils.InsideNavMesh(pos, GameManager.universalAgent))
            {
                #if MARKER
                Instantiate(shelterMarkerPrefab, pos + Vector3.up * .1f, Quaternion.identity);
                #endif
                coverDockPoints.Add(DockPoint.Instance(pos));
            }

            flankPos = new Vector3(transform.position.x + xIdx * xInvert, transform.position.y, transform.position.z + (minZ - flankOffset) * zInvert);
            if (GeneralUtils.InsideNavMesh(flankPos, GameManager.universalAgent))
            {
                #if MARKER
                Instantiate(cornerMarkerPrefab, flankPos + Vector3.up * .1f, Quaternion.identity);
                #endif
                flankPoints.Add(DockPoint.Instance(flankPos));
            }
            ++xIdx;
        }

        while (zIdx <= maxZ - 1)
        {
            foundZAxisPoints = true;
            Vector3 pos = new Vector3(transform.position.x + maxX * xInvert, transform.position.y, transform.position.z + zIdx * zInvert);
            if (GeneralUtils.InsideNavMesh(pos, GameManager.universalAgent)) {
                #if MARKER
                Instantiate(shelterMarkerPrefab, pos + Vector3.up * .1f, Quaternion.identity);
                #endif
                coverDockPoints.Add(DockPoint.Instance(pos));
            }

            Vector3 flankPos = new Vector3(transform.position.x + (maxX + flankOffset) * xInvert, transform.position.y, transform.position.z + zIdx * zInvert);
            if (GeneralUtils.InsideNavMesh(flankPos, GameManager.universalAgent))
            {
                #if MARKER
                Instantiate(cornerMarkerPrefab, flankPos + Vector3.up * .1f, Quaternion.identity);
                #endif
                flankPoints.Add(DockPoint.Instance(flankPos));
            }


            pos = new Vector3(transform.position.x + minX * xInvert, transform.position.y, transform.position.z + zIdx * zInvert);
            if (GeneralUtils.InsideNavMesh(pos, GameManager.universalAgent))
            {
                #if MARKER
                Instantiate(shelterMarkerPrefab, pos + Vector3.up * .1f, Quaternion.identity);
                coverDockPoints.Add(DockPoint.Instance(pos));
                #endif
            }

            flankPos = new Vector3(transform.position.x + (minX - flankOffset) * xInvert, transform.position.y, transform.position.z + zIdx * zInvert);
            if (GeneralUtils.InsideNavMesh(flankPos, GameManager.universalAgent))
            {
                #if MARKER
                Instantiate(cornerMarkerPrefab, flankPos + Vector3.up * .1f, Quaternion.identity);
                #endif
                flankPoints.Add(DockPoint.Instance(flankPos));
            }

            ++zIdx;
        }
        if(!foundZAxisPoints) //Edge case when the cover is too narrow in a certain axis then generate one cover and flank point in the middle
        {
            Vector3 pos = new Vector3(transform.position.x + minX * xInvert, transform.position.y, center.z);
            if (GeneralUtils.InsideNavMesh(pos, GameManager.universalAgent))
            {
                #if MARKER
                Instantiate(shelterMarkerPrefab, pos + Vector3.up * .1f, Quaternion.identity);
                #endif
                coverDockPoints.Add(DockPoint.Instance(pos));
            }

            pos = new Vector3(transform.position.x + maxX * xInvert, transform.position.y, center.z);
            if (GeneralUtils.InsideNavMesh(pos, GameManager.universalAgent))
            {
                #if MARKER
                Instantiate(shelterMarkerPrefab, pos + Vector3.up * .1f, Quaternion.identity);
                #endif
                coverDockPoints.Add(DockPoint.Instance(pos));
            }
        }

        if (!foundXAxisPoints) //Edge case when the cover is too narrow in a certain axis then generate one cover and flank point in the middle
        {
            Vector3 pos = new Vector3(center.x, transform.position.y, transform.position.z + maxZ * zInvert);
            if (GeneralUtils.InsideNavMesh(pos, GameManager.universalAgent))
            {
                #if MARKER
                Instantiate(shelterMarkerPrefab, pos + Vector3.up * .1f, Quaternion.identity);
                #endif
                coverDockPoints.Add(DockPoint.Instance(pos));
            }

            pos = new Vector3(center.x, transform.position.y, transform.position.z + minZ * zInvert);
            if (GeneralUtils.InsideNavMesh(pos, GameManager.universalAgent))
            {
                #if MARKER
                Instantiate(shelterMarkerPrefab, pos + Vector3.up * .1f, Quaternion.identity);
                #endif
                coverDockPoints.Add(DockPoint.Instance(pos));
            }
        }
    }

    private void CalculateCorners(float maxX, float maxZ, float minX, float minZ, int xInvert, int zInvert)
    {
        CalculateCornerBody(minX, minZ, xInvert, zInvert);

        CalculateCornerBody(minX, maxZ, xInvert, zInvert);

        CalculateCornerBody(maxX, minZ, xInvert, zInvert);

        CalculateCornerBody(maxX, maxZ, xInvert, zInvert);
    }

    private void CalculateCornerBody(float xPos, float zPos, int xInvert, int zInvert)
    {
        Vector3 corner1 = new Vector3(transform.position.x + xPos * xInvert, transform.position.y, transform.position.z + zPos * zInvert);
        #if MARKER
        Instantiate(cornerMarkerPrefab, corner1 + Vector3.up * .1f, Quaternion.identity);
        #endif
        cornerPoints.Add(corner1);
        flankPoints.Add(DockPoint.Instance(corner1));
    }

    public void PlayRicochet()
    {
        StartCoroutine(AudioManager.PlayRicochet(ricochet.leading, ricochet.trailing, auSrc));
    }
}
