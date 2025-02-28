using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletHit : MonoBehaviour
{
    [SerializeField] private BulletVar bulletVar;
    void OnTriggerEnter(Collider other)
    {
        //check if target hit isn't a player, if so stop function
        if (other.transform == bulletVar.bulletOwner.transform)
        {
            return;
        }
        
        //destroy bullet
        Destroy(gameObject);
        
        //deal damage to target
        other.GetComponent<PlayerStats>().playerHealth -= bulletVar.bulletDamage;
        Debug.Log(other.GetComponent<PlayerStats>().playerHealth);

        //check if health is 0 or smaller, if so destroy target
        if (other.GetComponent<PlayerStats>().playerHealth <= 0)
        {
            Destroy(other.gameObject);
        }
    }
}
