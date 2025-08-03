using Unity.Netcode;
using UnityEngine;

public class ButtonSwitch : NetworkBehaviour
{
    public int switchID;
    private Animator animator;

    private NetworkVariable<bool> isUp = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

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

    private void OnSwitchStateChanged(bool oldValue, bool newValue)
    {
        if (animator != null)
        {
            if (newValue)
                animator.Play("PressButton");
            else
                animator.Play("DontPressButton"); // Yaz�m hatas� d�zeltilmi�ti
        }

        Debug.Log($"Switch {switchID} durumu: {(newValue ? "Yukar�" : "A�a��")}");
    }

    void Update()
    {
        if (IsClient && isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            ToggleSwitchServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ToggleSwitchServerRpc(ServerRpcParams rpcParams = default)
    {
        isUp.Value = !isUp.Value;

        // Kap� kontrol� i�in manager�a bildir
        if (ButtonSwitchManager.Instance != null)
        {
            ButtonSwitchManager.Instance.UpdateSwitchStateServerRpc(switchID, isUp.Value);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && other.TryGetComponent<NetworkObject>(out var netObj) && netObj.IsOwner)
        {
            isPlayerNearby = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && other.TryGetComponent<NetworkObject>(out var netObj) && netObj.IsOwner)
        {
            isPlayerNearby = false;
        }
    }
}
