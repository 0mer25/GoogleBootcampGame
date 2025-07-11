using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Y_PlayerMovementController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float crouchSpeed = 2f;
    public float sprintSpeed = 8f;
    public float jumpForce = 5f;

    [Header("Camera Settings")]
    public Transform cameraRoot;
    public float mouseSensitivity = 2f;
    public float minPitch = -80f;
    public float maxPitch = 80f;

    [Header("Crouch Settings")]
    public float crouchHeight = 1f;
    public float standHeight = 2f;
    public float crouchSmooth = 10f;

    [Header("Headbob Settings")]
    public float headbobSpeed = 10f;
    public float headbobAmount = 0.05f;
    public float sprintBobMultiplier = 1.5f;
    public float crouchBobMultiplier = 0.5f;

    private Rigidbody rb;
    private float pitch = 0f;
    private CapsuleCollider col;
    private float originalHeight;

    private bool isGrounded = false;

    private float bobTimer = 0f;
    private Vector3 cameraRootDefaultLocalPos;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        originalHeight = col.height;
        Cursor.lockState = CursorLockMode.Locked;
        cameraRootDefaultLocalPos = cameraRoot.localPosition;
    }

    void Update()
    {
        HandleMouseLook();
        HandleCrouch();
        HandleJump();
        HandleHeadbob();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        cameraRoot.localRotation = Quaternion.Euler(pitch, 0, 0);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        move.Normalize();

        float currentSpeed = moveSpeed;

        if (IsCrouching())
            currentSpeed = crouchSpeed;
        else if (IsSprinting())
            currentSpeed = sprintSpeed;

        Vector3 velocity = new Vector3(move.x * currentSpeed, rb.linearVelocity.y, move.z * currentSpeed);
        rb.linearVelocity = velocity;
    }

    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z); // reset Y
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void HandleCrouch()
    {
        float targetHeight = IsCrouching() ? crouchHeight : standHeight;
        col.height = Mathf.Lerp(col.height, targetHeight, Time.deltaTime * crouchSmooth);
    }
    void HandleHeadbob()
    {
        bool isMoving = rb.linearVelocity.magnitude > 0.1f && isGrounded;

        if (isMoving)
        {
            float speedMultiplier = 1f;
            if (IsSprinting()) speedMultiplier = sprintBobMultiplier;
            else if (IsCrouching()) speedMultiplier = crouchBobMultiplier;

            bobTimer += Time.deltaTime * headbobSpeed * speedMultiplier;

            float bobX = Mathf.Sin(bobTimer) * headbobAmount;
            float bobY = Mathf.Cos(bobTimer * 2f) * headbobAmount;

            cameraRoot.localPosition = cameraRootDefaultLocalPos + new Vector3(bobX, bobY, 0);
        }
        else
        {
            bobTimer = 0f;
            cameraRoot.localPosition = Vector3.Lerp(cameraRoot.localPosition, cameraRootDefaultLocalPos, Time.deltaTime * 5f);
        }
    }

    bool IsCrouching()
    {
        return Input.GetKey(KeyCode.LeftControl);
    }

    bool IsSprinting()
    {
        return Input.GetKey(KeyCode.LeftShift) && !IsCrouching() && rb.linearVelocity.magnitude > 0.1f;
    }

    void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (Vector3.Angle(contact.normal, Vector3.up) < 45f)
            {
                isGrounded = true;
                return;
            }
        }
        isGrounded = false;
    }

    void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }
}
