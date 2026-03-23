using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class BallController : MonoBehaviour
{
    private Rigidbody2D rb;
    
    [Header("Game Settings")]
    public float initialSpeed = 6f;
    [Tooltip("How much the ball speed increases per second")]
    public float difficultyRampRate = 0.1f; 
    [Tooltip("X position where the ball goes out of bounds to score")]
    public float xBoundary = 10f; 
    
    [Header("UI")]
    public Text scoreText; // Assign a UI Text element here
    private int playerScore = 0;
    private int aiScore = 0;

    private float currentSpeedMultiplier = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        ResetBall();
    }

    void Update()
    {
        // 1. Difficulty Ramp: Ball gets progressively and continuously faster
        currentSpeedMultiplier += difficultyRampRate * Time.deltaTime;

        // 2. Enforce Target Velocity smoothly
        if (rb.linearVelocity.sqrMagnitude > 0)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * (initialSpeed * currentSpeedMultiplier);
        }

        // 3. Simple Minimal Scoring (No extra trigger scripts/colliders needed!)
        if (transform.position.x < -xBoundary)
        {
            Score(false); // AI scored (ball passed player on the left)
        }
        else if (transform.position.x > xBoundary)
        {
            Score(true); // Player scored (ball passed AI on the right)
        }
    }

    void Score(bool playerScored)
    {
        if (playerScored) 
            playerScore++;
        else 
            aiScore++;
        
        if (scoreText != null)
            scoreText.text = $"{playerScore} - {aiScore}";

        // Frustration mechanic strategy: We DON'T reset the speed multiplier if the AI scored!
        // We only reset it if the player scores, giving them false hope.
        if (playerScored)
        {
            currentSpeedMultiplier = 1f; 
        }

        ResetBall();
    }

    void ResetBall()
    {
        transform.position = Vector2.zero;
        
        // Randomize launch direction
        float xDir = Random.value < 0.5f ? -1f : 1f;
        float yDir = Random.Range(-0.8f, 0.8f);
        
        rb.linearVelocity = new Vector2(xDir, yDir).normalized * initialSpeed;
    }
    
    // Add a bit of randomness to bounces so the ball doesn't get stuck in a straight horizontal line
    void OnCollisionEnter2D(Collision2D collision)
    {
        Vector2 tweak = new Vector2(0f, Random.Range(-1f, 1f));
        rb.linearVelocity = (rb.linearVelocity + tweak).normalized * (initialSpeed * currentSpeedMultiplier);
    }
}
