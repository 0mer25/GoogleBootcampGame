using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 720f;

    [Header("Head Rotation & Shooting")]
    public Transform headTransform;
    public Transform projectileSpawnPoint;
    public GameObject projectilePrefab;
    public float projectileSpeed = 20f;

    [Header("Ammo & Reload")]
    public int magazineSize = 6;
    public float reloadDuration = 2f;

    [Header("Dash")]
    public float dashDistance = 5f;
    public float dashCooldown = 2f;
    public float dashDuration = 0.15f;
    private bool isDashing = false;
    private bool dashOnCooldown = false;

    private int currentAmmo;
    private bool isReloading = false;

    private CharacterController controller;
    private Camera cam;
    private Vector3 moveDirection;
    [SerializeField] private Animator animator;
    private bool isAimingEnabled = true;
    [SerializeField] private ParticleSystem shootEffect;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        cam = Camera.main;
        currentAmmo = magazineSize;
    }

    void Update()
    {
        if (isDashing) return;

        MoveAndRotateBody();

        if (isAimingEnabled)
            AimHeadToMouse();

        if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(Reload());
            return;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            TryDash();
        }

        if(isReloading)
            return;

        HandleShooting();
    }

    void MoveAndRotateBody()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        moveDirection = new Vector3(h, 0, v).normalized;

        if (moveDirection.magnitude >= 0.1f)
        {
            controller.SimpleMove(moveDirection * moveSpeed);

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        bool isMoving = moveDirection.magnitude >= 0.1f;
        animator.SetBool("isMoving", isMoving);
    }

    void AimHeadToMouse()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, LayerMask.GetMask("Default")))
        {
            Vector3 lookDirection = hit.point - headTransform.position;
            lookDirection.y = 0;

            if (lookDirection.magnitude > 0.01f)
            {
                Quaternion lookRot = Quaternion.LookRotation(lookDirection);
                headTransform.rotation = Quaternion.RotateTowards(headTransform.rotation, lookRot, rotationSpeed * Time.deltaTime);
            }
        }
    }

    void HandleShooting()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (currentAmmo <= 0)
            {
                Debug.Log("Şarjör boş!");
                return;
            }

            Shoot();
        }
    }

    void Shoot()
    {
        shootEffect?.Play();
        
        GameObject proj = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);
        Rigidbody rb = proj.GetComponent<Rigidbody>();
        rb.linearVelocity = projectileSpawnPoint.forward * projectileSpeed;

        currentAmmo--;
        Debug.Log($"Ateş edildi. Kalan mermi: {currentAmmo}");

        if (currentAmmo <= 0)
        {
            CameraShaker.Instance.Shake(.3f, .1f);
            StartCoroutine(Reload());
        }
    }

    IEnumerator Reload()
    {
        Debug.Log("Yeniden dolduruluyor...");
        isReloading = true;
        yield return new WaitForSeconds(reloadDuration);
        currentAmmo = magazineSize;
        isReloading = false;
        Debug.Log("Şarjör doldu!");
    }

    void TryDash()
    {
        if (dashOnCooldown || moveDirection == Vector3.zero)
            return;

        StartCoroutine(DashCoroutine());
    }

    IEnumerator DashCoroutine()
    {
        isDashing = true;
        dashOnCooldown = true;
        isAimingEnabled = false;
    
        // Önce karakteri hareket yönüne döndür
        if (moveDirection != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = lookRotation;
        }
    
        // Roll animasyonu
        animator.SetTrigger("roll");
    
        // Dash hareketi
        float startTime = Time.time;
        Vector3 dashVelocity = moveDirection * (dashDistance / dashDuration);
    
        while (Time.time < startTime + dashDuration)
        {
            controller.Move(dashVelocity * Time.deltaTime);
            yield return null;
        }
    
        isDashing = false;
        isAimingEnabled = true;
    
        yield return new WaitForSeconds(dashCooldown);
        dashOnCooldown = false;
    }
}
