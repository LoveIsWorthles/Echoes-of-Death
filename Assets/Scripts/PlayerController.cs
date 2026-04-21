using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    // Component references
    private Rigidbody rb;
    private Animator animator;
    private Camera mainCamera; // We need the camera to track the mouse
    private Vector3 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        // Automatically find the Main Camera in the scene
        mainCamera = Camera.main; 
    }

    void Update()
    {
        // 1. Capture Movement Input
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.z = Input.GetAxisRaw("Vertical");
        movement.y = 0f;
        movement = movement.normalized;

        // 2. Update the Animator
        animator.SetFloat("Speed", movement.magnitude);

        // 3. Handle Mouse Aiming
        AimAtMouse();
    }

    void FixedUpdate()
    {
        // 4. Move the physical Rigidbody
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    private void AimAtMouse()
    {
        // Create an invisible mathematical flat plane at the character's height
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0));

        // Draw a line from the camera through the mouse cursor
        Ray cameraRay = mainCamera.ScreenPointToRay(Input.mousePosition);

        // If the line hits our invisible ground plane...
        if (groundPlane.Raycast(cameraRay, out float hitDistance))
        {
            // Find the exact 3D coordinate where the laser hit
            Vector3 targetPoint = cameraRay.GetPoint(hitDistance);

            // Tell the character to look at that exact point
            transform.LookAt(targetPoint);
        }
    }
}