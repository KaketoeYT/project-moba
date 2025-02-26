using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class MoveBullet : MonoBehaviour
{
    [SerializeField] private BulletVar bulletVar;
    
    // Start is called before the first frame update
    void Start()
    {
        //kill bullet after 2 seconds
        Destroy(gameObject, bulletVar.bulletDeathTime);
        
        bulletVar.bulletDirection = (new Vector3(bulletVar.bulletDirection.x,transform.position.y,bulletVar.bulletDirection.z) - transform.position).normalized;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += bulletVar.bulletDirection * bulletVar.bulletSpeed * Time.deltaTime;
    }
}
