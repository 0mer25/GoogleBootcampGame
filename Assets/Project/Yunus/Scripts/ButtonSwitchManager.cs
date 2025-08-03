using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ButtonSwitchManager : NetworkBehaviour
{
    public static ButtonSwitchManager Instance;

    private Dictionary<int, bool> buttonswitchStates = new Dictionary<int, bool>();

    [Header("Door Logic")]
    [SerializeField] private List<int> mustBeUp; // Inspector'dan ayarlanabilir
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private string doorTriggerName = "MapDoor";

    public override void OnNetworkSpawn()
    {
        if (Instance == null) Instance = this;

        if (IsServer)
        {
            for (int i = 1; i <= 9; i++)
            {
                buttonswitchStates[i] = false;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateSwitchStateServerRpc(int id, bool isUp)
    {
        buttonswitchStates[id] = isUp;
        CheckDoorCondition();
    }

    private void CheckDoorCondition()
    {
        // Gerekli tüm switch'ler yukarýda mý?
        foreach (int id in mustBeUp)
        {
            if (!buttonswitchStates.ContainsKey(id) || !buttonswitchStates[id])
                return;
        }

        // Diðer tüm switch'ler aþaðýda mý?
        for (int i = 1; i <= 9; i++)
        {
            if (!mustBeUp.Contains(i) && buttonswitchStates.ContainsKey(i) && buttonswitchStates[i])
                return;
        }

        // Kapýyý tüm client'larda aç
        OpenDoorClientRpc();
    }

    [ClientRpc]
    private void OpenDoorClientRpc()
    {
        if (doorAnimator != null)
        {
            Debug.Log("Kapý Açýldý (ClientRPC)!");
            doorAnimator.Play(doorTriggerName);
        }
    }
}
