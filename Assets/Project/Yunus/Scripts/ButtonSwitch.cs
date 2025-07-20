using UnityEngine;

public class ButtonSwitch : MonoBehaviour
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
            animator.Play("PressButton");
        else
            animator.Play("DontPressButotn");

        ButtonSwitchManager.Instance.UpdateSwitchState(switchID, isUp);
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
