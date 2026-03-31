using UnityEngine;
using Tobii.Gaming; 
using System.IO; 
using System;    

public class EyeDataLogger : MonoBehaviour
{
    [Header("Logging Settings")]
    [Tooltip("How many times per second should we save data?")]
    public float logsPerSecond = 30f; 

    [Header("Game Data References")]
    [Tooltip("Drag the GameObject with the BallController script here")]
    public BallController ballController; // This creates a slot for your ball script!

    private string filePath;
    private StreamWriter writer;
    
    // Timer variables
    private float timer = 0f;
    private float logInterval;

    void Start()
    {
        logInterval = 1f / logsPerSecond;

        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string directoryPath = Application.dataPath + "/EyeData";

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        filePath = directoryPath + $"/EyeData_{timestamp}.csv";

        writer = new StreamWriter(filePath, true);
        
        // UPDATE 1: Added PlayerScore to the header columns
        writer.WriteLine("TimeElapsed(s),GazeX,GazeY,PlayerScore");
        
        Debug.Log($"Started logging eye data to: {filePath} at {logsPerSecond} times per second.");
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= logInterval)
        {
            GazePoint gazePoint = TobiiAPI.GetGazePoint();

            if (gazePoint.IsValid)
            {
                // UPDATE 2: Safely grab the score from the BallController
                int currentScore = 0;
                if (ballController != null)
                {
                    currentScore = ballController.playerScore;
                }

                // UPDATE 3: Write the score at the end of the CSV row
                writer.WriteLine($"{Time.time},{gazePoint.Screen.x},{gazePoint.Screen.y},{currentScore}");
            }

            timer -= logInterval;
        }
    }

    void OnApplicationQuit()
    {
        if (writer != null)
        {
            writer.Close();
            Debug.Log("Eye tracking log saved successfully.");
        }
    }
}