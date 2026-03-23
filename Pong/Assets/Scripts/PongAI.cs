using UnityEngine;

public class PongAI : MonoBehaviour
{
    [Header("References")]
    public Transform ball;

    [Header("Movement Settings")]
    public float baseSpeed = 5f;
    [Tooltip("Unfairly fast speed when the ball gets close")]
    public float catchUpSpeed = 15f; 

    [Header("Frustration Mechanics")]
    [Tooltip("Probability of the AI suddenly freezing for a moment")]
    public float freezeProbability = 0.5f;
    [Tooltip("Maximum distance the AI will intentionally misjudge the ball")]
    public float maxTrackingError = 2.0f;
    [Tooltip("Amount of visual jitter/shaking to make movement feel unnatural")]
    public float jitterAmount = 0.1f;
    
    [Tooltip("Distance at which the AI stops acting dumb and perfectly blocks the ball")]
    public float panicDistance = 4f;

    [Header("Difficulty Progression")]
    [Tooltip("How long (in seconds) it takes for the AI to reach its maximum difficulty. Good for eye-tracking users.")]
    public float difficultyRampUpTime = 90f;

    private float currentTrackingError = 0f;
    private float freezeTimer = 0f;
    private float timePassedInRally = 0f;
    private float totalTimePlayed = 0f;

    void Start()
    {
        // Periodically change the tracking error so the AI seems inconsistent
        InvokeRepeating(nameof(UpdateTrackingError), 0.5f, 1.5f);
    }

    void UpdateTrackingError()
    {
        currentTrackingError = Random.Range(-maxTrackingError, maxTrackingError);
    }

    void Update()
    {
        if (ball == null) return;
        
        // Progressively scale overall difficulty based on total session time played
        totalTimePlayed += Time.deltaTime;
        float sessionDifficulty = Mathf.Clamp01(totalTimePlayed / difficultyRampUpTime);

        // Scale AI unfairness based on how long the rally is going
        timePassedInRally += Time.deltaTime;
        float unfairnessMultiplier = 1f + (timePassedInRally * 0.1f); // AI gets 10% more aggressive/glitchy every second

        // 1. The "Freeze" Glitch: AI randomly stops responding to make the user think they won
        if (freezeTimer > 0)
        {
            freezeTimer -= Time.deltaTime;
            return; // Don't move
        }

        if (Random.value < (freezeProbability * unfairnessMultiplier * Time.deltaTime))
        {
            freezeTimer = Random.Range(0.2f, 0.6f); // Freeze for a fraction of a second
        }

        // Calculate horizontal distance to the ball
        float distanceToBallX = Mathf.Abs(ball.position.x - transform.position.x);

        // 2. Misaligned Tracking vs. Rubber-banding
        float targetY = ball.position.y;
        
        // Scale speeds so the AI is very slow and beatable initially
        float currentSpeed = Mathf.Lerp(1.5f, baseSpeed, sessionDifficulty);
        float currentPanicDistance = Mathf.Lerp(0f, panicDistance, sessionDifficulty);

        if (distanceToBallX > currentPanicDistance)
        {
            // When the ball is far, the AI tracks 'badly' (intentionally misaligned)
            // Make tracking errors worse early on to guarantee the player can score
            targetY += currentTrackingError * Mathf.Lerp(2.5f, 1f, sessionDifficulty);
        }
        else
        {
            // 3. Unfair Recovery: When ball gets close, instantly correct the error and speed up
            // This frustrates the user because it looks like they had it, but the AI cheats at the last second
            // Early on (sessionDifficulty near 0), it won't have the insane catch-up speed yet
            currentSpeed = Mathf.Lerp(currentSpeed, catchUpSpeed * unfairnessMultiplier, sessionDifficulty);
        }

        // Move paddle towards the targetY
        float newY = Mathf.MoveTowards(transform.position.y, targetY, currentSpeed * Time.deltaTime);

        // 4. Jitter: Add tiny, unexpected movements to make the AI feel erratic and broken
        if (Random.value > 0.8f)
        {
            newY += Random.Range(-jitterAmount * unfairnessMultiplier, jitterAmount * unfairnessMultiplier);
        }

        // Apply position
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        
        // Reset rally timer if ball is far away (near center)
        if (Mathf.Abs(ball.position.x) < 1f) timePassedInRally = 0f;
    }
}
