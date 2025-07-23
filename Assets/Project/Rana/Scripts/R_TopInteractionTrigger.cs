using UnityEngine;
using TMPro;

public class TopInteractionTrigger : MonoBehaviour
{
    [Header("E Tuþu Yazýsý")]
    public GameObject interactionTextUI; // Canvas içindeki TMP yazý objesi

    private void Start()
    {
        // Baþta gizli olsun (önceki prompt sistemine uygun)
        if (interactionTextUI != null)
            interactionTextUI.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Topa yaklaþýldý!");
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


