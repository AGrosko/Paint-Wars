using UnityEngine;

public class PlayerTextHelper : MonoBehaviour
{


    public Canvas worldSpaceCanvas;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (worldSpaceCanvas != null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                worldSpaceCanvas.worldCamera = mainCam;
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (worldSpaceCanvas != null && Camera.main != null)
        {
            worldSpaceCanvas.transform.LookAt(Camera.main.transform);
            worldSpaceCanvas.transform.Rotate(0, 180, 0); // Optional: flip to face correctly
        }

    }
}
