using UnityEngine;

public class FloorManager : MonoBehaviour
{
    [Header("Floor Groups")]
    public GameObject floor1Group;
    public GameObject floor2Group;

    [Header("Enemies")]
    [Tooltip("Drag your 'Enemies' GameObject here in the Inspector")]
    public GameObject floor2Enemies; 

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
        // GameObject.Find skips inactive objects, so we use a scene-wide search
        if (floor1Group == null) floor1Group = FindInLoadedScenesByName("Floor1_Group");
        if (floor2Group == null) floor2Group = FindInLoadedScenesByName("Floor2_Group");
        if (floor2Enemies == null) floor2Enemies = FindInLoadedScenesByName("Enemies");

        if (fogPlane == null)
        {
            GameObject fog = FindInLoadedScenesByName("Fog_Plane");
            if (fog != null) fogPlane = fog.transform;
        }
    }

    public void GoToFloor2()
    {
        // SHOW Floor 2 and Enemies (Physics & NavMesh are always active)
        SetGroupVisibility(floor2Group, true);
        SetGroupVisibility(floor2Enemies, true);

        if (fogPlane != null)
        {
            fogPlane.position = new Vector3(fogPlane.position.x, floor2FogHeight, fogPlane.position.z);
        }
    }

    public void GoToFloor1()
    {
        // HIDE Floor 2 and Enemies visually (Physics & NavMesh stay active)
        SetGroupVisibility(floor2Group, false);
        SetGroupVisibility(floor2Enemies, false);

        if (fogPlane != null)
        {
            fogPlane.position = new Vector3(fogPlane.position.x, floor1FogHeight, fogPlane.position.z);
        }
    }

    // Helper function to turn graphics on/off without breaking NavMesh/Colliders
    private void SetGroupVisibility(GameObject group, bool isVisible)
    {
        if (group == null) return;

        Renderer[] renderers = group.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.enabled = isVisible;
        }
    }

    private static GameObject FindInLoadedScenesByName(string name)
    {
        GameObject[] all = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject go in all)
        {
            if (go.name != name) continue;
            if (!go.scene.IsValid()) continue; // skip prefab assets
            return go;
        }
        return null;
    }
}