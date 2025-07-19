using UnityEngine;

public class Switch : MonoBehaviour
{
    public int switchID;
    private Animator animator;
    public bool isUp = false; // Baþlangýçta aþaðýda
    public bool isPlayerNearby = false;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            ToggleSwitch();
        }
    }

    void ToggleSwitch()
    {
        isUp = !isUp;

        if (isUp)
            animator.Play("UpSwitch");
        else
            animator.Play("DownSwitch");

        SwitchManager.Instance.UpdateSwitchState(switchID, isUp);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            isPlayerNearby = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            isPlayerNearby = false;
    }
}
