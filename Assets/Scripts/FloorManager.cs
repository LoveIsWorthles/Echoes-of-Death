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
        // GameObject.Find skips inactive objects, and Floor2_Group typically starts inactive,
        // so use a scene-wide search that includes inactive GameObjects.
        if (floor1Group == null) floor1Group = FindInLoadedScenesByName("Floor1_Group");
        if (floor2Group == null) floor2Group = FindInLoadedScenesByName("Floor2_Group");
        if (fogPlane == null)
        {
            GameObject fog = FindInLoadedScenesByName("Fog_Plane");
            if (fog != null) fogPlane = fog.transform;
        }
    }

    public void GoToFloor2()
    {
        if (floor2Group != null) floor2Group.SetActive(true);
        if (fogPlane != null)
        {
            fogPlane.position = new Vector3(fogPlane.position.x, floor2FogHeight, fogPlane.position.z);
        }
    }

    public void GoToFloor1()
    {
        if (floor2Group != null) floor2Group.SetActive(false);
        if (fogPlane != null)
        {
            fogPlane.position = new Vector3(fogPlane.position.x, floor1FogHeight, fogPlane.position.z);
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