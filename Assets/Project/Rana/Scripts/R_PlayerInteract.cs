using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Collections;

public class R_PlayerInteract : NetworkBehaviour
{
    [Header("References (Assign in Inspector)")]
    public Transform cameraHolder;
    public AudioSource audioSource;
    public AudioClip correctSound;
    public AudioClip wrongSound;
    public int correctTargetIndex;

    [Header("Swing Settings")]
    public float swingSpeed = 2f;
    public float swingAmplitude = 0.2f;

    // Runtime references
    private R_SwingController swingController;
    private R_PanelController panelController;
    private GameObject swingPromptUI;
    private GameObject swingExitPromptUI;

    private bool isOnSwing = false;
    private bool isNearSwing = false;
    private bool hasJustMountedSwing = false;
    private bool referencesInitialized = false;

    private float swingTime = 0f;
    private Vector3 initialCameraLocalPos;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        initialCameraLocalPos = cameraHolder.localPosition;
        StartCoroutine(InitializeReferencesCoroutine());
    }

    private IEnumerator InitializeReferencesCoroutine()
    {
        float timeout = 10f; // 10 saniye timeout
        float elapsedTime = 0f;

        while (!referencesInitialized && elapsedTime < timeout)
        {
            yield return new WaitForSeconds(0.1f);
            elapsedTime += 0.1f;

            TryInitializeReferences();
        }

        if (!referencesInitialized)
        {
            Debug.LogError("References could not be initialized within timeout period!");
        }
    }

    private void TryInitializeReferences()
    {
        // SwingController'ı bul
        if (swingController == null)
        {
            swingController = FindObjectOfType<R_SwingController>();
        }

        // PanelController'ı bul
        if (panelController == null)
        {
            panelController = FindObjectOfType<R_PanelController>();
        }

        // UI elementlerini bul
        if (swingPromptUI == null)
        {
            swingPromptUI = GameObject.FindGameObjectWithTag("SwingPromptUI");
        }

        if (swingExitPromptUI == null)
        {
            swingExitPromptUI = GameObject.FindGameObjectWithTag("SwingExitPromptUI");
        }

        // Tüm referanslar bulunduysa işaretle
        if (swingController != null && panelController != null &&
            swingPromptUI != null && swingExitPromptUI != null)
        {
            referencesInitialized = true;
            Debug.Log("All references initialized successfully!");
        }
    }

    void Update()
    {
        if (!IsOwner || !referencesInitialized) return;

        // UI güncellemeleri
        UpdateUI();

        // Input handling
        HandleInput();

        // Swing camera effect
        HandleSwingCameraEffect();

        // Reset mount flag
        if (hasJustMountedSwing)
            hasJustMountedSwing = false;
    }

    private void UpdateUI()
    {
        if (swingPromptUI != null)
            swingPromptUI.SetActive(isNearSwing && !isOnSwing);

        if (swingExitPromptUI != null && panelController != null)
            swingExitPromptUI.SetActive(isOnSwing && panelController.IsPuzzleComplete());
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!isOnSwing && isNearSwing)
            {
                MountSwing();
            }
            else if (isOnSwing && !hasJustMountedSwing && panelController != null)
            {
                TrySubmitAnswerServerRpc(OwnerClientId, panelController.GetCurrentLightIndex());
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape) && isOnSwing)
        {
            ExitSwing();
        }
    }

    private void HandleSwingCameraEffect()
    {
        if (isOnSwing)
        {
            swingTime += Time.deltaTime * swingSpeed;
            float yOffset = Mathf.Sin(swingTime) * swingAmplitude;
            cameraHolder.localPosition = initialCameraLocalPos + new Vector3(0, yOffset, 0);
        }
    }

    private void MountSwing()
    {
        isOnSwing = true;
        hasJustMountedSwing = true;

        if (swingController != null)
        {
            swingController.StartSwingServerRpc(OwnerClientId);
        }
    }

    private void ExitSwing()
    {
        isOnSwing = false;

        if (swingController != null)
        {
            swingController.StopSwingServerRpc(OwnerClientId);
        }

        cameraHolder.localPosition = initialCameraLocalPos;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;
        if (other.CompareTag("SwingTrigger"))
            isNearSwing = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsOwner) return;
        if (other.CompareTag("SwingTrigger"))
            isNearSwing = false;
    }

    [ServerRpc]
    void TrySubmitAnswerServerRpc(ulong clientId, int currentIndex)
    {
        // Server'da da referansları kontrol et
        if (panelController == null)
            panelController = FindObjectOfType<R_PanelController>();

        if (panelController == null)
        {
            Debug.LogError("PanelController not found on server!");
            return;
        }

        if (currentIndex == correctTargetIndex)
        {
            Debug.Log("✅ Doğru anda bastın!");
            panelController.GoToNextStep();
            PlaySoundClientRpc(true);
        }
        else
        {
            Debug.Log("❌ Yanlış!");
            PlaySoundClientRpc(false);
        }
    }

    [ClientRpc]
    void PlaySoundClientRpc(bool correct)
    {
        if (!IsOwner) return;

        if (audioSource != null)
        {
            if (correct && correctSound != null)
                audioSource.PlayOneShot(correctSound);
            else if (!correct && wrongSound != null)
                audioSource.PlayOneShot(wrongSound);
        }
    }

    // Public method to check if references are ready
    public bool AreReferencesReady()
    {
        return referencesInitialized;
    }
}