using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Clips")]
    public AudioClip ballHitClip;
    public AudioClip playerScoreClip;
    public AudioClip aiScoreClip;

    private AudioSource audioSource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void PlayBallHitSound()
    {
        if (ballHitClip != null)
            audioSource.PlayOneShot(ballHitClip);
    }

    public void PlayPlayerScoreSound()
    {
        if (playerScoreClip != null)
            audioSource.PlayOneShot(playerScoreClip);
    }

    public void PlayAIScoreSound()
    {
        if (aiScoreClip != null)
            audioSource.PlayOneShot(aiScoreClip);
    }
}
