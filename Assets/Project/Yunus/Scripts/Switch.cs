using UnityEngine;

public class Switch : MonoBehaviour
{
    public int switchID;
    public SwitchManager manager;

    public bool isUp = false;
    public bool isPlayerNearby = false;

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void ToggleSwitch()
    {
        isUp = !isUp;

        if (manager != null)
        {
            manager.UpdateSwitchState(switchID, isUp);
        }

        // Animasyonu oynat
        if (animator != null)
        {
            if (isUp)
                animator.Play("UpSwitch");
            else
                animator.Play("DownSwitch");
        }

        Debug.Log("Switch " + switchID + " durumu: " + (isUp ? "Yukarý" : "Aþaðý"));
    }

    void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            ToggleSwitch();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
        }
    }
}
