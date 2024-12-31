using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Animations;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
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

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            Destroy(GetComponentInChildren<Camera>().gameObject); // ensures there is only one camera in the scene for each client
        }
    }

    private void FixedUpdate() // Runs Once per physics tick
    { 
        if (IsOwner) // ensures that this code is only executed on the client game object by the client
        {
            Move(); // Move character

            MaxVelocity(); // Clamp Velocity to max speed
        }
    }

    private void Update() // Runs Once per frame
    { 
        if (IsOwner) // ensures that this code is only executed on the client game object by the client
        {
            GetInputs(); // Get keyboard Inputs

            if (Input.GetKeyDown(KeyCode.Space)){if(GroundCheck() || decreasingCoyoteTime){Jump();}} // Let player Jump if on ground or in coyote time

            GroundDecellerate(); // Decellerate player when on ground

            SendNetworkInfo();
        }
        else
        {
            GetNetworkInfo();
        }
    }

    private NetworkVariable<Vector3> networkPosition = new(writePerm: NetworkVariableWritePermission.Owner);
    private NetworkVariable<Quaternion> networkRotation = new(writePerm: NetworkVariableWritePermission.Owner);

    private void SendNetworkInfo()
    {
        networkPosition.Value = transform.position;
        networkRotation.Value = transform.rotation;
    }

    private void GetNetworkInfo()
    {
        transform.position = networkPosition.Value;
        transform.rotation = networkRotation.Value;
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
        rb.AddForce(movementInputVector * groundSpeed, ForceMode.Force);
    }
    private void MaxVelocity() // Clamp the velocity to our max speed and negative max speed
    {
        Vector3 flatVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z); // making sure this upcoming chunk of code only affects horizontal movement, no impact on vertical velocity
        if (flatVelocity.magnitude > maxGroundSpeed) // checking to see if we're moving too fast on the ground
        {
            Vector3 cappedVelocity = flatVelocity.normalized * maxGroundSpeed; // get our current movement DIRECTION (normalized velocity) and set its magnitude to our max ground speed
            rb.velocity = new Vector3(cappedVelocity.x, rb.velocity.y, cappedVelocity.z); // set the velocity of the player to this new capped velocity
        }
    }

    private void GroundDecellerate() // Add drag for player when on ground, has them deccelerate when not moving
    {
        if(onGround){rb.drag = decelerationSpeed;}
        else{rb.drag = 0;}
    }

    private void Jump() // Is called when the player tried and is allowed to jump
    {
        hasJumped = true; // let the engine know we have jumped.
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z); //remove our falling velocity so our jump doesnt have to fight gravity.
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse); // add a force upward
        StopCoroutine(DecreaseCoyoteTime());
        decreasingCoyoteTime = false;
        onGround = false;
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
