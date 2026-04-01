using UnityEngine;

public class ParticleEffectManager : MonoBehaviour
{
    public static ParticleEffectManager Instance { get; private set; }

    [Header("Particle Prefabs")]
    public GameObject confettiPrefab;
    
    [Header("Particle Settings")]
    public int confettiParticleCount = 20;
    public float confettiSpeed = 10f;
    public float confettiLifetime = 2f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void PlayConfetti(Vector3 position)
    {
        if (confettiPrefab != null)
        {
            var confetti = Instantiate(confettiPrefab, position, Quaternion.identity);
            var ps = confetti.GetComponent<ParticleSystem>();
            
            if (ps != null)
            {
                ps.Play();
                Destroy(confetti, confettiLifetime);
            }
        }
        else
        {
            // Fallback: Simple particle effect without a prefab
            SpawnSimpleParticles(position);
        }
    }

    private void SpawnSimpleParticles(Vector3 position)
    {
        // Create simple visual effect by spawning small cubes as particles
        for (int i = 0; i < confettiParticleCount; i++)
        {
            GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            particle.transform.position = position;
            particle.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            
            // Remove collider
            DestroyImmediate(particle.GetComponent<Collider>());
            
            // Random color
            var renderer = particle.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                Color randomColor = new Color(Random.value, Random.value, Random.value, 1f);
                renderer.material.color = randomColor;
            }
            
            // Add velocity
            var rb = particle.AddComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = Random.insideUnitSphere * confettiSpeed;
            
            // Destroy after lifetime
            Destroy(particle, confettiLifetime);
        }
    }
}
