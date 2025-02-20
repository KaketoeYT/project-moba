using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GameObject TargetPlayer;
    [SerializeField] private GameObject TargetFree;
    private bool CameraState;

    void setParent(GameObject newParent)
    {
        transform.parent = newParent.transform;
        transform.rotation = newParent.transform.rotation;
        transform.position = newParent.transform.position;
    }


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Space))
        {
            return;
        }

        CameraState = !CameraState;

        if (CameraState == true)
        {
            setParent(TargetFree);
            TargetFree.transform.position = TargetPlayer.transform.position;
        }
        else
        {
            setParent(TargetPlayer);
        }
    }
}
