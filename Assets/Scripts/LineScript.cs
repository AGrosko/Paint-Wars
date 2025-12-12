using System.Collections.Generic;
using UnityEngine;

public class LineScript : MonoBehaviour
{
    [SerializeField]
    private LineRenderer lineRenderer;

    private List<Vector3> trailPoints = new List<Vector3>();

    private Material trailMaterial;

    public float trailWidth = 1.5f;

    public float minPointDistance = 0.01f; // spacing between points



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lineRenderer.material = Resources.Load<Material>("TeamAMat");
        lineRenderer.positionCount = 0;
        lineRenderer.useWorldSpace = true;
        lineRenderer.startWidth = trailWidth;
        lineRenderer.endWidth = trailWidth;



    }

    // Update is called once per frame
    void Update()
    {
        Vector3 groundPos = transform.position;
        groundPos.y = 0.5f; // flatten to ground plane

        Debug.Log("Current Position: " + groundPos);

        if (trailPoints.Count == 0 || Vector3.Distance(trailPoints[trailPoints.Count - 1], groundPos) > minPointDistance)
        {
            trailPoints.Add(groundPos);
            lineRenderer.positionCount = trailPoints.Count;
            lineRenderer.SetPositions(trailPoints.ToArray());
            Debug.Log("Added trail point: " + groundPos + trailPoints.Count);
            
        }


    }

    // Expose trail points for scoring system
    public List<Vector3> GetTrailPoints()
    {
        return trailPoints;
    }

}
