using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class MouseLook : NetworkBehaviour
{
    public Transform player; // Reference to the player object
    public float sensitivity = 5.0f; // Mouse sensitivity
    public float maxYAngle = 80.0f; // Maximum vertical angle

    private float rotationY = 0.0f;
    [SerializeField] private PlayerInputActions playerControls;
    [SerializeField] private InputAction look;
    [SerializeField] bool invert;
    [SerializeField] FaceCamera[] faceCameras;

    private void Awake()
    {
        playerControls = new PlayerInputActions();
    }

    private void OnEnable()
    {

        if (Gamepad.all.Count >0)
        {
            sensitivity = 0.1f;
            invert = true;
            Debug.Log("Setting sensativity to " + sensitivity);
        }
        
        look = playerControls.Player.Look;
        look.Enable();
    }

    public override void OnNetworkSpawn()
    {
        for (int i = 0; i < faceCameras.Length; i++)
        {
            faceCameras[i].CameraToFace(GetComponent<Camera>());
        }
    }

    private void OnDisable()
    {
        look.Disable();
    }
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // Lock cursor to the center of the screen
        Cursor.visible = false; // Make cursor invisible
    }

    void Update()
    {

        if (IsOwner)
            MoveCamera();

    }

    void MoveCamera()
    {
        // Get mouse input

        float mouseX = look.ReadValue<Vector2>().x; 
        float mouseY = look.ReadValue<Vector2>().y;

        // Rotate the player horizontally based on mouse movement
        player.Rotate(0, mouseX * sensitivity, 0);

        // Rotate the camera vertically based on mouse movement
        if(!invert)
        rotationY += mouseY * sensitivity;
        else
        rotationY += -mouseY * sensitivity;

        rotationY = Mathf.Clamp(rotationY, -maxYAngle, maxYAngle); // Clamp vertical rotation
        transform.localEulerAngles = new Vector3(-rotationY, transform.localEulerAngles.y, 0);
    }

}
