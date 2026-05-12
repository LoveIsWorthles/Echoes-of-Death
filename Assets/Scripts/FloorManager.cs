using UnityEngine;

public class FloorManager : MonoBehaviour
{
    [Header("Floor Groups")]
    public GameObject floor1Group;
    public GameObject floor2Group;

    [Header("Fog Of War")]
    public Transform fogPlane;
    public float floor1FogHeight = 2f;
    public float floor2FogHeight = 6f; // Adjust this based on how tall your 1st floor walls are

    void Awake()
    {
        RefreshSceneReferences();
        GoToFloor1();
    }

    public void RefreshSceneReferences()
    {
        // If references are lost (e.g. after scene reload in a persistent manager), try to find them by name
        if (floor1Group == null) floor1Group = GameObject.Find("Floor1_Group");
        if (floor2Group == null) floor2Group = GameObject.Find("Floor2_Group");
        if (fogPlane == null)
        {
            GameObject fog = GameObject.Find("Fog_Plane"); // Guessing name based on context
            if (fog != null) fogPlane = fog.transform;
        }
    }

    public void GoToFloor2()
    {
        // Show the second floor geometry
        floor2Group.SetActive(true);
        
        // Raise the black fog blanket so it covers the 2nd floor instead of the 1st
        fogPlane.position = new Vector3(fogPlane.position.x, floor2FogHeight, fogPlane.position.z);
    }

    public void GoToFloor1()
    {
        // Hide the second floor so the top-down camera isn't staring at a roof!
        floor2Group.SetActive(false);
        
        // Lower the black fog blanket back to the 1st floor
        fogPlane.position = new Vector3(fogPlane.position.x, floor1FogHeight, fogPlane.position.z);
    }
}