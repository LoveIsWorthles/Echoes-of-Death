using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    [Header("Vision Settings")]
    public float viewRadius = 10f;
    [Range(0, 360)] public float viewAngle = 90f;
    
    [Header("Performance & Targeting")]
    public LayerMask obstacleMask; // Tells the rays what blocks vision
    public float meshResolution = 1f; // How many rays to shoot per degree. Higher = smoother, but heavier on CPU.

    [Header("Base (omnidirectional) Radius")]
    [Tooltip("A short radius around the player that's always visible (360°). Useful to see sides/behind slightly).")]
    public float baseRadius = 1.5f;

    public MeshFilter viewMeshFilter;
    private Mesh viewMesh;

    void Start()
    {
        // Initialize the empty mesh
        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        viewMeshFilter.mesh = viewMesh;
    }

    void LateUpdate()
    {
        // We draw the mesh in LateUpdate so it happens AFTER the player has moved and rotated
        DrawFieldOfView();
    }

    void DrawFieldOfView()
    {
        // We'll sample around the full 360° so we can combine the main cone
        // (at `viewRadius`) with a short omnidirectional base (`baseRadius`).
        int stepCount = Mathf.Max(8, Mathf.RoundToInt(360f * meshResolution));
        float stepAngleSize = 360f / stepCount;
        List<Vector3> viewPoints = new List<Vector3>();

        float forwardAngle = transform.eulerAngles.y;

        // 1. Shoot the rays in a full circle; use `viewRadius` inside the cone, `baseRadius` elsewhere
        for (int i = 0; i <= stepCount; i++)
        {
            float angle = transform.eulerAngles.y - 180f + stepAngleSize * i;
            Vector3 dir = DirFromAngle(angle, true);

            bool withinCone = Mathf.Abs(Mathf.DeltaAngle(forwardAngle, angle)) <= viewAngle / 2f;
            float sampleRadius = withinCone ? viewRadius : baseRadius;

            // If the ray hits an obstacle, add the hit point. Otherwise, add the point at the chosen radius.
            if (Physics.Raycast(transform.position, dir, out RaycastHit hit, sampleRadius, obstacleMask))
            {
                viewPoints.Add(hit.point);
            }
            else
            {
                viewPoints.Add(transform.position + dir * sampleRadius);
            }
        }

        // 2. Build the vertices (corners) of the mesh
        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero; // The origin point is the player

        for (int i = 0; i < vertexCount - 1; i++)
        {
            // Convert world space to local space for the mesh
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);

            // Connect the dots to draw the triangles that make up the shape
            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        // 3. Apply it to the actual visual mesh
        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();

        viewMesh.RecalculateBounds();
    }

    // Helper math function to turn an angle into a 3D direction vector
    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}