using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class FreeCameraMovement : MonoBehaviour
{
    [SerializeField] private float cameraSpeed;
    [SerializeField] private float cameraEdgeDistance;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //move camera up
        if (Screen.height * (1 - cameraEdgeDistance) < Input.mousePosition.y)
        {
            transform.position += new Vector3(0,0,1) * Time.deltaTime * cameraSpeed;
        }
        
        //move camera down
        if (Screen.height * cameraEdgeDistance > Input.mousePosition.y)
        {
            transform.position += new Vector3(0,0,-1) * Time.deltaTime * cameraSpeed;
        }
        
        //move camera left
        if (Screen.width * cameraEdgeDistance > Input.mousePosition.x)
        {
            transform.position += new Vector3(-1,0,0) * Time.deltaTime * cameraSpeed;
        }
        
        //move camera right
        if (Screen.width * (1 - cameraEdgeDistance) < Input.mousePosition.x)
        {
            transform.position += new Vector3(1,0,0) * Time.deltaTime * cameraSpeed;
        }
    }
}
