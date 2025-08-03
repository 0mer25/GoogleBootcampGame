using Unity.Netcode;
using UnityEngine;

public class KeypadTriggerZone : NetworkBehaviour
{
    [Header("Keypad Settings")]
    [SerializeField] private GameObject keypadPrefab; // Mevcut keypad prefab'ýnýz
    [SerializeField] private Transform cameraTransform; // Kamera referansý (opsiyonel)

    [Header("Door Animation")]
    [SerializeField] private Animator doorAnimator; // Kapý animatörü

    private GameObject activeKeypad;
    private bool keypadIsOpen = false;

    private void Start()
    {
        // Baþlangýçta keypad kapalý
    }

    private void Update()
    {
        // ESC ile kapatma - sadece keypad açýkken ve local player ise
        if (keypadIsOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            HideKeypadLocal();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Sadece local player tetikleyebilir
        var networkObject = other.GetComponent<NetworkObject>();
        if (networkObject != null && networkObject.IsOwner && other.CompareTag("Player"))
        {
            Debug.Log("Player entered keypad zone");
            ShowKeypadServerRpc(networkObject.OwnerClientId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ShowKeypadServerRpc(ulong clientId)
    {
        Debug.Log($"Server: Showing keypad for client {clientId}");
        ShowKeypadClientRpc(clientId);
    }

    [ClientRpc]
    private void ShowKeypadClientRpc(ulong clientId)
    {
        // Sadece belirtilen client keypad'i görür
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            Debug.Log("Client: Showing keypad locally");
            ShowKeypadLocal();
        }
    }

    private void ShowKeypadLocal()
    {
        if (keypadPrefab == null)
        {
            Debug.LogError("Keypad Prefab is not assigned!");
            return;
        }

        if (activeKeypad != null)
        {
            Debug.LogWarning("Keypad is already open!");
            return;
        }

        // Keypad prefab'ý instantiate et
        activeKeypad = Instantiate(keypadPrefab);
        activeKeypad.SetActive(true);
        keypadIsOpen = true;

        Debug.Log("Keypad instantiated and activated");

        // Kamera pozisyonuna yerleþtir
        PositionKeypadInFrontOfCamera();

        // Mouse cursor'u aç
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Keypad script'ine bu trigger zone'u referans ver
        var keypad = activeKeypad.GetComponentInChildren<NavKeypad.Keypad>();
        if (keypad != null)
        {
            keypad.SetTriggerZone(this);
            Debug.Log("Keypad script reference set");
        }
        else
        {
            Debug.LogWarning("Keypad script not found in prefab!");
        }

        // Player hareketini durdur
        DisablePlayerMovement();

        Debug.Log("Keypad opened successfully");
    }

    private void PositionKeypadInFrontOfCamera()
    {
        // Kamera referansýný bul
        Camera mainCamera = null;

        // Önce manuel atanmýþ kamera transformunu kontrol et
        if (cameraTransform != null)
        {
            mainCamera = cameraTransform.GetComponent<Camera>();
            if (mainCamera == null)
            {
                // Transform varsa ama kamera yoksa, kamerayý child'larda ara
                mainCamera = cameraTransform.GetComponentInChildren<Camera>();
            }
        }

        // Eðer hala kamera yoksa, ana kamerayý bul
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // Local player'ýn kamerasýný bul
        if (mainCamera == null)
        {
            var localPlayer = FindLocalPlayer();
            if (localPlayer != null)
            {
                mainCamera = localPlayer.GetComponentInChildren<Camera>();
            }
        }

        if (mainCamera != null)
        {
            // Canvas modu kontrolü
            Canvas keypadCanvas = activeKeypad.GetComponent<Canvas>();

            if (keypadCanvas != null)
            {
                if (keypadCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    // Screen Space Overlay - ekran merkezine koy
                    Debug.Log("Positioning keypad as Screen Space Overlay");
                    // Zaten ekranýn merkezinde olacak, ek pozisyonlama gerekmez
                }
                else if (keypadCanvas.renderMode == RenderMode.ScreenSpaceCamera)
                {
                    // Screen Space Camera - kamerayý ata
                    Debug.Log("Positioning keypad as Screen Space Camera");
                    keypadCanvas.worldCamera = mainCamera;
                    keypadCanvas.planeDistance = 0.5f;
                }
                else if (keypadCanvas.renderMode == RenderMode.WorldSpace)
                {
                    // World Space - kamera önüne yerleþtir
                    Debug.Log("Positioning keypad as World Space");
                    activeKeypad.transform.SetParent(mainCamera.transform);
                    activeKeypad.transform.localPosition = new Vector3(0, 0, 0.5f);
                    activeKeypad.transform.localRotation = Quaternion.identity;
                    activeKeypad.transform.localScale = Vector3.one * 0.001f; // Küçült
                }
            }
            else
            {
                // Canvas yoksa, direkt world space olarak kamera önüne koy
                Debug.Log("No canvas found, positioning as world object");
                activeKeypad.transform.SetParent(mainCamera.transform);
                activeKeypad.transform.localPosition = new Vector3(0, 0, 0.5f);
                activeKeypad.transform.localRotation = Quaternion.identity;
            }

            Debug.Log($"Keypad positioned relative to camera: {mainCamera.name}");
        }
        else
        {
            Debug.LogError("No camera found! Keypad positioning failed.");
        }
    }

    private CharacterMovement FindLocalPlayer()
    {
        var allPlayers = FindObjectsOfType<CharacterMovement>();
        foreach (var player in allPlayers)
        {
            if (player.IsOwner)
            {
                return player;
            }
        }
        return null;
    }

    private void DisablePlayerMovement()
    {
        // Local player'ý bul ve durdur
        var localPlayer = FindLocalPlayer();
        if (localPlayer != null)
        {
            localPlayer.enabled = false;

            // Animator'u durdur
            var animator = localPlayer.GetComponentInChildren<Animator>();
            if (animator != null)
            {
                animator.SetFloat("Speed", 0f);
                animator.SetFloat("MotionSpeed", 0f);
            }

            Debug.Log("Player movement disabled");
        }
    }

    private void EnablePlayerMovement()
    {
        // Local player'ý bul ve aktif et
        var localPlayer = FindLocalPlayer();
        if (localPlayer != null)
        {
            localPlayer.enabled = true;
            Debug.Log("Player movement enabled");
        }
    }

    public void HideKeypadLocal()
    {
        if (activeKeypad != null)
        {
            Destroy(activeKeypad);
            activeKeypad = null;
            Debug.Log("Keypad destroyed");
        }

        keypadIsOpen = false;

        // Mouse cursor'u gizle
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Player hareketini geri aç
        EnablePlayerMovement();

        Debug.Log("Keypad closed successfully");
    }

    // Keypad baþarýyla açýldýðýnda çalýþacak
    public void OnKeypadSuccess()
    {
        Debug.Log("Keypad success - notifying server");
        OnKeypadSuccessServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnKeypadSuccessServerRpc()
    {
        Debug.Log("Server: Keypad success received");
        OnKeypadSuccessClientRpc();
    }

    [ClientRpc]
    private void OnKeypadSuccessClientRpc()
    {
        Debug.Log("Client: Opening door");

        // Kapý animasyonu
        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger("Open");
        }
        else
        {
            Debug.LogWarning("Door animator not assigned!");
        }
    }
}