using UnityEngine;

public class ButtonTrigger : MonoBehaviour
{
    public int buttonIndex; // 0 ya da 1
    public R_DoubleButtonPuzzle puzzleManager;
    private AudioSource audioSource;


    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }




    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            puzzleManager.SetButtonState(buttonIndex, true);
        }

        if (audioSource != null)
            audioSource.Play();

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            puzzleManager.SetButtonState(buttonIndex, false);
        }
    }
}

