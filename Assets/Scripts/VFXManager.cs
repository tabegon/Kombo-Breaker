using UnityEngine;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance;
    
    [Header("Particle Systems")]
    public ParticleSystem hitParticles;
    public ParticleSystem dodgeParticles;
    public ParticleSystem victoryParticles;
    public ParticleSystem powerUpParticles;
    public ParticleSystem trailParticles;
    
    [Header("Prefabs (si tu veux instancier)")]
    public GameObject hitEffectPrefab;
    public GameObject dodgeEffectPrefab;
    public GameObject victoryEffectPrefab;
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void PlayHitEffect(Vector3 position)
    {
        if (hitParticles != null)
        {
            hitParticles.transform.position = position;
            hitParticles.Play();
        }
        else if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, position, Quaternion.identity);
            Destroy(effect, 2f);
        }
    }

    public void PlayDodgeEffect(Vector3 position)
    {
        if (dodgeParticles != null)
        {
            dodgeParticles.transform.position = position;
            dodgeParticles.Play();
        }
        else if (dodgeEffectPrefab != null)
        {
            GameObject effect = Instantiate(dodgeEffectPrefab, position, Quaternion.identity);
            Destroy(effect, 2f);
        }
    }

    public void PlayVictoryEffect(Vector3 position)
    {
        if (victoryParticles != null)
        {
            victoryParticles.transform.position = position;
            victoryParticles.Play();
        }
        else if (victoryEffectPrefab != null)
        {
            GameObject effect = Instantiate(victoryEffectPrefab, position, Quaternion.identity);
            Destroy(effect, 3f);
        }
    }

    public void PlayPowerUpEffect(Vector3 position)
    {
        if (powerUpParticles != null)
        {
            powerUpParticles.transform.position = position;
            powerUpParticles.Play();
        }
    }

    public void EnableTrail(TrailRenderer trail)
    {
        if (trail != null)
        {
            trail.enabled = true;
            trail.Clear();
        }
    }

    public void DisableTrail(TrailRenderer trail)
    {
        if (trail != null)
        {
            trail.enabled = false;
        }
    }
}
