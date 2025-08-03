using UnityEngine;
using Unity.Netcode;

public class R_PanelController : NetworkBehaviour
{
    [Header("Light Settings")]
    public Renderer[] lights;
    public Material redMat;
    public Material greenMat;
    public float interval = 1f;

    private bool isLocked = false;
    private NetworkVariable<int> currentIndex = new NetworkVariable<int>(0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        // Network spawn'da subscription yap
        currentIndex.OnValueChanged += OnLightIndexChanged;

        // İlk ışık durumunu ayarla
        UpdateLights(currentIndex.Value);

        // Sadece server'da cycle başlat
        if (IsServer)
        {
            InvokeRepeating(nameof(CycleLightsServer), 1f, interval);
        }
    }

    public override void OnNetworkDespawn()
    {
        // Subscription'ı temizle
        currentIndex.OnValueChanged -= OnLightIndexChanged;
    }

    void OnLightIndexChanged(int oldIndex, int newIndex)
    {
        UpdateLights(newIndex);
    }

    void UpdateLights(int index)
    {
        if (lights == null || lights.Length == 0) return;

        // Tüm ışıkları kırmızı yap
        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i] != null)
                lights[i].material = redMat;
        }

        // Mevcut indeksi yeşil yap
        if (index >= 0 && index < lights.Length && lights[index] != null)
            lights[index].material = greenMat;
    }

    void CycleLightsServer()
    {
        if (isLocked || lights == null || lights.Length == 0) return;
        currentIndex.Value = (currentIndex.Value + 1) % lights.Length;
    }

    public int GetCurrentLightIndex()
    {
        return currentIndex.Value;
    }

    public void GoToNextStep()
    {
        if (!IsServer) return;

        CancelInvoke(nameof(CycleLightsServer));
        isLocked = true;

        // Mevcut yeşil ışığı sabitle
        int greenIndex = currentIndex.Value;
        SetLightsStateClientRpc(greenIndex);

        Debug.Log("🎯 Puzzle kilitlendi, doğru ışık sabitlendi!");
    }

    [ClientRpc]
    void SetLightsStateClientRpc(int greenIndex)
    {
        if (lights == null || lights.Length == 0) return;

        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i] != null)
                lights[i].material = (i == greenIndex) ? greenMat : redMat;
        }
    }

    public bool IsPuzzleComplete()
    {
        return isLocked;
    }
}