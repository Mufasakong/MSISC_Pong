using UnityEngine;
using Tobii.Gaming; // Our classic, working namespace!

public class EyeTrackerTest : MonoBehaviour
{
    [Header("Assign this in the Inspector!")]
    public Transform crosshair;

    [Header("Movement Settings")]
    [Tooltip("How smoothly the paddle follows the eyes. Lower is smoother/slower.")]
    public float smoothSpeed = 10f;
    [Tooltip("Maximum up/down world position for the paddle")]
    public float yLimit = 4f;

    void Update()
    {
        // 1. Get the current frame of eye data
        GazePoint gazePoint = TobiiAPI.GetGazePoint();

        // 2. Check if the tracker actually sees your eyes right now
        if (gazePoint.IsValid)
        {
            // Convert eye screen coordinates (pixels) to World coordinates
            Vector3 eyeScreenPos = gazePoint.Screen;
            eyeScreenPos.z = Mathf.Abs(Camera.main.transform.position.z); 
            Vector3 worldTarget = Camera.main.ScreenToWorldPoint(eyeScreenPos);

            // 3. Find target position and clamp it so it doesn't go off-screen
            Vector3 targetPos = crosshair.position;
            targetPos.y = Mathf.Clamp(worldTarget.y, -yLimit, yLimit);

            // 4. Smoothly move the crosshair to the target instead of snapping instantly
            crosshair.position = Vector3.Lerp(crosshair.position, targetPos, smoothSpeed * Time.deltaTime);
        }
    }
}