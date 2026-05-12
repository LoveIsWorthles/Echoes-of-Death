using UnityEngine;

public class Hostage : MonoBehaviour
{
    private bool isSaved = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isSaved) return;

        if (other.CompareTag("Player"))
        {
            isSaved = true;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.HostageSaved();
            }
            
            // For now, we just deactivate the hostage to signal they've been saved.
            // In a more complex game, they might follow the player or run to safety.
            gameObject.SetActive(false);
        }
    }
}
