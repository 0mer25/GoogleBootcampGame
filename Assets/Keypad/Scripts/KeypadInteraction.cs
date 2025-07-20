using UnityEngine;

public class KeypadInteraction : MonoBehaviour
{
    [SerializeField] private GameObject keypadUI; // ekran önünde gösterilecek versiyon
    [SerializeField] private Transform cameraTransform;
    

    private void Start()
    {
        keypadUI.SetActive(false);
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
        keypadUI.transform.SetParent(cameraTransform);
        keypadUI.transform.localPosition = new Vector3(0, 0, 1.2f); // 1.2 metre önünde
        keypadUI.transform.localRotation = Quaternion.identity;
        
    }

    public void HideKeypad()
    {
        keypadUI.SetActive(false);
        keypadUI.transform.SetParent(null); // veya world'e alýn
       
    }
}