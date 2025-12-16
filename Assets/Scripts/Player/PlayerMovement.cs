using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Vector3 Velocity;
    private Vector2 playerMovementInput;
    private Vector2 playerMouseInput;
    private float xRotation; // Vertical rotation (pitch)
    private float yRotation; // Horizontal rotation (yaw)

    [SerializeField] private Transform PlayerCamera;
    [SerializeField] private CharacterController Controller;

    [Space]
    [SerializeField] private float Speed;
    [SerializeField] private float Sensitivity;

    void Update()
    {
        playerMovementInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        playerMouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        MovePlayer();
        MovePlayerCamera();
    }

    private void MovePlayer()
    {
        // Get camera's forward and right directions (ignoring Y)
        Vector3 forward = PlayerCamera.transform.forward;
        forward.y = 0;
        forward.Normalize();
        
        Vector3 right = PlayerCamera.transform.right;
        right.y = 0;
        right.Normalize();
        
        // Calculate move direction based on camera orientation
        Vector3 moveVector = right * playerMovementInput.x + forward * playerMovementInput.y;

        if(Input.GetKey(KeyCode.Space))
        {
            Velocity.y = 1f;
        }
        else if(Input.GetKey(KeyCode.LeftShift))
        {
            Velocity.y = -1f;
        }
        else
        {
            Velocity.y = 0f;
        }

        Controller.Move(moveVector * Speed * Time.deltaTime);
        Controller.Move(Velocity * Speed * Time.deltaTime);
    }

    private void MovePlayerCamera()
    {
        if(Input.GetMouseButton(1))
        {
            xRotation -= playerMouseInput.y * Sensitivity;
            yRotation += playerMouseInput.x * Sensitivity;
            
            xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Prevent over-rotation
            
            // Apply both horizontal and vertical rotation to camera only
            PlayerCamera.transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
        }
    }
}