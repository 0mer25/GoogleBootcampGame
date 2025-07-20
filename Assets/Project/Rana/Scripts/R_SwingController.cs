using UnityEngine;

public class R_SwingController : MonoBehaviour
{
    public Transform swingSeat; // Kameranýn geçici baðlanacaðý yer
    public float swingSpeed = 2f;
    public float swingAmount = 0.3f;

    private bool isSwinging = false;
    private float timer = 0f;
    private Vector3 originalCamPos;

    void Update()
    {
        if (isSwinging)
        {
            timer += Time.deltaTime * swingSpeed;
            float swingOffset = Mathf.Sin(timer) * swingAmount;
            Camera.main.transform.localPosition = originalCamPos + new Vector3(0, 0, swingOffset);
        }
    }

    public void StartSwing()
    {
        originalCamPos = Camera.main.transform.localPosition;
        isSwinging = true;
        timer = 0f;
    }

    public void StopSwing()
    {
        isSwinging = false;
        Camera.main.transform.localPosition = originalCamPos;
    }
}
