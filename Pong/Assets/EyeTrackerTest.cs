using UnityEngine;
using Tobii.Gaming; // Our classic, working namespace!

public class EyeTrackerTest : MonoBehaviour
{
    [Header("Assign this in the Inspector!")]
    public RectTransform crosshair;

    void Update()
    {
        // 1. Get the current frame of eye data
        GazePoint gazePoint = TobiiAPI.GetGazePoint();

        // 2. Check if the tracker actually sees your eyes right now
        if (gazePoint.IsValid)
        {
            // 3. Move the UI crosshair to your exact eye coordinates
            Vector3 pos = crosshair.position;
            pos.y = gazePoint.Screen.y;
            crosshair.position = pos;
        }
    }
}