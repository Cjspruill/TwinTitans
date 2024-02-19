using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    [SerializeField] Camera camera;
    // Start is called before the first frame update
    void Start()
    {
    
    }

    // Update is called once per frame
    void Update()
    {
        if(camera !=null)
        transform.LookAt(camera.transform);
    }

    public void CameraToFace(Camera gameObject)
    {
        camera = gameObject;
    }
}
