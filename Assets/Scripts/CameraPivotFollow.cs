using UnityEngine;

public class CameraPivotFollow : MonoBehaviour
{
    [Header("Tracking")]
    // You will drag your Player GameObject into this slot in the Inspector
    public Transform playerTarget; 
    
    [Header("Dynamics")]
    // Controls how "snappy" the camera is. 
    // Lower number = looser, smoother follow. Higher number = tighter lock.
    public float smoothSpeed = 10f; 

    void LateUpdate()
    {
        // Don't do anything if the player hasn't been assigned or is destroyed
        if (playerTarget == null) return;

        // The exact position the pivot needs to go to
        Vector3 desiredPosition = playerTarget.position;

        // Vector3.Lerp smoothly glides the pivot from its current spot to the player's spot
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // Apply the new smoothly calculated position to the Camera Pivot
        transform.position = smoothedPosition;
    }
}