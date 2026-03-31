using UnityEngine;
using LSL;

/// <summary>
/// Streams event markers via LSL for synchronization with sensor data
/// Use this to mark important game events (ball hits, score changes, etc.)
/// </summary>
public class LSLEventMarker : MonoBehaviour
{
    [Header("LSL Stream Settings")]
    [Tooltip("Name of the marker stream")]
    public string streamName = "UnityMarkers";
    
    [Tooltip("Type identifier for the stream")]
    public string streamType = "Markers";

    private StreamOutlet outlet;
    private string[] markerSample;

    void Start()
    {
        // Marker stream has 1 string channel
        markerSample = new string[1];

        // Create LSL stream info for markers
        StreamInfo streamInfo = new StreamInfo(
            streamName, 
            streamType, 
            1,  // 1 channel (marker text)
            0,  // Irregular rate (events happen when they happen)
            channel_format_t.cf_string, 
            "UnityEventMarkers_" + System.Guid.NewGuid().ToString()
        );

        // Add metadata
        XMLElement channels = streamInfo.desc().append_child("channels");
        channels.append_child("channel")
            .append_child_value("label", "MarkerString")
            .append_child_value("type", "Event");

        // Create the outlet
        outlet = new StreamOutlet(streamInfo);

        Debug.Log($"LSL Event Marker Stream created: {streamName} (type: {streamType})");
        Debug.Log("Use LSLEventMarker.Instance.SendMarker(\"YourEvent\") to send markers!");
    }

    /// <summary>
    /// Send a string marker to the LSL stream
    /// </summary>
    /// <param name="marker">The marker text (e.g., "BallHit", "ScoreIncreased", "GameStart")</param>
    public void SendMarker(string marker)
    {
        if (outlet != null)
        {
            markerSample[0] = marker;
            outlet.push_sample(markerSample);
            Debug.Log($"LSL Marker sent: {marker} at time {Time.time}");
        }
    }

    /// <summary>
    /// Send a numerical marker (will be converted to string)
    /// </summary>
    /// <param name="markerValue">A number to send as a marker</param>
    public void SendMarker(int markerValue)
    {
        SendMarker(markerValue.ToString());
    }

    /// <summary>
    /// Send a marker with additional data
    /// </summary>
    /// <param name="eventName">Name of the event</param>
    /// <param name="value">Additional value</param>
    public void SendMarker(string eventName, float value)
    {
        SendMarker($"{eventName}:{value}");
    }

    void OnApplicationQuit()
    {
        if (outlet != null)
        {
            SendMarker("GameEnd");
            Debug.Log("LSL Event Marker Stream closed.");
        }
    }

    // Singleton pattern for easy access from anywhere
    private static LSLEventMarker _instance;
    public static LSLEventMarker Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<LSLEventMarker>();
                if (_instance == null)
                {
                    Debug.LogWarning("No LSLEventMarker found in scene. Creating one.");
                    GameObject go = new GameObject("LSLEventMarker");
                    _instance = go.AddComponent<LSLEventMarker>();
                }
            }
            return _instance;
        }
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
}
