using UnityEngine;
using Tobii.Gaming;
using LSL;

/// <summary>
/// Streams Tobii eye tracking data via LSL (Lab Streaming Layer)
/// This allows synchronization with other LSL streams like OpenViBE sensors
/// </summary>
public class LSLEyeTrackingStreamer : MonoBehaviour
{
    [Header("LSL Stream Settings")]
    [Tooltip("Name of the LSL stream")]
    public string streamName = "UnityEyeTracking";
    
    [Tooltip("Type identifier for the stream")]
    public string streamType = "Gaze";
    
    [Header("Sampling Settings")]
    [Tooltip("How many times per second to push data")]
    public float samplesPerSecond = 90f;

    private StreamOutlet outlet;
    private float[] sample;
    private float timer = 0f;
    private float sampleInterval;

    void Start()
    {
        // Sample contains: [GazeX, GazeY, Timestamp, IsValid]
        sample = new float[4];
        sampleInterval = 1f / samplesPerSecond;

        // Create LSL stream info
        // Channels: GazeX, GazeY, LocalTimestamp, IsValid
        StreamInfo streamInfo = new StreamInfo(
            streamName, 
            streamType, 
            4,  // 4 channels
            samplesPerSecond, 
            channel_format_t.cf_float32, 
            "UnityEyeTracker_" + System.Guid.NewGuid().ToString()
        );

        // Add metadata
        XMLElement channels = streamInfo.desc().append_child("channels");
        
        channels.append_child("channel")
            .append_child_value("label", "GazeX")
            .append_child_value("unit", "pixels")
            .append_child_value("type", "ScreenX");
            
        channels.append_child("channel")
            .append_child_value("label", "GazeY")
            .append_child_value("unit", "pixels")
            .append_child_value("type", "ScreenY");
            
        channels.append_child("channel")
            .append_child_value("label", "LocalTime")
            .append_child_value("unit", "seconds")
            .append_child_value("type", "Timestamp");
            
        channels.append_child("channel")
            .append_child_value("label", "IsValid")
            .append_child_value("unit", "boolean")
            .append_child_value("type", "Quality");

        // Create the outlet
        outlet = new StreamOutlet(streamInfo);

        Debug.Log($"LSL Eye Tracking Stream created: {streamName} (type: {streamType})");
        Debug.Log($"Streaming at {samplesPerSecond} Hz with 4 channels: GazeX, GazeY, LocalTime, IsValid");
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= sampleInterval)
        {
            GazePoint gazePoint = TobiiAPI.GetGazePoint();

            // Fill sample data
            if (gazePoint.IsValid)
            {
                sample[0] = gazePoint.Screen.x;
                sample[1] = gazePoint.Screen.y;
                sample[2] = Time.time;
                sample[3] = 1f; // Valid
            }
            else
            {
                sample[0] = -1f;
                sample[1] = -1f;
                sample[2] = Time.time;
                sample[3] = 0f; // Invalid
            }

            // Push sample to LSL
            outlet.push_sample(sample);

            timer -= sampleInterval;
        }
    }

    void OnApplicationQuit()
    {
        if (outlet != null)
        {
            Debug.Log("LSL Eye Tracking Stream closed.");
        }
    }
}
