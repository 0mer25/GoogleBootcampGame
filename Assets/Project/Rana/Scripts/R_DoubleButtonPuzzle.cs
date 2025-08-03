using Unity.Netcode;
using UnityEngine;

public class R_DoubleButtonPuzzle : NetworkBehaviour
{
    [Header("Button States")]
    private NetworkVariable<bool> button1State = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<bool> button2State = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("Door Settings")]
    public Animator doorAnimator;
    public string doorOpenAnimationName = "DoorOpen";

    private bool isPuzzleSolved = false;

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            button1State.OnValueChanged += OnButton1StateChanged;
            button2State.OnValueChanged += OnButton2StateChanged;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            button1State.OnValueChanged -= OnButton1StateChanged;
            button2State.OnValueChanged -= OnButton2StateChanged;
        }
    }

    private void OnButton1StateChanged(bool previousValue, bool newValue)
    {
        Debug.Log("Button 1 durumu: " + (newValue ? "Basılı" : "Bırakıldı"));
        CheckPuzzleComplete();
    }

    private void OnButton2StateChanged(bool previousValue, bool newValue)
    {
        Debug.Log("Button 2 durumu: " + (newValue ? "Basılı" : "Bırakıldı"));
        CheckPuzzleComplete();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetButtonStateServerRpc(int buttonIndex, bool state)
    {
        if (buttonIndex == 0)
            button1State.Value = state;
        else if (buttonIndex == 1)
            button2State.Value = state;
    }

    private void CheckPuzzleComplete()
    {
        if (!isPuzzleSolved && button1State.Value && button2State.Value)
        {
            SolvePuzzle();
        }
    }

    private void SolvePuzzle()
    {
        isPuzzleSolved = true;
        Debug.Log("🎉 Puzzle çözüldü! Kapı açılıyor!");

        if (doorAnimator != null)
            doorAnimator.Play(doorOpenAnimationName);
    }
}




