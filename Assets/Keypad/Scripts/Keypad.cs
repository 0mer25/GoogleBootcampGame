using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace NavKeypad
{
    public class Keypad : MonoBehaviour
    {
        [Header("Keypad Settings")]
        [SerializeField] private int keypadCombo = 1234;
        [SerializeField] private int maxDigits = 6;

        [Header("Display Settings")]
        [SerializeField] private string accessGrantedText = "ACCESS GRANTED";
        [SerializeField] private string accessDeniedText = "ACCESS DENIED";
        [SerializeField] private float displayResultTime = 2f;

        [Header("UI References")]
        [SerializeField] private TMP_Text keypadDisplayText;
        [SerializeField] private Renderer panelMesh; // Keypad ekranýnýn mesh renderer'ý (opsiyonel)

        [Header("Visual Effects")]
        [SerializeField] private Color screenNormalColor = Color.yellow;
        [SerializeField] private Color screenDeniedColor = Color.red;
        [SerializeField] private Color screenGrantedColor = Color.green;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip buttonClickedSfx;
        [SerializeField] private AudioClip accessDeniedSfx;
        [SerializeField] private AudioClip accessGrantedSfx;

        [Header("Events")]
        [SerializeField] private UnityEvent onAccessGranted;
        [SerializeField] private UnityEvent onAccessDenied;

        // Private variables
        private KeypadTriggerZone keypadZone;
        private string currentInput = "";
        private bool displayingResult = false;
        private bool accessWasGranted = false;

        private void Awake()
        {
            InitializeKeypad();
        }

        private void Start()
        {
            // Keyboard input handler'ý ekle
            var inputHandler = gameObject.AddComponent<KeypadInputHandler>();
            inputHandler.Initialize(this);
        }

        private void InitializeKeypad()
        {
            // Display'i temizle
            if (keypadDisplayText != null)
            {
                keypadDisplayText.text = "";
            }

            // Ekran rengini ayarla (eðer mesh renderer varsa)
            if (panelMesh != null)
            {
                panelMesh.material.SetColor("_EmissionColor", screenNormalColor);
            }

            // Audio source kontrol
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
            }

            Debug.Log("Keypad initialized successfully");
        }

        public void SetTriggerZone(KeypadTriggerZone zone)
        {
            keypadZone = zone;
            Debug.Log("Trigger zone reference set to keypad");
        }

        public void AddInput(string input)
        {
            if (displayingResult || accessWasGranted)
            {
                Debug.Log($"Input ignored: displayingResult={displayingResult}, accessWasGranted={accessWasGranted}");
                return;
            }

            Debug.Log($"Adding input: {input}");

            // Ses efekti çal
            PlayButtonSound();

            if (input == "enter")
            {
                CheckCombo();
                return;
            }

            if (input == "clear")
            {
                ClearInput();
                return;
            }

            // Maksimum digit kontrolü
            if (currentInput.Length >= maxDigits)
            {
                Debug.Log("Maximum digits reached");
                return;
            }

            // Input'u ekle
            currentInput += input;
            UpdateDisplay();

            Debug.Log($"Current input: {currentInput}");
        }

        private void UpdateDisplay()
        {
            if (keypadDisplayText != null)
            {
                // Girilen karakterleri göster (þifre için yýldýz da yapabilirsiniz)
                keypadDisplayText.text = currentInput;
            }
        }

        private void PlayButtonSound()
        {
            if (audioSource != null && buttonClickedSfx != null)
            {
                audioSource.PlayOneShot(buttonClickedSfx);
            }
        }

        private void CheckCombo()
        {
            Debug.Log($"Checking combo: '{currentInput}' vs '{keypadCombo}'");

            if (int.TryParse(currentInput, out var currentCombo))
            {
                bool correct = currentCombo == keypadCombo;
                Debug.Log($"Combo check result: {correct}");
                StartCoroutine(DisplayResult(correct));
            }
            else
            {
                Debug.LogError("Failed to parse current input as integer");
                StartCoroutine(DisplayResult(false));
            }
        }

        private IEnumerator DisplayResult(bool granted)
        {
            displayingResult = true;
            Debug.Log($"Displaying result: {granted}");

            if (granted)
                AccessGranted();
            else
                AccessDenied();

            yield return new WaitForSeconds(displayResultTime);

            if (granted)
            {
                // Baþarýlý giriþ durumunda keypad'i kapat
                if (keypadZone != null)
                {
                    keypadZone.HideKeypadLocal();
                }
                yield break;
            }

            // Baþarýsýz durumda sýfýrla
            ResetKeypad();
        }

        private void AccessGranted()
        {
            accessWasGranted = true;

            Debug.Log("Access granted!");

            // Display güncelle
            if (keypadDisplayText != null)
                keypadDisplayText.text = accessGrantedText;

            // Ekran rengini deðiþtir
            if (panelMesh != null)
                panelMesh.material.SetColor("_EmissionColor", screenGrantedColor);

            // Ses efekti
            if (audioSource != null && accessGrantedSfx != null)
                audioSource.PlayOneShot(accessGrantedSfx);

            // Event'i tetikle
            onAccessGranted?.Invoke();

            // Trigger zone'a baþarý durumunu bildir (multiplayer sync için)
            if (keypadZone != null)
            {
                keypadZone.OnKeypadSuccess();
            }
        }

        private void AccessDenied()
        {
            Debug.Log("Access denied!");

            // Display güncelle
            if (keypadDisplayText != null)
                keypadDisplayText.text = accessDeniedText;

            // Ekran rengini deðiþtir
            if (panelMesh != null)
                panelMesh.material.SetColor("_EmissionColor", screenDeniedColor);

            // Ses efekti
            if (audioSource != null && accessDeniedSfx != null)
                audioSource.PlayOneShot(accessDeniedSfx);

            // Event'i tetikle
            onAccessDenied?.Invoke();
        }

        private void ResetKeypad()
        {
            ClearInput();

            if (panelMesh != null)
                panelMesh.material.SetColor("_EmissionColor", screenNormalColor);

            displayingResult = false;

            Debug.Log("Keypad reset");
        }

        private void ClearInput()
        {
            currentInput = "";

            if (keypadDisplayText != null)
                keypadDisplayText.text = "";

            Debug.Log("Input cleared");
        }

        // Public method for manual clear button
        public void ClearInputManual()
        {
            if (!displayingResult && !accessWasGranted)
            {
                ClearInput();
            }
        }
    }
}