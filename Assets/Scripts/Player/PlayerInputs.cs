using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerInputs : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;

    private float _basicNextFireTime = 0;
        
    public UnityEvent basicAttack = new UnityEvent();
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > _basicNextFireTime)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                basicAttack.Invoke();
                Debug.Log("attack");
                _basicNextFireTime = Time.time + playerStats.playerAttackSpeed;
            }
        }
    }
}
