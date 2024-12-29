using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [SerializeField] private float mouseSensitivity = 100f;

    public Transform playerBody;

    float xRotation = 0f; // xRotation of the Main Camera (up and down) 
    
    void Start() // Start is called before the first frame update
    {
        // Lock the cursor to the middle of the screen
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update() // Update is called once per frame
    {
        // Get Mouse Inputs
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Move the camera vertically, clamp it to 90 deg up and down, then only rotate the camera when looking up/
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Rotate the player left and right when the camera moves left or right
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
