using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public abstract class Pickup : MonoBehaviour
{
    private Collider playerInZone;
    private InputAction interactAction;

    private void Start()
    {
        interactAction = InputSystem.actions.FindAction("Interact");
    }

    private void Reset()
    {
        ConfigureTrigger();
    }

    private void OnValidate()
    {
        ConfigureTrigger();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the collider belongs to something that can pick things up (has LoadoutController)
        if (other.GetComponentInParent<LoadoutController>() != null)
        {
            playerInZone = other;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == playerInZone)
        {
            playerInZone = null;
        }
    }

    private void Update()
    {
        if (playerInZone != null && interactAction != null && interactAction.WasPressedThisFrame())
        {
            if (TryPickup(playerInZone))
            {
                Destroy(gameObject);
            }
        }
    }

    protected abstract bool TryPickup(Collider other);

    private void ConfigureTrigger()
    {
        Collider triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
    }
}
