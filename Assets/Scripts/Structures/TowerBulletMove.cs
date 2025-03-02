using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerBulletMove : MonoBehaviour
{
    [SerializeField] private BulletVar bulletVar;
    // Start is called before the first frame update
    void Start()
    {
        //kill bullet after 2 seconds
        Destroy(gameObject, bulletVar.bulletDeathTime);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += GetComponent<TowerShoot>().towerBulletDirection * (bulletVar.bulletSpeed * Time.deltaTime);
    }
}
