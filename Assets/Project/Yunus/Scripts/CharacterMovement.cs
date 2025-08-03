using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CharacterMovement : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 2.0f;
    public float sprintSpeed = 5.0f;
    public float mouseSensitivity = 2.0f;
    public float jumpHeight = 1.2f;
    public float gravity = -15.0f;

    [Header("Ground Check")]
    public float groundDistance = 0.4f;
    public LayerMask groundMask = 1; // Default layer
    public bool isGrounded;

    [Header("Camera")]
    public Transform cameraTransform;
    public float minLookAngle = -80f;
    public float maxLookAngle = 80f;

    [Header("Animation")]
    public Animator animator;

    [Header("Debug")]
    public bool showDebugLogs = false;

    // Private variables
    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation = 0f;

    // Animation IDs
    private int animIDSpeed;
    private int animIDGrounded;
    private int animIDJump;
    private int animIDFreeFall;
    private int animIDMotionSpeed;

    // Input variables
    private float horizontal;
    private float vertical;
    private float mouseX;
    private float mouseY;
    private bool isRunning;
    private bool jumpPressed;

    // Network sync
    private NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>();
    private NetworkVariable<Quaternion> networkRotation = new NetworkVariable<Quaternion>();

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        // Animator'ı bul
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        // Kamerayı bul
        if (cameraTransform == null)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null)
                cameraTransform = cam.transform;
        }

        // Animation ID'lerini ata
        AssignAnimationIDs();
    }

    private void AssignAnimationIDs()
    {
        animIDSpeed = Animator.StringToHash("Speed");
        animIDGrounded = Animator.StringToHash("Grounded");
        animIDJump = Animator.StringToHash("Jump");
        animIDFreeFall = Animator.StringToHash("FreeFall");
        animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }

    public override void OnNetworkSpawn()
    {
        // Sadece owner client'ın kamerasını aktif et
        if (cameraTransform != null)
        {
            Camera cam = cameraTransform.GetComponent<Camera>();
            if (cam != null)
            {
                cam.enabled = IsOwner;

                // Audio listener'ı da kontrol et
                AudioListener listener = cameraTransform.GetComponent<AudioListener>();
                if (listener != null)
                    listener.enabled = IsOwner;
            }
        }

        // Cursor'u kilitle (sadece owner için)
        if (IsOwner)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // Spawn pozisyonu ayarla
        if (IsServer)
        {
            SpawnAtRandomPosition();
        }
    }

    private void SpawnAtRandomPosition()
    {
        SpawnManager spawnManager = FindObjectOfType<SpawnManager>();
        if (spawnManager != null)
        {
            Vector3 spawnPos = spawnManager.GetNextSpawnPosition();

            // CharacterController'ı geçici olarak kapat
            controller.enabled = false;
            transform.position = spawnPos;
            controller.enabled = true;

            // Network pozisyonu güncelle
            networkPosition.Value = spawnPos;

            // Client'lara bildir
            SetPositionClientRpc(spawnPos);
        }
    }

    [ClientRpc]
    private void SetPositionClientRpc(Vector3 position)
    {
        if (!IsServer)
        {
            controller.enabled = false;
            transform.position = position;
            controller.enabled = true;
        }
    }

    private void Update()
    {
        // Sadece owner hareket edebilir
        if (!IsOwner)
        {
            // Diğer client'lar için pozisyon senkronizasyonu
            if (Vector3.Distance(transform.position, networkPosition.Value) > 0.1f)
            {
                transform.position = Vector3.Lerp(transform.position, networkPosition.Value, Time.deltaTime * 10f);
            }
            transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation.Value, Time.deltaTime * 10f);
            return;
        }

        HandleInput();
        HandleMouseLook();
        HandleMovement();
        HandleJump();

        // Network değişkenlerini güncelle
        if (IsServer)
        {
            networkPosition.Value = transform.position;
            networkRotation.Value = transform.rotation;
        }
        else
        {
            // Client ise server'a pozisyon gönder
            UpdatePositionServerRpc(transform.position, transform.rotation);
        }

        // Debug
        if (showDebugLogs)
        {
            Debug.Log($"Movement Input: H:{horizontal}, V:{vertical} | Speed: {controller.velocity.magnitude:F2}");
        }
    }

    [ServerRpc]
    private void UpdatePositionServerRpc(Vector3 position, Quaternion rotation)
    {
        networkPosition.Value = position;
        networkRotation.Value = rotation;
    }

    private void HandleInput()
    {
        // WASD veya Arrow keys
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");

        // Mouse
        mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Diğer inputlar
        isRunning = Input.GetKey(KeyCode.LeftShift);
        jumpPressed = Input.GetKeyDown(KeyCode.Space);

        // ESC ile cursor'u serbest bırak
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    private void HandleMouseLook()
    {
        if (cameraTransform == null || Cursor.lockState != CursorLockMode.Locked)
            return;

        // Yatay döndürme (karakter)
        transform.Rotate(Vector3.up * mouseX);

        // Dikey döndürme (kamera)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minLookAngle, maxLookAngle);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    private void HandleMovement()
    {
        // Ground check
        isGrounded = Physics.CheckSphere(transform.position, groundDistance, groundMask);

        // Animator'ü güncelle - Grounded
        if (animator)
        {
            animator.SetBool(animIDGrounded, isGrounded);
        }

        // Hareket yönü hesapla
        Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;
        moveDirection.Normalize();

        // Hız belirle
        float currentSpeed = isRunning ? sprintSpeed : walkSpeed;
        float targetSpeed = moveDirection.magnitude * currentSpeed;

        // Hareketi uygula
        controller.Move(moveDirection * currentSpeed * Time.deltaTime);

        // Animator'ü güncelle - Speed ve Motion
        if (animator)
        {
            animator.SetFloat(animIDSpeed, targetSpeed);
            animator.SetFloat(animIDMotionSpeed, moveDirection.magnitude);
        }

        if (showDebugLogs && moveDirection.magnitude > 0)
        {
            Debug.Log($"Move Direction: {moveDirection} | Speed: {currentSpeed} | Animation Speed: {targetSpeed}");
        }
    }

    private void HandleJump()
    {
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Küçük negatif değer (grounded kalması için)

            // Animator'ü güncelle
            if (animator)
            {
                animator.SetBool(animIDJump, false);
                animator.SetBool(animIDFreeFall, false);
            }
        }

        if (jumpPressed && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            // Jump animasyonu
            if (animator)
            {
                animator.SetBool(animIDJump, true);
            }
        }

        // Gravity uygula
        velocity.y += gravity * Time.deltaTime;

        // FreeFall animasyonu
        if (!isGrounded && velocity.y < -1f)
        {
            if (animator)
            {
                animator.SetBool(animIDFreeFall, true);
                animator.SetBool(animIDJump, false);
            }
        }

        // Vertical hareketi uygula
        controller.Move(velocity * Time.deltaTime);
    }

    private void OnGUI()
    {
        if (!IsOwner) return;

        GUI.Label(new Rect(10, 10, 300, 20), $"IsOwner: {IsOwner}");
        GUI.Label(new Rect(10, 30, 300, 20), $"Input H:{horizontal:F2} V:{vertical:F2}");
        GUI.Label(new Rect(10, 50, 300, 20), $"Speed: {controller.velocity.magnitude:F2}");
        GUI.Label(new Rect(10, 70, 300, 20), $"Grounded: {isGrounded}");
        GUI.Label(new Rect(10, 90, 300, 20), $"Animator: {(animator != null ? "Found" : "Missing")}");
    }

    // Animation Event handlers (karakterin animasyonlarından çağrılır)
    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            // Ayak sesi çalma kodu burada olacak
            if (showDebugLogs)
                Debug.Log("Footstep!");
        }
    }

    private void OnLand(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            // Yere düşme sesi çalma kodu burada olacak
            if (showDebugLogs)
                Debug.Log("Landed!");
        }
    }

    // Public metodlar
    public void SetMouseSensitivity(float sensitivity)
    {
        mouseSensitivity = sensitivity;
    }

    public void SetMovementSpeed(float walkSpd, float sprintSpd)
    {
        walkSpeed = walkSpd;
        sprintSpeed = sprintSpd;
    }

    private void OnDrawGizmosSelected()
    {
        // Ground check sphere'ini göster
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, groundDistance);
    }
}