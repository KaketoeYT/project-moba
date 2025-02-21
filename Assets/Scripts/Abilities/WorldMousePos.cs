using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMousePos : MonoBehaviour
{
    public Vector3 target;
    public Vector3 Hit;
    [SerializeField] private Camera cam;
    [SerializeField] private GameObject Sphere;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        target = cam.ScreenToWorldPoint(Input.mousePosition);
        Physics.Raycast(target, transform.forward, out RaycastHit hit);
        Hit = hit.point;
        Sphere.transform.position = hit.point;
    }
}
