using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAimation : MonoBehaviour
{

    [SerializeField]
    PlayerMovement playerMovement;
    [SerializeField]
    Rigidbody2D rb;
    [SerializeField]
    Animator animator;


    Vector3 last;

    private void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        //float speed = Mathf.Abs(player.position.x - last.x);
        float speed = Mathf.Abs(rb.velocity.x);

        animator.SetBool("jumping", playerMovement.IsJumping());
        animator.SetBool("falling", playerMovement.IsFalling());
        if (speed > 0.001f)
        {
            animator.SetFloat("speed", speed);
        } else
        {
            animator.SetFloat("speed", 0);
        }

        

    }
}
