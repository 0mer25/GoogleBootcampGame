using Unity.Netcode;
using UnityEngine;
using TMPro;

public class PuzzleManager : NetworkBehaviour
{
    [Header("Genel Ayarlar")]
    public GameObject cardakObject;
    public TextMeshProUGUI puzzlePromptText;
    public int totalRequired = 3;

    [Header("3. Panel Ayarları")]
    public GameObject cardakSpecialObject;
    public GameObject thirdPanelObject;

    private NetworkVariable<int> interactionCount = new NetworkVariable<int>(0,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private void Start()
    {
        if (cardakSpecialObject != null)
            cardakSpecialObject.SetActive(false);
        if (thirdPanelObject != null)
            thirdPanelObject.SetActive(false);

        // NetworkVariable değişiklik dinleyicisi
        interactionCount.OnValueChanged += OnInteractionCountChanged;
    }

    public override void OnDestroy()
    {
        if (interactionCount != null)
            interactionCount.OnValueChanged -= OnInteractionCountChanged;
        base.OnDestroy();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RegisterInteractionServerRpc(string objectName, ServerRpcParams rpcParams = default)
    {
        if (!IsServer) return;

        interactionCount.Value++;

        // Tüm clientlere mesaj gönder
        ShowPromptClientRpc($"{objectName} ile etkileşim sağlandı! ({interactionCount.Value}/{totalRequired})");

        if (interactionCount.Value >= totalRequired)
        {
            ShowPromptClientRpc("Tüm objeler tamamlandı! Çardağa git!");
            ActivateObjectsClientRpc();
        }
    }

    [ClientRpc]
    private void ShowPromptClientRpc(string message)
    {
        ShowPrompt(message);
    }

    [ClientRpc]
    private void ActivateObjectsClientRpc()
    {
        if (cardakObject != null)
            cardakObject.SetActive(true);
        if (cardakSpecialObject != null)
            cardakSpecialObject.SetActive(true);
        if (thirdPanelObject != null)
            thirdPanelObject.SetActive(true);
    }

    private void OnInteractionCountChanged(int oldValue, int newValue)
    {
        // İsteğe bağlı: NetworkVariable değiştiğinde ek işlemler
    }

    private void ShowPrompt(string message)
    {
        if (puzzlePromptText == null) return;

        puzzlePromptText.text = message;
        puzzlePromptText.gameObject.SetActive(true);

        CancelInvoke(nameof(HidePrompt));
        Invoke(nameof(HidePrompt), 6f);
    }

    private void HidePrompt()
    {
        if (puzzlePromptText != null)
            puzzlePromptText.gameObject.SetActive(false);
    }
}