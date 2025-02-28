using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;

    public CharacterController CharacterController;
    private float _moveX, _moveZ;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _moveX = Input.GetAxis("Horizontal");
        _moveZ = Input.GetAxis("Vertical");

        CharacterController.Move(new Vector3(_moveX,0,_moveZ)*playerStats.playerMoveSpeed*Time.deltaTime);
    }
}
