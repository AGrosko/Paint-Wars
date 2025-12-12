using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MeshTrail : MonoBehaviour
{
    public float trailWidth = 0.5f;
    public float minPointDistance = 0.1f;
    public Material trailMaterial;

    private List<Vector3> points = new List<Vector3>();
    private Mesh mesh;

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        MeshRenderer mr = GetComponent<MeshRenderer>();

        // switch this later to use team color
        mr.material =  new Material(Shader.Find("Unlit/Color")) { color = Color.green };
    }

    void Update()
    {
        Vector3 pos = transform.position;
        pos.y = 0.01f;

        if (points.Count == 0 || Vector3.Distance(points[points.Count - 1], pos) > minPointDistance)
        {
            points.Add(pos);
            UpdateMesh();
        }
    }

    void UpdateMesh()
    {
        if (points.Count < 2) return;

        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();

        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 dir = (points[i + 1] - points[i]).normalized;
            Vector3 normal = new Vector3(-dir.z, 0, dir.x) * trailWidth * 0.5f;

            Vector3 aLeft = points[i] - normal;
            Vector3 aRight = points[i] + normal;
            Vector3 bLeft = points[i + 1] - normal;
            Vector3 bRight = points[i + 1] + normal;

            verts.Add(aLeft);
            verts.Add(aRight);
            verts.Add(bLeft);
            verts.Add(bRight);

            int start = i * 4;

            // First triangle
            tris.Add(start);
            tris.Add(start + 1);
            tris.Add(start + 2);

            // Second triangle
            tris.Add(start + 1);
            tris.Add(start + 3);
            tris.Add(start + 2);
        }

        mesh.Clear();
        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateNormals();

        Debug.Log("Trail updated: verts=" + verts.Count + " tris=" + tris.Count);
    }
}