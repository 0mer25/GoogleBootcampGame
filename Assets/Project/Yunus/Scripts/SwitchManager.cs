using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SwitchManager : NetworkBehaviour
{
    private Dictionary<int, bool> switchStates = new Dictionary<int, bool>();

    [Header("Door Logic")]
    [SerializeField] private List<int> mustBeUp;
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private string doorTriggerName = "OpenDungeonDoor";

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            for (int i = 1; i <= 9; i++)
                switchStates[i] = false;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateSwitchStateServerRpc(int id, bool isUp)
    {
        switchStates[id] = isUp;
        CheckDoorCondition();
    }

    private void CheckDoorCondition()
    {
        foreach (int id in mustBeUp)
        {
            if (!switchStates.ContainsKey(id) || !switchStates[id])
                return;
        }

        for (int i = 1; i <= 9; i++)
        {
            if (!mustBeUp.Contains(i) && switchStates[i])
                return;
        }

        OpenDoorClientRpc();
    }

    [ClientRpc]
    private void OpenDoorClientRpc()
    {
        Debug.Log("Kapý Açýldý: " + doorTriggerName);
        doorAnimator.Play(doorTriggerName);
    }
}
