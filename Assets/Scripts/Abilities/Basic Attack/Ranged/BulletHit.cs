using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletHit : MonoBehaviour
{
    [SerializeField] private BulletVar bulletVar;
    void OnTriggerEnter(Collider other)
    {
        if (other.transform == bulletVar.bulletOwner.transform)
        {
            return;
        }
        
        Destroy(gameObject);
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
