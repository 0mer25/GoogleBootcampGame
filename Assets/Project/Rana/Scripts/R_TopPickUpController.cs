using UnityEngine;

public class TopPickupController : MonoBehaviour
{
    [Header("Bağlantılar")]
    public Transform topHolder;         // Player içindeki boş obje
    public Rigidbody topRb;             // Topun Rigidbody bileşeni
    public GameObject interactionUI;    // "E tuşuna bas" yazısı

    private bool isHolding = false;
    private bool playerNearby = false;

    void Start()
    {
        if (interactionUI != null)
            interactionUI.SetActive(false); // Başta yazı görünmesin

        if (topRb == null)
            topRb = GetComponent<Rigidbody>(); // Otomatik bağlanırsa harika olur
    }

    void Update()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.E) && !isHolding)
        {
            Debug.Log("🎯 E’ye basıldı, top alınacak.");
            PickUpTop();
        }
    }

    void PickUpTop()
    {
        if (topHolder == null)
        {
            Debug.LogWarning("❌ TopHolder atanmadı!");
            return;
        }

        isHolding = true;

        // Rigidbody ayarları
        if (topRb != null)
        {
            topRb.isKinematic = true;
            topRb.useGravity = false;
        }

        // Pozisyon ve ebeveynlik
        transform.SetParent(topHolder);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        if (interactionUI != null)
            interactionUI.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isHolding)
        {
            playerNearby = true;
            if (interactionUI != null)
                interactionUI.SetActive(true);

            Debug.Log("👀 Topa yaklaşıldı.");
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

