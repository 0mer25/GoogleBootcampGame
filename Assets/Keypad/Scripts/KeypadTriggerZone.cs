using UnityEngine;

public class KeypadTriggerZone : MonoBehaviour
{
    [SerializeField] private GameObject keypadUI;
    [SerializeField] private Transform cameraTransform;
    public GameObject targetObject;

    private bool keypadIsOpen = false;

    private void Start()
    {
        keypadUI.SetActive(false);
    }

    private void Update()
    {
        // ESC ile kapatma
        if (keypadIsOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            HideKeypad();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ShowKeypad();
        }
    }

    private void ShowKeypad()
    {
        keypadUI.SetActive(true);
        keypadIsOpen = true;

        // Kamera önüne yerleþtir
        keypadUI.transform.SetParent(cameraTransform);
        keypadUI.transform.localPosition = new Vector3(0, 0, 0.5f);
        keypadUI.transform.localRotation = Quaternion.identity;

        // Mouse imleci aç
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Keypad interaction sistemine set et
        var interaction = Camera.main.GetComponent<NavKeypad.KeypadInteractionFPV>();
        if (interaction != null)
        {
            var keypad = keypadUI.GetComponentInChildren<NavKeypad.Keypad>();
            if (keypad != null)
            {
                interaction.SetActiveKeypad(keypad);
            }
        }

        // Oyuncu hareketini devre dýþý býrak
        if (targetObject != null)
        {
            var moveScript = targetObject.GetComponent<PlayerMovement>();
            if (moveScript != null)
                moveScript.enabled = false;
        }
    }

    public void HideKeypad()
    {
        keypadUI.SetActive(false);
        keypadIsOpen = false;

        keypadUI.transform.SetParent(null);

        // Mouse imleci gizle
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Keypad interaction sýfýrla
        var interaction = Camera.main.GetComponent<NavKeypad.KeypadInteractionFPV>();
        if (interaction != null)
        {
            interaction.SetActiveKeypad(null);
        }

        // Oyuncu hareketini geri aç
        if (targetObject != null)
        {
            var moveScript = targetObject.GetComponent<PlayerMovement>();
            if (moveScript != null)
                moveScript.enabled = true;
        }
    }
}
