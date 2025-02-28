using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletSpawn : MonoBehaviour
{
    [SerializeField] private GameObject Bullet;
    private GameObject _currentBullet;
    private Camera _camera;
    
    public void RangedAttack()
    {
        //spawn bullet and give it the owner and direction
        _currentBullet = Instantiate(Bullet, transform.position, Quaternion.identity);
        _currentBullet.GetComponent<BulletVar>().bulletOwner = gameObject;
        _currentBullet.GetComponent<BulletVar>().bulletDirection = _camera.GetComponent<WorldMousePos>().Hit;
    }

    //assign the main camera to _camera var
    private void Awake()
    {
        _camera = Camera.main;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
