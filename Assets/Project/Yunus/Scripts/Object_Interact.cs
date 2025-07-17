using UnityEngine;

public class Object_Interact : MonoBehaviour
{
    [Header("Drag your player here (with Y_PlayerMovementController)")]
    public GameObject targetObject; // Player objeni Inspector�dan atayacaks�n

    [Header("Object will move here when examining")]
    public GameObject offset;

    [Header("Set your examine area/table here (optional)")]
    public GameObject tableObject;

    [Header("Assign your Canvas here")]
    public Canvas _canva;

    private GameObject examinedObject;
    private Vector3 lastMousePosition;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    public bool isExamining = false;

    void Start()
    {
        _canva.enabled = false;
    }

    void Update()
    {
        // E tu�una bas�nca inceleme modunu a�/kapat
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!isExamining)
            {
                // Ray ile bak�lan nesneyi bul
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 3f)) // 3 birim mesafede
                {
                    if (hit.collider.CompareTag("Object"))
                    {
                        examinedObject = hit.collider.gameObject;
                        originalPosition = examinedObject.transform.position;
                        originalRotation = examinedObject.transform.rotation;
                        if (CheckUserClose())
                        {
                            StartExamination();
                        }
                    }
                }
            }
            else
            {
                StopExamination();
            }
        }

        // �nceleme s�ras�nda
        if (isExamining && examinedObject != null)
        {
            _canva.enabled = false;
            Examine();
        }
        else
        {
            _canva.enabled = CheckUserClose();
        }
    }

    void StartExamination()
    {
        isExamining = true;
        lastMousePosition = Input.mousePosition;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Hareket ve mouse look scriptini kapat
        if (targetObject != null)
        {
            var moveScript = targetObject.GetComponent<O_PlayerMovementController>();
            if (moveScript != null)
                moveScript.enabled = false;
        }
    }

    void StopExamination()
    {
        isExamining = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Hareket ve mouse look scriptini tekrar a�
        if (targetObject != null)
        {
            var moveScript = targetObject.GetComponent<O_PlayerMovementController>();
            if (moveScript != null)
                moveScript.enabled = true;
        }

        // Objeyi eski yerine d�nd�r
        if (examinedObject != null)
        {
            examinedObject.transform.position = originalPosition;
            examinedObject.transform.rotation = originalRotation;
            examinedObject = null;
        }
    }

    void Examine()
    {
        // Objeyi offset noktas�na yumu�ak�a ta��
        examinedObject.transform.position = Vector3.Lerp(examinedObject.transform.position, offset.transform.position, 0.2f);

        // Sadece sol mouse tu�una bas�l�ysa d�nd�r
        if (Input.GetMouseButton(0))
        {
            Vector3 deltaMouse = Input.mousePosition - lastMousePosition;
            float rotationSpeed = 0.15f; // �stersen Inspector�dan public yapabilirsin
            examinedObject.transform.Rotate(Vector3.up, -deltaMouse.x * rotationSpeed, Space.World);
            examinedObject.transform.Rotate(Vector3.right, deltaMouse.y * rotationSpeed, Space.World);
        }
        lastMousePosition = Input.mousePosition;
    }

    bool CheckUserClose()
    {
        if (targetObject == null || tableObject == null) return true; // Table bo�sa hep true
        float distance = Vector3.Distance(targetObject.transform.position, tableObject.transform.position);
        return (distance < 2f);
    }
}