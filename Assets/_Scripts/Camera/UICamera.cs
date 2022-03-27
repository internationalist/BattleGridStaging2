using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICamera : MonoBehaviour
{
    private GameObject previewSubject;

    public float zOffset;
    public float yOffset;
    public GameObject defaultPreview;

    public List<IDModelMap> modelMapList;
    // Start is called before the first frame update
    void Start()
    {
        GameManager.OnSelected += OnSelect;
        previewSubject = defaultPreview;
    }

    private void OnDestroy()
    {
        GameManager.OnSelected -= OnSelect;
    }



    // Update is called once per frame
    void Update()
    {
        if (previewSubject != null)
        {
            Vector3 position = previewSubject.transform.position;
            position += Vector3.up * yOffset;
            position += Vector3.forward * zOffset;
            transform.position = position;
        }
    }

    public void OnSelect(PlayerController player)
    {
        previewSubject.SetActive(false);
        if (player != null)
        {
            previewSubject = getObjectByID(player.ID);
            if (previewSubject != null)
            {
                previewSubject.SetActive(true);
            } else
            {
                previewSubject = defaultPreview;
                //previewSubject.SetActive(true);
            }
        } else
        {
            previewSubject = defaultPreview;
        }
    }
    GameObject getObjectByID(string objectID)
    {
        foreach(IDModelMap modelMap in modelMapList)
        {
            if(modelMap.objectID.Equals(objectID))
            {
                return modelMap.model;
            }
        }
        return null;
    }
    [System.Serializable]
    public class IDModelMap {
        public int objectID;
        public GameObject model;
    }
}
