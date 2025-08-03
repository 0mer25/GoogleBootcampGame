using Unity.Netcode;
using UnityEngine;

public class KeypadTriggerZone : NetworkBehaviour
{
    [Header("Keypad Settings")]
    [SerializeField] private GameObject keypadPrefab; // Mevcut keypad prefab'�n�z
    [SerializeField] private Transform cameraTransform; // Kamera referans� (opsiyonel)

    [Header("Door Animation")]
    [SerializeField] private Animator doorAnimator; // Kap� animat�r�

    [Header("Debug Settings")]
    [SerializeField] private bool enableDebugLogs = true; // Debug loglar� a��p kapatmak i�in

    private GameObject activeKeypad;
    private bool keypadIsOpen = false;

    private void Start()
    {
        // Ba�lang��ta keypad kapal�
        DebugLog("KeypadTriggerZone initialized");

        // Prefab kontrol�
        if (keypadPrefab == null)
        {
            Debug.LogError("Keypad Prefab is not assigned in KeypadTriggerZone!");
        }
    }

    private void Update()
    {
        // ESC ile kapatma - sadece keypad a��kken ve local player ise
        if (keypadIsOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            DebugLog("ESC pressed - closing keypad");
            HideKeypadLocal();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        DebugLog($"Trigger entered by: {other.name}, Tag: {other.tag}");

        // Player tag kontrol�
        if (!other.CompareTag("Player"))
        {
            DebugLog("Not a player - ignoring");
            return;
        }

        // NetworkObject kontrol�
        var networkObject = other.GetComponent<NetworkObject>();
        if (networkObject == null)
        {
            DebugLog("No NetworkObject found on player");
            return;
        }

        // Owner kontrol�
        if (!networkObject.IsOwner)
        {
            DebugLog("Not the owner of this player object");
            return;
        }

        DebugLog($"Valid player entered keypad zone. Client ID: {networkObject.OwnerClientId}");

        // E�er zaten keypad a��ksa yeni bir tane a�ma
        if (keypadIsOpen)
        {
            DebugLog("Keypad is already open");
            return;
        }

        ShowKeypadServerRpc(networkObject.OwnerClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ShowKeypadServerRpc(ulong clientId)
    {
        DebugLog($"Server: Showing keypad for client {clientId}");
        ShowKeypadClientRpc(clientId);
    }

    [ClientRpc]
    private void ShowKeypadClientRpc(ulong clientId)
    {
        DebugLog($"Client RPC received. Local Client ID: {NetworkManager.Singleton.LocalClientId}, Target Client ID: {clientId}");

        // Sadece belirtilen client keypad'i g�r�r
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            DebugLog("This client should show the keypad");
            ShowKeypadLocal();
        }
        else
        {
            DebugLog("This client should NOT show the keypad");
        }
    }

    private void ShowKeypadLocal()
    {
        DebugLog("ShowKeypadLocal called");

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

        DebugLog("Creating keypad instance...");

        // Keypad prefab'� instantiate et
        activeKeypad = Instantiate(keypadPrefab);

        if (activeKeypad == null)
        {
            Debug.LogError("Failed to instantiate keypad prefab!");
            return;
        }

        DebugLog($"Keypad instantiated: {activeKeypad.name}");

        // Keypad'i aktif et
        activeKeypad.SetActive(true);
        keypadIsOpen = true;

        DebugLog("Keypad set to active");

        // Kamera pozisyonuna yerle�tir
        PositionKeypadInFrontOfCamera();

        // Mouse cursor'u a�
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        DebugLog("Cursor unlocked and made visible");

        // Keypad script'ine bu trigger zone'u referans ver
        var keypad = activeKeypad.GetComponentInChildren<NavKeypad.Keypad>();
        if (keypad != null)
        {
            keypad.SetTriggerZone(this);
            DebugLog("Keypad script reference set");
        }
        else
        {
            // Root'ta da kontrol et
            keypad = activeKeypad.GetComponent<NavKeypad.Keypad>();
            if (keypad != null)
            {
                keypad.SetTriggerZone(this);
                DebugLog("Keypad script found on root and reference set");
            }
            else
            {
                Debug.LogWarning("Keypad script not found in prefab! Checking all children...");

                // T�m children'lar� kontrol et
                var allKeypadScripts = activeKeypad.GetComponentsInChildren<NavKeypad.Keypad>();
                DebugLog($"Found {allKeypadScripts.Length} keypad scripts in children");

                if (allKeypadScripts.Length > 0)
                {
                    allKeypadScripts[0].SetTriggerZone(this);
                    DebugLog("First keypad script found and reference set");
                }
            }
        }

        // Player hareketini durdur
        DisablePlayerMovement();

        DebugLog("Keypad opened successfully");
    }

    private void PositionKeypadInFrontOfCamera()
    {
        DebugLog("Positioning keypad in front of camera...");

        // Kamera referans�n� bul
        Camera mainCamera = null;

        // �nce manuel atanm�� kamera transformunu kontrol et
        if (cameraTransform != null)
        {
            mainCamera = cameraTransform.GetComponent<Camera>();
            if (mainCamera == null)
            {
                // Transform varsa ama kamera yoksa, kameray� child'larda ara
                mainCamera = cameraTransform.GetComponentInChildren<Camera>();
            }
        }

        // E�er hala kamera yoksa, ana kameray� bul
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // Local player'�n kameras�n� bul
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
            DebugLog($"Camera found: {mainCamera.name}");

            // Canvas modu kontrol�
            Canvas keypadCanvas = activeKeypad.GetComponent<Canvas>();

            if (keypadCanvas != null)
            {
                DebugLog($"Canvas found with render mode: {keypadCanvas.renderMode}");

                if (keypadCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    // Screen Space Overlay - ekran merkezine koy
                    DebugLog("Positioning keypad as Screen Space Overlay");
                    // Zaten ekran�n merkezinde olacak, ek pozisyonlama gerekmez
                }
                else if (keypadCanvas.renderMode == RenderMode.ScreenSpaceCamera)
                {
                    // Screen Space Camera - kameray� ata
                    DebugLog("Positioning keypad as Screen Space Camera");
                    keypadCanvas.worldCamera = mainCamera;
                    keypadCanvas.planeDistance = 0.5f;
                }
                else if (keypadCanvas.renderMode == RenderMode.WorldSpace)
                {
                    // World Space - kamera �n�ne yerle�tir
                    DebugLog("Positioning keypad as World Space");
                    activeKeypad.transform.SetParent(mainCamera.transform);
                    activeKeypad.transform.localPosition = new Vector3(0, 0, 0.5f);
                    activeKeypad.transform.localRotation = Quaternion.identity;
                    activeKeypad.transform.localScale = Vector3.one * 0.001f; // K���lt
                }
            }
            else
            {
                // Canvas yoksa, direkt world space olarak kamera �n�ne koy
                DebugLog("No canvas found, positioning as world object");
                activeKeypad.transform.SetParent(mainCamera.transform);
                activeKeypad.transform.localPosition = new Vector3(0, 0, 0.5f);
                activeKeypad.transform.localRotation = Quaternion.identity;

                // Scale'i kontrol et - �ok k���kse g�r�nmez
                if (activeKeypad.transform.localScale.magnitude < 0.01f)
                {
                    activeKeypad.transform.localScale = Vector3.one * 0.1f;
                    DebugLog("Scale adjusted for visibility");
                }
            }

            DebugLog($"Keypad positioned relative to camera: {mainCamera.name}");
        }
        else
        {
            Debug.LogError("No camera found! Keypad positioning failed.");

            // Kamera bulunamazsa varsay�lan pozisyonda b�rak
            DebugLog("Using default world position for keypad");
        }
    }

    private CharacterMovement FindLocalPlayer()
    {
        var allPlayers = FindObjectsOfType<CharacterMovement>();
        DebugLog($"Found {allPlayers.Length} CharacterMovement components");

        foreach (var player in allPlayers)
        {
            if (player.IsOwner)
            {
                DebugLog($"Local player found: {player.name}");
                return player;
            }
        }

        DebugLog("No local player found");
        return null;
    }

    private void DisablePlayerMovement()
    {
        // Local player'� bul ve durdur
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

            DebugLog("Player movement disabled");
        }
        else
        {
            DebugLog("Could not find local player to disable movement");
        }
    }

    private void EnablePlayerMovement()
    {
        // Local player'� bul ve aktif et
        var localPlayer = FindLocalPlayer();
        if (localPlayer != null)
        {
            localPlayer.enabled = true;
            DebugLog("Player movement enabled");
        }
        else
        {
            DebugLog("Could not find local player to enable movement");
        }
    }

    public void HideKeypadLocal()
    {
        DebugLog("HideKeypadLocal called");

        if (activeKeypad != null)
        {
            Destroy(activeKeypad);
            activeKeypad = null;
            DebugLog("Keypad destroyed");
        }

        keypadIsOpen = false;

        // Mouse cursor'u gizle
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Player hareketini geri a�
        EnablePlayerMovement();

        DebugLog("Keypad closed successfully");
    }

    // Keypad ba�ar�yla a��ld���nda �al��acak
    public void OnKeypadSuccess()
    {
        DebugLog("Keypad success - notifying server");
        OnKeypadSuccessServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnKeypadSuccessServerRpc()
    {
        DebugLog("Server: Keypad success received");
        OnKeypadSuccessClientRpc();
    }

    [ClientRpc]
    private void OnKeypadSuccessClientRpc()
    {
        DebugLog("Client: Opening door");

        // Kap� animasyonu
        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger("Open");
        }
        else
        {
            Debug.LogWarning("Door animator not assigned!");
        }
    }

    private void DebugLog(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[KeypadTriggerZone] {message}");
        }
    }

    // Inspector'da test etmek i�in
    [ContextMenu("Test Show Keypad")]
    private void TestShowKeypad()
    {
        if (Application.isPlaying)
        {
            ShowKeypadLocal();
        }
    }

    [ContextMenu("Test Hide Keypad")]
    private void TestHideKeypad()
    {
        if (Application.isPlaying)
        {
            HideKeypadLocal();
        }
    }
}