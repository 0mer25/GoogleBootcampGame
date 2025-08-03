using Unity.Netcode;
using UnityEngine;

public class Switch : NetworkBehaviour
{
    public int switchID;
    public SwitchManager manager;

    private Animator animator;
    private NetworkVariable<bool> isUp = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public bool isPlayerNearby = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        isUp.OnValueChanged += OnSwitchStateChanged;
    }

    void OnDestroy()
    {
        isUp.OnValueChanged -= OnSwitchStateChanged;
    }

    private void OnSwitchStateChanged(bool previousValue, bool newValue)
    {
        if (animator != null)
        {
            if (newValue)
                animator.Play("UpSwitch");
            else
                animator.Play("DownSwitch");
        }

        Debug.Log("Switch " + switchID + " durumu: " + (newValue ? "Yukarý" : "Aþaðý"));
    }

    public void ToggleSwitch()
    {
        if (!IsOwner) return; // Sadece oyuncunun kendisi tetikleyebilir

        ToggleSwitchServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ToggleSwitchServerRpc(ServerRpcParams rpcParams = default)
    {
        isUp.Value = !isUp.Value;

        if (manager != null)
        {
            manager.UpdateSwitchStateServerRpc(switchID, isUp.Value);
        }
    }


    void Update()
    {
        if (IsClient && isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            ToggleSwitchServerRpc(); // Direkt sunucuya isteði gönder
        }
    }


    void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if (other.CompareTag("Player"))
        {
            ulong clientId = other.GetComponent<NetworkObject>().OwnerClientId;
            SetNearbyClientRpc(true, clientId);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;

        if (other.CompareTag("Player"))
        {
            ulong clientId = other.GetComponent<NetworkObject>().OwnerClientId;
            SetNearbyClientRpc(false, clientId);
        }
    }

    [ClientRpc]
    void SetNearbyClientRpc(bool state, ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;

        isPlayerNearby = state;
    }

}
