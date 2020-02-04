using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    //Player Object variables
    private Rigidbody2D rb;
    private Animator ani;
    private Collider2D coll;
  
    //Inspector variables
    [SerializeField] private float speed = 5f;
    [SerializeField] private float runningSpeed = 7f; 
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private LayerMask ground;
    [SerializeField] private int cherries = 0;
    [SerializeField] private Text CoinsNumber;

    //FSM
    private enum State { idle, running, jumping, falling };
    private State state = State.idle;

    // Start is called before the first frame update
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        ani = GetComponent<Animator>();
        coll = GetComponent<Collider2D>();
        CoinsNumber.text = cherries.ToString();
    }

    // Update is called once per frame
    private void Update()
    {
        InputManager();

        stateSwitch(); //Calling the state machine
        ani.SetInteger("state", (int)state); //Setting the animation according to the state
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if(collision.tag == "Coins"){
            Destroy(collision.gameObject);
            cherries ++;
            CoinsNumber.text = cherries.ToString();
        }
    }
    private void InputManager()
    {
        float hDirection = Input.GetAxis("Horizontal");


        //Moving left with horizontal axis
        if (hDirection < 0)
        {

            //Sprint
            if (Input.GetKey(KeyCode.LeftShift))
            {
                rb.velocity = new Vector2(-runningSpeed, rb.velocity.y);
                ani.SetFloat("aniSpeed", 2f);
            }
            //Normal
            else
            {
                rb.velocity = new Vector2(-speed, rb.velocity.y);
                ani.SetFloat("aniSpeed", 1f);
            }
            transform.localScale = new Vector3(-1, 1, 1); //Flipping the sprite
        }

        //Moving right with horizontal axis
        else if (hDirection > 0)
        {

            //Sprint
            if (Input.GetKey(KeyCode.LeftShift))
            {
                rb.velocity = new Vector2(runningSpeed, rb.velocity.y);
                ani.SetFloat("aniSpeed", 2f);
            }
            //Normal
            else
            {
                rb.velocity = new Vector2(speed, rb.velocity.y);
                ani.SetFloat("aniSpeed", 1f);
            }
            transform.localScale = new Vector3(1, 1, 1); //Flipping the sprite
        }

        //Jump mechanism 
        if (Input.GetButtonDown("Jump") && coll.IsTouchingLayers(ground))
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            state = State.jumping;
        }
    }

    //State machine
    private void stateSwitch()
    {
        if(state == State.jumping){
            if(rb.velocity.y < 1f){
                state = State.falling;
            }
        } 
        else if(state == State.falling){
            if(coll.IsTouchingLayers(ground)){
                state = State.idle;
            }
        }
        else if (Mathf.Abs(rb.velocity.x) > 2f){
            state = State.running;
        }
        else{
            state = State.idle;
        }
    }


}
    