using UnityEngine;
using TMPro;

public class R_PlayerInteract : MonoBehaviour
{
    public R_SwingController swingController;
    public Transform cameraHolder;
    public GameObject swingPromptUI;
    public GameObject swingExitPromptUI;

    public AudioSource audioSource;
    public AudioClip correctSound;
    public AudioClip wrongSound;

    public R_PanelController panelController;
    public int correctTargetIndex;

    private bool isOnSwing = false;
    private bool isNearSwing = false;
    private bool hasJustMountedSwing = false;

    private float swingTime = 0f;
    public float swingSpeed = 2f;
    public float swingAmplitude = 0.2f;
    private Vector3 initialCameraLocalPos;

    void Start()
    {
        initialCameraLocalPos = cameraHolder.localPosition;
    }

    void Update()
    {
        swingPromptUI.SetActive(isNearSwing && !isOnSwing);
        swingExitPromptUI.SetActive(isOnSwing && panelController.IsPuzzleComplete());

        if (Input.GetKeyDown(KeyCode.E) && !isOnSwing && isNearSwing)
        {
            isOnSwing = true;
            hasJustMountedSwing = true;
            swingController.StartSwing();
        }
        else if (Input.GetKeyDown(KeyCode.E) && isOnSwing && !hasJustMountedSwing)
        {
            if (panelController.GetCurrentLightIndex() == correctTargetIndex)
            {
                Debug.Log("✅ Doğru anda bastın! PUZZLE İLERLEDİ");
                audioSource.PlayOneShot(correctSound);
                panelController.GoToNextStep();
            }
            else
            {
                Debug.Log("❌ Yanlış anda bastın, tekrar dene!");
                audioSource.PlayOneShot(wrongSound);
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape) && isOnSwing)
        {
            isOnSwing = false;
            swingController.StopSwing();
            cameraHolder.localPosition = initialCameraLocalPos;
        }

        if (isOnSwing)
        {
            swingTime += Time.deltaTime * swingSpeed;
            float yOffset = Mathf.Sin(swingTime) * swingAmplitude;
            cameraHolder.localPosition = initialCameraLocalPos + new Vector3(0, yOffset, 0);
        }

        if (hasJustMountedSwing)
        {
            hasJustMountedSwing = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SwingTrigger"))
            isNearSwing = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("SwingTrigger"))
            isNearSwing = false;
    }
}













