using UnityEngine;

public class StairTrigger : MonoBehaviour
{
    public FloorManager floorManager;
    public bool isGoingUpToFloor2 = true; // Check this box if this trigger moves you UP

    private void OnTriggerEnter(Collider other)
    {
        // Make sure your player character has the tag "Player" at the top of their Inspector!
        if (other.CompareTag("Player"))
        {
            if (isGoingUpToFloor2)
            {
                floorManager.GoToFloor2();
            }
            else
            {
                floorManager.GoToFloor1();
            }
        }
    }
}