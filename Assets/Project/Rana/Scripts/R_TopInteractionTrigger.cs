using UnityEngine;
using TMPro;

public class TopInteractionTrigger : MonoBehaviour
{
    [Header("E Tu�u Yaz�s�")]
    public GameObject interactionTextUI; // Canvas i�indeki TMP yaz� objesi

    private void Start()
    {
        // Ba�ta gizli olsun (�nceki prompt sistemine uygun)
        if (interactionTextUI != null)
            interactionTextUI.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Topa yakla��ld�!");
            interactionTextUI.SetActive(true);
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && interactionTextUI != null)
        {
            interactionTextUI.SetActive(false);
        }
    }
}


