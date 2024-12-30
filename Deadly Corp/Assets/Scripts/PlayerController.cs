using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float groundSpeed; // Base Speed of player
    [SerializeField] private float maxGroundSpeed; // Max Base Speed of player
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
        Move(); // Move character

        //IsMovingNoInput(); // Deccelerate player if there is no input

        MaxVelocity(); // Clamp Velocity to max speed
    }

    private void Update() // Runs Once per frame
    { 
        GetInputs(); // Get keyboard Inputs

        if (Input.GetKeyDown(KeyCode.Space)){if(GroundCheck() || decreasingCoyoteTime){Jump();}} // Let player Jump if on ground or in coyote time
    }
    
    private void GetInputs() // Gets any inputs we need from the keyboard
    {
        // Get the wasd Inputs
        XInput = Input.GetAxisRaw("Horizontal");
        ZInput = Input.GetAxisRaw("Vertical");
    }
    private void Move() // Makes character move by adding force
    {
        Vector3 movementInputVector = ((XInput * transform.right) + (ZInput * transform.forward)).normalized; //get our input vector so our movements match the cameras position.
        rb.AddForce(movementInputVector * groundSpeed);
    }
    private void MaxVelocity() // Clamp the velocity to our max speed and negative max speed
    {
        // //-----------------------
        // // Trying to make it so you dont move faster when moving diagonally
        // int diagonalMovementScaling;
        // float scaledMaxGroundSpeed;
        // if(XInput != 0 && ZInput != 0)
        // {
        //     diagonalMovementScaling = 1; // tried this at value '2' but still felt weird
        // }
        // else
        // {
        //     diagonalMovementScaling = 1;
        // }
        // scaledMaxGroundSpeed = maxGroundSpeed / diagonalMovementScaling;
        // //-----------------------

        // // Clamp Velocity
        // rb.velocity = new Vector3(Mathf.Clamp(rb.velocity.x, -scaledMaxGroundSpeed, scaledMaxGroundSpeed),rb.velocity.y,Mathf.Clamp(rb.velocity.z, -scaledMaxGroundSpeed, scaledMaxGroundSpeed));

        Vector3 flatVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z); // making sure this upcoming chunk of code only affects horizontal movement, no impact on vertical velocity
        if (flatVelocity.magnitude > maxGroundSpeed) // checking to see if we're moving too fast on the ground
        {
            Vector3 cappedVelocity = flatVelocity.normalized * maxGroundSpeed; // get our current movement DIRECTION (normalized velocity) and set its magnitude to our max ground speed
            rb.velocity = new Vector3(cappedVelocity.x, rb.velocity.y, cappedVelocity.z); // set the velocity of the player to this new capped velocity
        }
    }

    private void IsMovingNoInput() // If we are moving while not inputting anything, reduce our speed on that specific axis to zero
    {
        if(XInput == 0f && rb.velocity.x != 0f || ZInput == 0f && rb.velocity.z != 0f)
        {
            rb.velocity = Vector3.Lerp(rb.velocity, new Vector3(rb.velocity.x * Mathf.Abs(XInput),rb.velocity.y,rb.velocity.z * Mathf.Abs(ZInput)),decelerationSpeed * Time.deltaTime);
        }
    }

    private void Jump() // Is called when the player tried and is allowed to jump
    {
        hasJumped = true; // let the engine know we have jumped.
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z); //remove our falling velocity so our jump doesnt have to fight gravity.
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse); // add a force upward
        StopCoroutine(DecreaseCoyoteTime());
        decreasingCoyoteTime = false;
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
