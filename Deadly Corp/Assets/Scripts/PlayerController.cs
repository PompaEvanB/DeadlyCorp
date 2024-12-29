using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float groundSpeed; // Base Speed of player
    [SerializeField] private float decelerationSpeed; // Base Speed of player
    [SerializeField] private Rigidbody rb; // Player Rigidbody

    // WASD Inputs
    private float XInput; // A and D Inputs
    private float ZInput; // W and S Inputs

    [SerializeField] private float jumpForce; // Force of the players jump
    private bool hasJumped = false; // Lets us know if the jump was successful
    private bool onGround = false; // Lets us know of the players on the ground

    [SerializeField] bool decreasingCoyoteTime = false; // If the player is in coyote time
    [SerializeField] bool hasCoyoteTime = true;// THIS IS THE DEFAULT VALUE OF COYOTETIME
    [SerializeField] float coyoteTime = 0.25f; // Length of coyote time

    [SerializeField] LayerMask groundLayers; // Layer the groundcheck raycast looks for
    [SerializeField] float groundCheckDist; // Length of the raycast

    private void FixedUpdate() // Runs Once per physics tick
    { 
        // Make our Player move, get our input vector so our movements match the cameras position.
        Vector3 movementInputVector = ((XInput * transform.right) + (ZInput * transform.forward)).normalized;
        rb.AddForce(movementInputVector * groundSpeed);

        // If the player is moving while not inputting anything, reduce their speed to zero (except for falling speed) by x amount of time
        if(IsMovingNoInput())
        {
            rb.velocity = Vector3.Lerp(rb.velocity, new Vector3(0,rb.velocity.y,0),decelerationSpeed * Time.deltaTime);
            // FUN IDEA: Make it so it takes longer to reduce speed give how fast you are going.
        }

        // LERP VELOCITY DOWN TO MAX SPEED IF WE EXCEED IT # NEXT TASK
    }

    private void Update() // Runs Once per frame
    { 
        // Get the wasd Inputs
        XInput = Input.GetAxisRaw("Horizontal");
        ZInput = Input.GetAxisRaw("Vertical");

        // If the player tries to jump, do a ground check or check if we are in coyote time.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if(GroundCheck() || decreasingCoyoteTime)
            {
                // Make the player jump, and stop the coyoteTime Coroutine
                Jump();
                StopCoroutine(DecreaseCoyoteTime());
                decreasingCoyoteTime = false;
            }
        }
    }

    private bool IsMovingNoInput() // Returns if The player is moving while not inputting anything
    {
        if(XInput == 0f && ZInput == 0f && rb.velocity.magnitude > 0f){return true;}
        else{return false;}
    }

    private void Jump() // Is called when the player tried and is allowed to jump
    {
        hasJumped = true; // let the engine know we have jumped.
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z); //remove our falling velocity so our jump doesnt have to fight gravity.
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse); // add a force upward
        
    }

    private IEnumerator DecreaseCoyoteTime() // This coroutine decreases jump coyote time and doesnt let the player jump once its done running 
    { 
        if(!decreasingCoyoteTime && hasCoyoteTime) // if we have coyote time and we aren't decreasing it yet
        {
            decreasingCoyoteTime = true; // let the coroutine know we are decreasing coyote time. this makes the coroutine only run when needed.
            yield return new WaitForSeconds(coyoteTime); // wait for the desired amount of coyote time desired.
            hasCoyoteTime = false; // after waiting for our window, let the engine know we missed our window
            decreasingCoyoteTime = false; // let the engine know the coroutine is done.
        }
    }

    private bool GroundCheck() // Check to see if the player is on the ground
    {
        if (Physics.Raycast(gameObject.transform.position, Vector3.down, groundCheckDist, groundLayers)) // if on the ground
        {
            // Stop the coyote time routine, reset the coyote time variables, and let the game know we are grounded
            StopCoroutine(DecreaseCoyoteTime());
            hasCoyoteTime = true;
            decreasingCoyoteTime = false;
            onGround = true;
            return true;
        }
        else // if not on ground
        {
            // Start the coyote time routine and let the game know we are not on the ground
            if(hasCoyoteTime){StartCoroutine(DecreaseCoyoteTime());}
            onGround = false;
            return false;
        }
    }

    private void OnCollisionEnter(Collision other) 
    {
        GroundCheck();
    }
    private void OnCollisionExit(Collision other) 
    {
        GroundCheck();
    }
}
