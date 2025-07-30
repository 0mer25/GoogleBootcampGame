using UnityEngine;

public class PuzzleInteractable : MonoBehaviour
{
    public PuzzleManager puzzleManager;
    public GameObject interactionUI; 

    private bool playerNearby = false;
    private bool interacted = false;

    private void Start()
    {
        if (interactionUI != null)
            interactionUI.SetActive(false); 
    }

    private void Update()
    {
        if (playerNearby && !interacted && Input.GetKeyDown(KeyCode.E))
        {
            interacted = true;
            if (interactionUI != null)
                interactionUI.SetActive(false);

            Debug.Log($"🟢 {gameObject.name} ile etkileşime geçildi.");
            puzzleManager.RegisterInteraction(gameObject); 
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !interacted)
        {
            playerNearby = true;
            if (interactionUI != null)
                interactionUI.SetActive(true); 
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            if (interactionUI != null)
                interactionUI.SetActive(false); 
        }
    }
}




