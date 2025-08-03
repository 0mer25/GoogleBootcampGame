using Unity.Netcode;
using UnityEngine;

public class PuzzleInteractable : NetworkBehaviour
{
    public PuzzleManager puzzleManager;
    public GameObject interactionUI;

    private bool playerNearby = false;
    private NetworkVariable<bool> interacted = new NetworkVariable<bool>(false,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private void Start()
    {
        if (interactionUI != null)
            interactionUI.SetActive(false);

        interacted.OnValueChanged += OnInteractedChanged;
    }

    public override void OnDestroy()
    {
        if (interacted != null)
            interacted.OnValueChanged -= OnInteractedChanged;
        base.OnDestroy();
    }

    private void Update()
    {
        if (playerNearby && !interacted.Value && Input.GetKeyDown(KeyCode.E))
        {
            InteractServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractServerRpc()
    {
        if (!IsServer || interacted.Value) return;

        interacted.Value = true;
        Debug.Log($"🟢 {gameObject.name} ile etkileşime geçildi.");

        if (puzzleManager != null)
            puzzleManager.RegisterInteractionServerRpc(gameObject.name);
    }

    private void OnInteractedChanged(bool oldValue, bool newValue)
    {
        if (newValue && interactionUI != null)
            interactionUI.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !interacted.Value)
        {
            // Sadece local player için trigger kontrolü
            if (other.GetComponent<NetworkObject>()?.IsOwner == true)
            {
                playerNearby = true;
                if (interactionUI != null)
                    interactionUI.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.GetComponent<NetworkObject>()?.IsOwner == true)
            {
                playerNearby = false;
                if (interactionUI != null)
                    interactionUI.SetActive(false);
            }
        }
    }
}