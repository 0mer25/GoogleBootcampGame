using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CharacterMovement : NetworkBehaviour
{
    [Header("Player")]
    [Tooltip("Move speed of the character in m/s")]
    public float MoveSpeed = 2.0f;

    [Tooltip("Sprint speed of the character in m/s")]
    public float SprintSpeed = 5.335f;

    [Tooltip("How fast the character turns to face movement direction")]
    [Range(0.0f, 0.3f)]
    public float RotationSmoothTime = 0.12f;

    [Tooltip("Acceleration and deceleration")]
    public float SpeedChangeRate = 10.0f;

    public float Sensitivity = 1f;

    public AudioClip LandingAudioClip;
    public AudioClip[] FootstepAudioClips;
    [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

    [Space(10)]
    [Tooltip("The height the player can jump")]
    public float JumpHeight = 1.2f;

    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    public float Gravity = -15.0f;

    [Space(10)]
    [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
    public float JumpTimeout = 0.50f;

    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
    public float FallTimeout = 0.15f;

    [Header("Player Grounded")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    public bool Grounded = true;

    [Tooltip("Useful for rough ground")]
    public float GroundedOffset = -0.14f;

    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float GroundedRadius = 0.28f;

    [Tooltip("What layers the character uses as ground")]
    public LayerMask GroundLayers;

    [Header("Camera Settings")]
    [Tooltip("Ana kamera objesi (karakterin çocuğu veya sahnede ana kamera).")]
    public Camera MainCamera;
    public Transform cameraRoot;


    [Tooltip("How far in degrees can you move the camera up")]
    public float TopClamp = 70.0f;

    [Tooltip("How far in degrees can you move the camera down")]
    public float BottomClamp = -30.0f;

    [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
    public float CameraAngleOverride = 0.0f;

    [Tooltip("For locking the camera position on all axis")]
    public bool LockCameraPosition = false;

    // camera
    private float _targetYaw;
    private float _targetPitch;

    // player
    private float _speed;
    private float _animationBlend;
    private float _targetRotation = 0.0f;
    private float _rotationVelocity;
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;

    // timeout deltatime
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    // animation IDs
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;

    private Animator _animator;
    private CharacterController _controller;
    private bool _rotateOnMove = true;

    private const float _threshold = 0.01f;

    // Klasik input değerleri
    private Vector2 _moveInput;
    private Vector2 _lookInput;
    private bool _jumpInput;
    private bool _sprintInput;


    private NetworkVariable<Quaternion> netRotation = new NetworkVariable<Quaternion>(
    writePerm: NetworkVariableWritePermission.Owner);
    public override void OnNetworkSpawn()
    {
        // Sadece owner'ın kamerasını aktif et
        if (IsOwner && MainCamera != null)
        {
            MainCamera.gameObject.SetActive(true);
        }
        else if (MainCamera != null)
        {
            MainCamera.gameObject.SetActive(false);
        }
        if (IsServer)
        {
            SpawnManager spawnManager = FindObjectOfType<SpawnManager>();
            if (spawnManager != null)
            {
                Vector3 spawnPosition = spawnManager.GetNextSpawnPosition();

                CharacterController cc = GetComponent<CharacterController>();
                if (cc != null)
                {
                    cc.enabled = false; // Karakteri hareket ettiren sistem geçici olarak kapatılmalı
                    transform.position = spawnPosition;
                    cc.enabled = true;
                }
                else
                {
                    transform.position = spawnPosition;
                }

                // 🔹 Client'lara pozisyonu bildir
                SetPositionClientRpc(spawnPosition);
            }
        }
    }
    [ClientRpc]
    void SetPositionClientRpc(Vector3 position)
    {
        if (!IsServer)
        {
            CharacterController cc = GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.enabled = false;
                transform.position = position;
                cc.enabled = true;
            }
            else
            {
                transform.position = position;
            }
        }
    }

    private void Awake()
    {
        if (MainCamera == null)
            MainCamera = GetComponentInChildren<Camera>(true);
    }

    private void Start()
    {
        
        _targetYaw = transform.rotation.eulerAngles.y;
        _targetPitch = 0f;

        _animator = GetComponentInChildren<Animator>();
        _controller = GetComponent<CharacterController>();

        AssignAnimationIDs();

        // reset our timeouts on start
        _jumpTimeoutDelta = JumpTimeout;
        _fallTimeoutDelta = FallTimeout;
    }

    private void Update()
    {
        if(!IsOwner) return;
        ReadClassicInputs();

        JumpAndGravity();
        GroundedCheck();
        Move();
        netRotation.Value = transform.rotation;
    }

    private void LateUpdate()
    {
        if (IsOwner)
        {
            CameraRotation();
        }
        else
        {
            // Owner değilsek, rotasyonu sync edilmiş değerden çek
            transform.rotation = netRotation.Value;
        }
    }

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }

    private void ReadClassicInputs()
    {
        // Hareket inputları
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        _moveInput = new Vector2(horizontal, vertical);

        // Mouse hareketi
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        _lookInput = new Vector2(mouseX, mouseY);

        // Diğer inputlar
        _jumpInput = Input.GetKey(KeyCode.Space);
        _sprintInput = Input.GetKey(KeyCode.LeftShift);
    }

    private void GroundedCheck()
    {
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
            transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
            QueryTriggerInteraction.Ignore);

        // update animator if using character
        if (_animator)
        {
            _animator.SetBool(_animIDGrounded, Grounded);
        }
    }

    private void CameraRotation()
    {
        if (LockCameraPosition) return;

        if (_lookInput.sqrMagnitude >= _threshold)
        {
            // Mouse sensitivity
            float deltaTimeMultiplier = 1.0f;

            _targetYaw += _lookInput.x * deltaTimeMultiplier * Sensitivity;
            _targetPitch -= _lookInput.y * deltaTimeMultiplier * Sensitivity; // Mouse Y ters (oyunlarda standart)
        }

        _targetYaw = ClampAngle(_targetYaw, float.MinValue, float.MaxValue);
        _targetPitch = ClampAngle(_targetPitch, BottomClamp, TopClamp);

        // Karakterin child'ı olan ana kameraya uygula (FPS benzeri)
        if (cameraRoot != null)
        {
            cameraRoot.localRotation = Quaternion.Euler(_targetPitch + CameraAngleOverride, 0.0f, 0.0f);
            transform.rotation = Quaternion.Euler(0.0f, _targetYaw, 0.0f);
        }
    }

    // CharacterMovement.cs içindeki Move() fonksiyonunda bu değişikliği yapın:

    private void Move()
    {
        if (!_controller.enabled)
            return;

        // set target speed based on move speed, sprint speed and if sprint is pressed
        float targetSpeed = _sprintInput ? SprintSpeed : MoveSpeed;

        // if there is no input, set the target speed to 0
        if (_moveInput == Vector2.zero) targetSpeed = 0.0f;

        // a reference to the players current horizontal velocity
        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

        float speedOffset = 0.1f;
        float inputMagnitude = 1f;
        if (_moveInput.magnitude > 1f) inputMagnitude = _moveInput.magnitude;

        // accelerate or decelerate to target speed
        if (currentHorizontalSpeed < targetSpeed - speedOffset ||
            currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                Time.deltaTime * SpeedChangeRate);

            // round speed to 3 decimal places
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }

        _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
        if (_animationBlend < 0.01f) _animationBlend = 0f;

        // Hareket yönünü kameraya göre belirle - NULL CHECK EKLE
        if (MainCamera != null)
        {
            Vector3 forward = MainCamera.transform.forward;
            Vector3 right = MainCamera.transform.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            Vector3 moveDirection = forward * _moveInput.y + right * _moveInput.x;
            moveDirection.Normalize();

            // Hareket uygula
            _controller.Move(moveDirection * (_speed * Time.deltaTime) +
                                new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            // update animator if using character
            if (_animator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, moveDirection.magnitude);
            }
        }
    }

    private void JumpAndGravity()
    {
        if (Grounded)
        {
            // reset fall timeout timer
            _fallTimeoutDelta = FallTimeout;

            // update animator if using character
            if (_animator)
            {
                _animator.SetBool(_animIDJump, false);
                _animator.SetBool(_animIDFreeFall, false);
            }

            // stop our velocity dropping infinitely when grounded
            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }

            // Jump
            if (_jumpInput && _jumpTimeoutDelta <= 0.0f)
            {
                // the square root of H * -2 * G = how much velocity needed to reach desired height
                _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                // update animator if using character
                if (_animator) _animator.SetBool(_animIDJump, true);
            }

            // jump timeout
            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            // reset the jump timeout timer
            _jumpTimeoutDelta = JumpTimeout;

            // fall timeout
            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                if (_animator)
                    _animator.SetBool(_animIDFreeFall, true);
            }

            // if we are not grounded, do not jump
            _jumpInput = false;
        }

        // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
        if (_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity += Gravity * Time.deltaTime;
        }
    }

    public void SetJumpState(bool isJumping)
    {
        if (_animator)
        {
            _animator.SetBool(_animIDJump, isJumping);
            _animator.SetBool(_animIDFreeFall, false);
        }

        if (!isJumping)
        {
            _jumpTimeoutDelta = JumpTimeout;
            _verticalVelocity = 0f;
            _jumpInput = false;
        }
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (Grounded) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;

        // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
        Gizmos.DrawSphere(
            new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
            GroundedRadius);
    }

    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            if (FootstepAudioClips.Length > 0)
            {
                var index = Random.Range(0, FootstepAudioClips.Length);
                AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }
    }

    private void OnLand(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
        }
    }

    public void SetSensitivity(float sensitivity)
    {
        Sensitivity = sensitivity;
    }
    public void SetRotateOnMove(bool newRotateOnMove)
    {
        _rotateOnMove = newRotateOnMove;
    }
    private void OnAnimatorMove()
    {

    }
}
