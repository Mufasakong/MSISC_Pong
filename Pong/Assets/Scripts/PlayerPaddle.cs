using UnityEngine;
using UnityEngine.InputSystem;

// A simple manual paddle controller for testing before you hook up your eye-tracker
public class PlayerPaddle : MonoBehaviour
{
    public float speed = 10f;
    public float yBoundary = 4.5f; // Adjust this to keep the paddle on screen
    
    [Header("Streak Penalty")]
    public BallController ballController;
    private Vector3 initialScale;
    private float minScaleMultiplier = 0.3f; // Paddle won't shrink smaller than 30% of original size

    void Start()
    {
        initialScale = transform.localScale;
    }

    void Update()
    {
        // Get generic Up/Down arrow or W/S key input using the new Input System
        float move = 0f;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.upArrowKey.isPressed || Keyboard.current.wKey.isPressed)
                move = 1f;
            else if (Keyboard.current.downArrowKey.isPressed || Keyboard.current.sKey.isPressed)
                move = -1f;
        }

        float newY = transform.position.y + move * speed * Time.deltaTime;
        
        // Prevent the paddle from leaving the top/bottom of the screen
        newY = Mathf.Clamp(newY, -yBoundary, yBoundary);

        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        
        // Update paddle size based on player streak
        if (ballController != null)
        {
            int streak = ballController.GetPlayerStreak();
            float scaleMultiplier = Mathf.Max(minScaleMultiplier, 1f - (streak * ballController.paddleShrinkPerStreak));
            transform.localScale = new Vector3(initialScale.x, initialScale.y * scaleMultiplier, initialScale.z);
        }
    }
}