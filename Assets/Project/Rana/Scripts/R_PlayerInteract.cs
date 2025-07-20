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

    void Update()
    {
        //Salıncağa bin yazısı 
        swingPromptUI.SetActive(isNearSwing && !isOnSwing);

        // Çözüm sonrası çık yazısı (salıncaktayken + puzzle tamamlandıysa)
        swingExitPromptUI.SetActive(isOnSwing && panelController.IsPuzzleComplete());

        
        if (Input.GetKeyDown(KeyCode.E) && !isOnSwing && isNearSwing)
        {
            cameraHolder.position = swingController.swingSeat.position;

            // 🎥 Kamerayı salıncağa göre döndür
            Vector3 toSwing = (swingController.swingSeat.position - transform.position).normalized;
            Vector3 swingForward = swingController.swingSeat.forward;

            toSwing.y = 0;
            swingForward.y = 0;

            float angle = Vector3.Angle(swingForward, toSwing);
            float signedAngle = Vector3.SignedAngle(swingForward, toSwing, Vector3.up);

            Quaternion targetRotation;

            if (angle < 45f)
                targetRotation = Quaternion.LookRotation(swingForward);
            else if (angle > 135f)
                targetRotation = Quaternion.LookRotation(-swingForward);
            else if (signedAngle > 0)
                targetRotation = Quaternion.LookRotation(Quaternion.Euler(0, -90, 0) * swingForward);
            else
                targetRotation = Quaternion.LookRotation(Quaternion.Euler(0, 90, 0) * swingForward);

            cameraHolder.rotation = targetRotation;

            swingController.StartSwing();
            isOnSwing = true;
            hasJustMountedSwing = true;
        }

        
        else if (Input.GetKeyDown(KeyCode.E) && isOnSwing && !hasJustMountedSwing)
        {
            if (panelController.GetCurrentLightIndex() == correctTargetIndex)
            {
                Debug.Log(" Doğru anda bastın! PUZZLE İLERLEDİ ");
                audioSource.PlayOneShot(correctSound); 
                panelController.GoToNextStep();
            }
            else
            {
                Debug.Log(" Yanlış anda bastın, tekrar dene!");
                audioSource.PlayOneShot(wrongSound); 
            }
        }


        
        if (Input.GetKeyDown(KeyCode.Escape) && isOnSwing)
        {
            swingController.StopSwing();
            isOnSwing = false;
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










