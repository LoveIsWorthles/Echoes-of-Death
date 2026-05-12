using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class CanvasAimReticle : MonoBehaviour
{
    private RectTransform rectTransform;
    private Image image;
    private InputAction pointAction;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        
        // Ensure the reticle doesn't block UI clicks (very important!)
        if (image != null)
        {
            image.raycastTarget = false;
        }

        // Use the "Point" action from your Input System
        pointAction = InputSystem.actions.FindAction("Point");
    }

    void Update()
    {
        // Hide the reticle whenever the OS cursor is visible (pause / game over / win)
        bool hideReticle = GameManager.Instance != null
            && (GameManager.Instance.isPaused || GameManager.Instance.isGameOver);

        if (hideReticle)
        {
            image.enabled = false;
            return;
        }

        if (pointAction != null)
        {
            image.enabled = true;
            // Moves the UI element directly to the mouse screen position
            rectTransform.position = pointAction.ReadValue<Vector2>();
        }
    }
}