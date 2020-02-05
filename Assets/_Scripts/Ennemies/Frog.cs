using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Frog : MonoBehaviour
{
    //Frog GameObject
    private Rigidbody2D rb;
    private Animator ani;
    private Collider2D coll;
    private float leftCap;
    private float rightCap;

    //Frog pattern parameters
    [Header("Pattern parameters")]
    [Range(0, 30f)]
    [SerializeField] private float jumpLength = 3;
    [Range(0, 30f)]
    [SerializeField] private float jumpHeight = 10;
    [Range(0, 30f)]
    [SerializeField] private float waitTime = 5f;
    [SerializeField] private LayerMask ground;
    private bool isFacingLeft = true;
    private bool isIdling = false;


    //FSM variables
    private enum State { idle,jumping,falling};
    private State state = State.idle;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        ani = GetComponent<Animator>();
        coll = GetComponent<Collider2D>();
        
        leftCap = this.gameObject.transform.GetChild(0).position.x;
        rightCap = this.gameObject.transform.GetChild(1).position.x;

        //Debug.Log("leftCap :" + leftCap + "righCap : " + rightCap); 
    }

    // Update is called once per frame
    void Update()
    {
        movementPattern();
        stateSwitch(); //Calling the state machine
        ani.SetInteger("state", (int)state); //Setting the animation according to the state
    }

    private void movementPattern()
    {
        if (isFacingLeft)
        {
            if (transform.position.x > leftCap && !isIdling)
            {
                if (coll.IsTouchingLayers(ground))
                {
                    jumpLeft();
                }
            }
            else
            {
                isFacingLeft = false;
                isIdling = true;
                StartCoroutine(wait());
            }
        }
        else
        {
            if (transform.position.x < rightCap && !isIdling)
            {
                if (coll.IsTouchingLayers(ground))
                {
                    jumpRight();
                }
            }
            else
            {
                isFacingLeft = true;
                isIdling = true;
                StartCoroutine(wait());
            }
        }
    }

    private void invertSprite()
    {
        transform.localScale = new Vector3((this.transform.localScale.x*-1), this.transform.localScale.y, this.transform.localScale.z);
    }
    private void jumpLeft()
    {
        rb.velocity = new Vector2(-jumpLength, jumpHeight);
        state = State.jumping;
    }
    private void jumpRight()
    {
        rb.velocity = new Vector2(jumpLength, jumpHeight);
        state = State.jumping;
    }

    IEnumerator wait()
    {
        //yield on a new YieldInstruction that waits for 5 seconds.
        yield return new WaitForSeconds(waitTime);
        isIdling = false;
        invertSprite();
        StopAllCoroutines();  
    }

    //State machine
    private void stateSwitch()
    {
        if (state == State.jumping)
        { //Jump
            if (rb.velocity.y < 0.1f)
            {
                state = State.falling;
            }
        }
        else if (state == State.falling)
        { //Fall after jump
            if (coll.IsTouchingLayers(ground))
            {
                state = State.idle;
            }
        }
        else
        {
                state = State.idle;
        }
    }
 
}
