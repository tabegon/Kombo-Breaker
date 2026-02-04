using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;
    
    private Vector3 originalPosition;
    private bool isShaking = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        originalPosition = transform.position;
    }

    public void Shake(float duration = 0.2f, float intensity = 0.3f)
    {
        if (!isShaking)
        {
            StartCoroutine(ShakeCoroutine(duration, intensity));
        }
    }

    IEnumerator ShakeCoroutine(float duration, float intensity)
    {
        isShaking = true;
        Vector3 startPosition = transform.position;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;
            
            transform.position = startPosition + new Vector3(x, y, 0);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        transform.position = startPosition;
        isShaking = false;
    }

    public void UpdateOriginalPosition(Vector3 newPosition)
    {
        originalPosition = newPosition;
    }
}
