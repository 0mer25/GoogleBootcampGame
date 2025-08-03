using UnityEngine;

public class TitleEffect : MonoBehaviour
{
    public float swayY = 2f;
    public float rotateZ = 1.2f;
    public float swaySpeed = 0.7f;
    public float pulseMin = 0.98f;
    public float pulseMax = 1.02f;
    public float pulseSpeed = 1.1f;

    private Vector3 startPos;
    private Quaternion startRot;
    private Vector3 startScale;

    void Awake()
    {
        startPos = transform.localPosition;
        startRot = transform.localRotation;
        startScale = transform.localScale;
    }

    void Update()
    {
        float t = Time.unscaledTime;

        // sallanma
        float s = Mathf.Sin(t * swaySpeed);
        transform.localPosition = startPos + new Vector3(0, s * swayY, 0);
        transform.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t * swaySpeed) * rotateZ);

        // nabýz
        float p = (Mathf.Sin(t * pulseSpeed) + 1f) / 2f;
        float scale = Mathf.Lerp(pulseMin, pulseMax, p);
        transform.localScale = startScale * scale;
    }
}

