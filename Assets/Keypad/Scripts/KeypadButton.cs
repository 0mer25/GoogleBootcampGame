using System.Collections;
using UnityEngine;

namespace NavKeypad
{
    public class KeypadButton : MonoBehaviour
    {
        [Header("Button Settings")]
        [SerializeField] private string value; // "0", "1", "2", ..., "9", "enter", "clear"

        [Header("Animation Settings")]
        [SerializeField] private float moveDist = 0.005f;
        [SerializeField] private float pressSpeed = 0.1f;

        [Header("References")]
        [SerializeField] private Keypad keypad; // Manuel olarak atanabilir

        private bool moving = false;
        private Collider buttonCollider;

        private void Awake()
        {
            // Collider referansýný al
            buttonCollider = GetComponent<Collider>();

            // Eðer keypad referansý yoksa parent'tan bul
            if (keypad == null)
            {
                keypad = GetComponentInParent<Keypad>();
            }

            Debug.Log($"KeypadButton initialized: {value}");
        }

        // Mouse click ile tetikleme
        private void OnMouseDown()
        {
            if (CanPress())
            {
                PressButton();
            }
        }

        // Collision ile tetikleme (3D etkileþim için)
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && CanPress())
            {
                PressButton();
            }
        }

        // Public method - dýþarýdan çaðrýlabilir
        public void PressButton()
        {
            if (!CanPress()) return;

            Debug.Log($"Button pressed: {value}");

            // Keypad'e input gönder
            if (keypad != null)
            {
                keypad.AddInput(value);
            }

            // Animasyonu baþlat
            StartCoroutine(ButtonAnimation());
        }

        private bool CanPress()
        {
            return !moving && keypad != null;
        }

        private IEnumerator ButtonAnimation()
        {
            moving = true;

            // Baþlangýç pozisyonu
            Vector3 startPos = transform.localPosition;
            Vector3 endPos = startPos + new Vector3(0, 0, -moveDist); // Z ekseninde geriye hareket

            // Aþaðý hareket
            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime / pressSpeed;
                transform.localPosition = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }

            // Kýsa bekleme
            yield return new WaitForSeconds(0.05f);

            // Yukarý hareket
            t = 0;
            while (t < 1)
            {
                t += Time.deltaTime / pressSpeed;
                transform.localPosition = Vector3.Lerp(endPos, startPos, t);
                yield return null;
            }

            // Tam pozisyona ayarla
            transform.localPosition = startPos;
            moving = false;
        }

        // Inspector'da deðer ayarlama için
        public void SetValue(string buttonValue)
        {
            value = buttonValue;
        }

        // Keypad referansýný manuel olarak ayarlama
        public void SetKeypad(Keypad targetKeypad)
        {
            keypad = targetKeypad;
        }

        // Debug için
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(value))
            {
                Debug.LogWarning($"Button value is not set for {gameObject.name}");
            }
        }
    }
}