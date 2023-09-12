using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] GameObject bullet;
    [SerializeField] Transform gun;

    float runSpeed = 5f;
    float jumpSpeed = 11f;
    float climbSpeed = 5f;

    bool playerHasHorizontalSpeed;
    bool isAlive = true;

    Vector2 moveInput;
    Rigidbody2D myRigidbody;
    Animator myAnimator;
    CapsuleCollider2D myBodyCollider;
    BoxCollider2D myFeetCollider;
    float gravityScaleAtStart;

    void Start()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        myBodyCollider = GetComponent<CapsuleCollider2D>();
        myFeetCollider = GetComponent<BoxCollider2D>();
        gravityScaleAtStart = myRigidbody.gravityScale;
    }

    void Update()
    {
        if(!isAlive) { return; }
        Run();
        FlipSprite();
        ClimbLadder();
        Die();
    }

    void OnFire(InputValue value)
    {
        if(!isAlive) { return; }
        if(value.isPressed)
            Instantiate(bullet, gun.position, transform.rotation);
    }

    void OnMove(InputValue value)
    {
        if(!isAlive) { return; }
        moveInput = value.Get<Vector2>();
        //Debug.Log(moveInput);
    }

    void OnJump(InputValue value)
    {
        if(!isAlive) { return; }
        if(!myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Ground"))) {return;}
        if(value.isPressed)
        {
            myRigidbody.velocity += new Vector2(0f, jumpSpeed);
        }
    }

    void Run()
    {
        Vector2 playerVelocity = new Vector2(runSpeed*moveInput.x, myRigidbody.velocity.y);
        myRigidbody.velocity = playerVelocity;
        myAnimator.SetBool("isRunning", playerHasHorizontalSpeed);
    }

    void ClimbLadder()
    {
        
        myAnimator.speed = 1;
        if(!myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Ladders")))
        {
            myRigidbody.gravityScale = gravityScaleAtStart;
            myAnimator.SetBool("isClimbing", false);
            return;
        }
        
        myAnimator.SetBool("isClimbing", true);
        Vector2 climbVelocity = new Vector2(myRigidbody.velocity.x, moveInput.y*climbSpeed);
        myRigidbody.velocity = climbVelocity;
        myRigidbody.gravityScale = 0f;

        bool playerHasVerticalSpeed = Mathf.Abs(myRigidbody.velocity.y) > Mathf.Epsilon;
        if(!playerHasVerticalSpeed && myAnimator.GetCurrentAnimatorStateInfo(0).IsName("Climbing"))
            myAnimator.speed = 0;
        
    }

    void FlipSprite()
    {
        playerHasHorizontalSpeed = Mathf.Abs(myRigidbody.velocity.x) > Mathf.Epsilon;

        if(playerHasHorizontalSpeed)
            transform.localScale = new Vector2(Mathf.Sign(myRigidbody.velocity.x), 1f);
    }

    void Die()
    {
        if(!myBodyCollider.IsTouchingLayers(LayerMask.GetMask("Enemies", "Hazards"))) {return;}
        
        isAlive = false;
        myAnimator.SetTrigger("Dying");
        myRigidbody.velocity = new Vector2(-runSpeed*transform.localScale.x, jumpSpeed);
        FindObjectOfType<GameSession>().ProcessPlayerDeath();
    }
}
