using UnityEngine;
using System.Collections;

public class CameraShaker : MonoBehaviour
{
    public static CameraShaker Instance;

    private Vector3 originalPos;
    private float shakeTime;
    private float shakeDuration;
    private float shakeMagnitude;

    private float noiseSeedX;
    private float noiseSeedY;

    void Awake()
    {
        Instance = this;
        originalPos = transform.localPosition;
    }

    void Update()
    {
        if (shakeTime > 0)
        {
            shakeTime -= Time.deltaTime;

            float x = (Mathf.PerlinNoise(Time.time * 10 + noiseSeedX, 0f) - 0.5f) * 2;
            float y = (Mathf.PerlinNoise(Time.time * 10 + noiseSeedY, 1f) - 0.5f) * 2;

            Vector3 offset = new Vector3(x, y, 0) * shakeMagnitude;
            transform.localPosition = originalPos + offset;
        }
        else
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, originalPos, Time.deltaTime * 5f);
        }
    }

    public void Shake(float duration = 0.3f, float magnitude = 0.08f)
    {
        shakeDuration = duration;
        shakeMagnitude = magnitude;
        shakeTime = duration;

        noiseSeedX = Random.Range(0f, 100f);
        noiseSeedY = Random.Range(0f, 100f);
    }
}
