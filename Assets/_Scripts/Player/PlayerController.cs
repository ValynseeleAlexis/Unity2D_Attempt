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
    [Header("Player parameters")]
    [Range(0, 30f)]
    [SerializeField] private float speed = 5f;
    [Range(0, 30f)]
    [SerializeField] private float runningSpeed = 7f;
    [Range(0, 30f)]
    [SerializeField] private float jumpForce = 10f;
    [Range(0, 30f)]
    [SerializeField] private float hurtForce = 5f;

    [Header("Physics and UI")]
    [SerializeField] public LayerMask ground;
    [SerializeField] public LayerMask walls;
    [SerializeField] public LayerMask death;
    [SerializeField] public Text CoinsNumberBox;
                
    //Scoring
    private int cherries = 0;

    //FSM variables
    private enum State { idle, running, jumping, falling, hurt };
    private State state = State.idle;

    //Health
    [Header("Health System")]
    [SerializeField] public Slider healthSlider;
    [SerializeField] public Image deathScreen;
    
    [Range(0, 30f)]
    [SerializeField] private float healthValue = 10f;
    [Range(0, 30f)]
    [SerializeField] private float hurtDamagevalue = 1f;
    [SerializeField] private bool coinsHealing = true;
    [Range(0, 30f)]
    [SerializeField] private float coinsHealingValue = 1f;
    private float maxHealth;
    private bool dead = false;



    // Start is called before the first frame update
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        ani = GetComponent<Animator>();
        coll = GetComponent<Collider2D>();
        CoinsNumberBox.text = cherries.ToString();
        maxHealth = healthValue;
    }

    // Update is called once per frame
    private void Update()
    {
        if (state != State.hurt && !dead)
        {
            InputManager();
        }
        
        if(!dead){
            stateSwitch(); //Calling the state machine
            ani.SetInteger("state", (int)state); //Setting the animation according to the state
            fallingAgainstWall();
            healthSystem();
            deathSystem();
        }
        if(dead){
            Deathanimation();
        }
    }


    //Controls
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
        if (Input.GetButtonDown("Jump") && coll.IsTouchingLayers(ground) && !coll.IsTouchingLayers(walls))
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            state = State.jumping;
        }
    }

    private void jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        state = State.jumping;
    }

    private void fallingAgainstWall(){
        if(coll.IsTouchingLayers(walls)){
            rb.velocity = new Vector2(0,rb.velocity.y);
        }
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
        else if (state == State.hurt)
        { //Hurt
            if (Mathf.Abs(rb.velocity.x) < 0.1f)
            {
                state = State.idle;
            }
        }
        else if (Mathf.Abs(rb.velocity.x) > 2f) //Running
        {
            if (!coll.IsTouchingLayers(ground))
                state = State.falling;
            else
                state = State.running;
        }
        else
        {//Back to idle
            if (!coll.IsTouchingLayers(ground))
                state = State.falling;
            else
                state = State.idle;
        }
    }



    //Collision and triggers
    private void OnTriggerEnter2D(Collider2D collision)
    { //Automatically called when a collision occur
        if (collision.tag == "Coins")
        {
            Destroy(collision.gameObject);
            cherries++;
            if(coinsHealing)
                healthValue += coinsHealingValue;
            CoinsNumberBox.text = cherries.ToString();
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "Ennemies")
        {
            Frog frog = other.gameObject.GetComponent<Frog>(); //We get the frog object(intialised by Frog script) attached to the ennemy we bumped into  
            
            if (state == State.falling)
            {
                frog.Deathanimation();
                jump();
            }
            else
            {
                healthValue -= hurtDamagevalue; //Update Health
                if (other.gameObject.transform.position.x > transform.position.x)
                { //ennemy to player's right
                    state = State.hurt;
                    rb.velocity = new Vector2(-hurtForce, rb.velocity.y);
                }
                else
                { //ennemy to the left
                    state = State.hurt;
                    rb.velocity = new Vector2(hurtForce, rb.velocity.y);
                }
            }
        }
    }

    //Health and Death 
    private void healthSystem()
    {
        healthSlider.value = healthValue / maxHealth;

       
        if (healthValue > maxHealth){
            healthValue = maxHealth;
        }
    }

    private void deathSystem(){
        //Death
        if (healthValue <= 0 || coll.IsTouchingLayers(death))
        {
            dead = true;
        }
    }
    private void Deathanimation()
    {
        ani.SetBool("Death",true);
    }
    private void Death()
    {
        Destroy(this.gameObject);
        deathScreen.enabled = true;
    }

}
