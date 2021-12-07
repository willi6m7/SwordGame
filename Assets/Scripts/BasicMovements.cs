using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class BasicMovements : NetworkBehaviour
{
    public bool isLeft;
    public bool isRight;
    public float movementSpeed = 5f;
    public float rotateSpeed = 25f;
    public Transform camT;
    CharacterController mpCharController;
    private SpriteRenderer renderer;


    void Start()
    {
        mpCharController = GetComponent<CharacterController>();
        renderer = GetComponent<SpriteRenderer>();
        isRight = true;
        if (renderer == null)
        {
            Debug.Log("Something went wrong with the player's sprite.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        MPMovePlayer();
    }
    void MPMovePlayer()
    {

        Vector3 moveVect = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        Vector3 characterScale = transform.localScale;
        mpCharController.SimpleMove(moveVect * movementSpeed);

        //while the Flip function flips the sprite properly, it does not flip the attack point.
        //flips the entire model as it is technically 3d, not 2d.
        //The issue with this is that it flips the camera as well if it's included in the prefab.
        if (Input.GetAxisRaw("Horizontal") < 0)
        {
            if (!isLeft)
            {
                isLeft = true;
                isRight = false;
                transform.Rotate(0f, -180f, 0f);
            }
            
        }
        else if(Input.GetAxisRaw("Horizontal") > 0)
        {
            if (!isRight)
            {
                isRight = true;
                isLeft = false;
                transform.Rotate(0f, 180f, 0f);
            }
            
        }
    }
}
