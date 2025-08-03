using Unity.Netcode;
using UnityEngine;

public class ButtonTrigger : NetworkBehaviour
{
    [Header("Button Settings")]
    public int buttonIndex; // 0 veya 1
    public R_DoubleButtonPuzzle puzzleManager;

    private AudioSource audioSource;
    private bool isPlayerOnButton = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if (other.CompareTag("Player"))
        {
            isPlayerOnButton = true;
            puzzleManager.SetButtonStateServerRpc(buttonIndex, true);

            // Ses efekti için tüm clientlara bildir
            PlayButtonSoundClientRpc();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;

        if (other.CompareTag("Player"))
        {
            isPlayerOnButton = false;
            puzzleManager.SetButtonStateServerRpc(buttonIndex, false);
        }
    }

    [ClientRpc]
    private void PlayButtonSoundClientRpc()
    {
        if (audioSource != null)
            audioSource.Play();
    }
}

