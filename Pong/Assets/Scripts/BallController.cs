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
    public int playerScore = 0;
    public int aiScore = 0;
    
    [Header("Streak & Difficulty")]
    public float paddleShrinkPerStreak = 0.05f; // How much paddle shrinks per streak point

    private float currentSpeedMultiplier = 1f;
    private int playerStreak = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        ResetBall();
        
        // Send game start marker
        if (LSLEventMarker.Instance != null)
        {
            LSLEventMarker.Instance.SendMarker("GameStart");
        }
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
        {
            playerScore++;
            playerStreak++;
        }
        else 
        {
            aiScore++;
            playerStreak = 0; // Reset streak when AI scores
        }
        
        if (scoreText != null)
            scoreText.text = $"{playerScore} - {aiScore}";

        // Play sound effect for scoring
        if (AudioManager.Instance != null)
        {
            if (playerScored)
            {
                AudioManager.Instance.PlayPlayerScoreSound();
            }
            else
            {
                AudioManager.Instance.PlayAIScoreSound();
            }
        }

        // Send LSL event marker for score
        if (LSLEventMarker.Instance != null)
        {
            if (playerScored)
            {
                LSLEventMarker.Instance.SendMarker("PlayerScore", playerScore);
            }
            else
            {
                LSLEventMarker.Instance.SendMarker("AIScore", aiScore);
            }
        }

        // Frustration mechanic strategy: We DON'T reset the speed multiplier if the AI scored!
        // We only reset it if the player scores, giving them false hope.
        if (playerScored)
        {
            currentSpeedMultiplier = 1f; 
        }

        ResetBall();
    }
    
    public int GetPlayerStreak()
    {
        return playerStreak;
    }

    void ResetBall()
    {
        transform.position = Vector2.zero;
        
        // Randomize launch direction
        float xDir = Random.value < 0.5f ? -1f : 1f;
        float yDir = Random.Range(-0.8f, 0.8f);
        
        rb.linearVelocity = new Vector2(xDir, yDir).normalized * initialSpeed;
        
        // Send ball reset marker
        if (LSLEventMarker.Instance != null)
        {
            LSLEventMarker.Instance.SendMarker("BallReset");
        }
    }
    
    // Add a bit of randomness to bounces so the ball doesn't get stuck in a straight horizontal line
    void OnCollisionEnter2D(Collision2D collision)
    {
        Vector2 tweak = new Vector2(0f, Random.Range(-1f, 1f));
        rb.linearVelocity = (rb.linearVelocity + tweak).normalized * (initialSpeed * currentSpeedMultiplier);
        
        // Play sound effect when ball hits paddle
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBallHitSound();
        }

        // Play confetti/particle effect at collision point
        if (ParticleEffectManager.Instance != null)
        {
            ParticleEffectManager.Instance.PlayConfetti(collision.contacts[0].point);
        }
        
        // Send paddle hit marker
        if (LSLEventMarker.Instance != null)
        {
            LSLEventMarker.Instance.SendMarker($"BallHit_{collision.gameObject.name}");
        }
    }
}
