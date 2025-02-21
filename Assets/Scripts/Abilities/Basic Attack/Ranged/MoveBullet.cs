using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class MoveBullet : MonoBehaviour
{
    public Vector3 targetLocation;
    private Vector3 _targetDirection;
    [SerializeField] private float bulletSpeed;
    
    // Start is called before the first frame update
    void Start()
    {
        _targetDirection = (new Vector3(targetLocation.x,transform.position.y,targetLocation.z) - transform.position).normalized;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += _targetDirection * bulletSpeed * Time.deltaTime;
    }
}
