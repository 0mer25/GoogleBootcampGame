using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace NavKeypad
{
    public class Keypad : MonoBehaviour
    {
        [Header("Events")]
        [SerializeField] private UnityEvent onAccessGranted;
        [SerializeField] private UnityEvent onAccessDenied;

        [SerializeField] private int keypadCombo = 1234;

        [Header("Settings")]
        [SerializeField] private string accessGrantedText = "Granted";
        [SerializeField] private string accessDeniedText = "Denied";

        [Header("Visuals")]
        [SerializeField] private float displayResultTime = 1f;
        [SerializeField] private Renderer panelMesh;
        [SerializeField] private TMP_Text keypadDisplayText;

        [Header("Colors")]
        [SerializeField] private Color screenNormalColor = Color.yellow;
        [SerializeField] private Color screenDeniedColor = Color.red;
        [SerializeField] private Color screenGrantedColor = Color.green;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip buttonClickedSfx;
        [SerializeField] private AudioClip accessDeniedSfx;
        [SerializeField] private AudioClip accessGrantedSfx;

        [Header("Manager Reference")]
        [SerializeField] private KeypadTriggerZone keypadZone;

        [SerializeField] private Animator doorAnimator;

        private string currentInput = "";
        private bool displayingResult = false;
        private bool accessWasGranted = false;

        private void Awake()
        {
            keypadDisplayText.text = "";
            panelMesh.material.SetColor("_EmissionColor", screenNormalColor);
        }

        public void AddInput(string input)
        {
            if (displayingResult || accessWasGranted) return;

            audioSource.PlayOneShot(buttonClickedSfx);

            if (input == "enter")
            {
                CheckCombo();
                return;
            }

            if (currentInput.Length >= 9) return;

            currentInput += input;
            keypadDisplayText.text = currentInput;
        }

        private void CheckCombo()
        {
            if (int.TryParse(currentInput, out var currentCombo))
            {
                bool correct = currentCombo == keypadCombo;
                StartCoroutine(DisplayResult(correct));
            }
        }

        private IEnumerator DisplayResult(bool granted)
        {
            displayingResult = true;

            if (granted) AccessGranted();
            else AccessDenied();

            yield return new WaitForSeconds(displayResultTime);

            if (granted) yield break;

            ClearInput();
            panelMesh.material.SetColor("_EmissionColor", screenNormalColor);
            displayingResult = false;
        }

        private void AccessGranted()
        {
            accessWasGranted = true;
            keypadDisplayText.text = accessGrantedText;
            panelMesh.material.SetColor("_EmissionColor", screenGrantedColor);
            audioSource.PlayOneShot(accessGrantedSfx);
            onAccessGranted?.Invoke();

            doorAnimator?.SetTrigger("Open"); // << Animasyonu tetikle

            keypadZone?.HideKeypad(); // ekraný kapat
        }


        private void AccessDenied()
        {
            keypadDisplayText.text = accessDeniedText;
            panelMesh.material.SetColor("_EmissionColor", screenDeniedColor);
            audioSource.PlayOneShot(accessDeniedSfx);
            onAccessDenied?.Invoke();
        }

        private void ClearInput()
        {
            currentInput = "";
            keypadDisplayText.text = "";
        }
    }
}
