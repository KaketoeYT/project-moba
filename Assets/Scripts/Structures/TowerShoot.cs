using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class TowerShoot : MonoBehaviour
{
    [SerializeField] private GameObject Bullet;
    [SerializeField] private BulletVar bulletVar;
    private GameObject _towerTarget;
    private GameObject _currentBullet;

    private void OnTriggerEnter(Collider other)
    {
        _towerTarget = other.gameObject;
        
        TowerAttack();
    }


    public void TowerAttack()
    {
        //spawn bullet and give it the owner and direction
        _currentBullet = Instantiate(Bullet, transform.position, Quaternion.identity);
        _currentBullet.GetComponent<BulletVar>().bulletOwner = gameObject.transform.parent.gameObject;
        _currentBullet.GetComponent<BulletVar>().bulletDirection = _towerTarget.transform.position - transform.position;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        //kill bullet after 2 seconds
        Destroy(_currentBullet.gameObject, bulletVar.bulletDeathTime);
    }

    // Update is called once per frame
    void Update()
    {
        if (_currentBullet == null)
        {
            return;
        }
        
        _currentBullet.transform.position += _towerTarget.transform.position * (bulletVar.bulletSpeed * Time.deltaTime);
        
        //debug.Log(_towerTarget.transform.position);
    }
}
