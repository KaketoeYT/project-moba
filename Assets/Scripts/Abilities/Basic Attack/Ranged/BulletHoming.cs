using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletHoming : MonoBehaviour
{
    [SerializeField] private BulletVar bulletVar;
    void OnTriggerEnter(Collider other)
    {
        if (other.transform == bulletVar.bulletOwner.transform)
        {
            return;
        }
        
        bulletVar.bulletDirection = (new Vector3(other.transform.position.x ,transform.position.y ,other.transform.position.z) - transform.position).normalized;
    }
}
