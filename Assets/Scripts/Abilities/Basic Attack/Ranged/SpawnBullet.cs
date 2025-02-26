using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnBullet : MonoBehaviour
{
    [SerializeField] private GameObject Bullet;
    private GameObject _currentBullet;
    private Camera _camera;
    
    public void RangedAttack()
    {
        _currentBullet = Instantiate(Bullet, transform.position, Quaternion.identity);
        _currentBullet.GetComponent<BulletVar>().bulletOwner = gameObject;
        _currentBullet.GetComponent<BulletVar>().bulletDirection = _camera.GetComponent<WorldMousePos>().Hit;
    }

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
