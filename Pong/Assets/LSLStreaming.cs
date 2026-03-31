using UnityEngine;
using LSL; 
using System.IO;
using System;

public class LSLStreaming : MonoBehaviour
{
    [Header("LSL Settings")]
    [Tooltip("The 'type' of the LSL stream broadcasted by OpenSignals")]
    public string streamType = "HRV"; 

    private StreamInfo[] streamInfos;
    private StreamInlet inlet;
    private float[] sample; 

    private string filePath;
    private StreamWriter writer;

    void Start()
    {
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string directoryPath = Application.dataPath + "/HRVDATA";

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        filePath = directoryPath + $"/HRV_{timestamp}.csv";
        writer = new StreamWriter(filePath, true);
        
        writer.WriteLine("TimeElapsed(s),HRV_Value"); 

        Debug.Log($"Looking for LSL stream of type: {streamType}...");
        
        // THE FIX IS RIGHT HERE:
        streamInfos = LSL.LSL.resolve_stream("type", streamType, 1, 0.0);
        
        if (streamInfos.Length > 0)
        {
            inlet = new StreamInlet(streamInfos[0]);
            
            // Note: If OpenSignals sends multiple channels of data at once, 
            // you might need to increase this array size (e.g., new float[3])
            sample = new float[1]; 
            
            Debug.Log("LSL HRV Stream found and connected!");
            Debug.Log("Started logging HRV data to: " + filePath);
        }
        else
        {
            Debug.LogError($"Uh oh! No LSL stream found with type '{streamType}'. Is OpenSignals broadcasting?");
        }
    }

    void Update()
    {
        if (inlet != null)
        {
            double timestamp = inlet.pull_sample(sample, 0.0);

            if (timestamp != 0.0) 
            {
                writer.WriteLine($"{Time.time},{sample[0]}");
            }
        }
    }

    void OnApplicationQuit()
    {
        if (writer != null)
        {
            writer.Close();
            Debug.Log("HRV tracking log saved successfully.");
        }
    }
}