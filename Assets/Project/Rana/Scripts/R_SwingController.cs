using UnityEngine;
using Unity.Netcode;

public class R_SwingController : NetworkBehaviour
{
    public Transform swingSeat;
    public float swingSpeed = 2f;
    public float swingAmount = 0.3f;

    private NetworkVariable<bool> isSwinging = new NetworkVariable<bool>();
    private float timer = 0f;
    private Vector3 originalCamPos;

    void Update()
    {
        if (!IsOwner) return;

        if (isSwinging.Value)
        {
            timer += Time.deltaTime * swingSpeed;
            float swingOffset = Mathf.Sin(timer) * swingAmount;
            Camera.main.transform.localPosition = originalCamPos + new Vector3(0, 0, swingOffset);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void StartSwingServerRpc(ulong clientId)
    {
        isSwinging.Value = true;
        StartSwingClientRpc(clientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void StopSwingServerRpc(ulong clientId)
    {
        isSwinging.Value = false;
        StopSwingClientRpc(clientId);
    }

    [ClientRpc]
    void StartSwingClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            originalCamPos = Camera.main.transform.localPosition;
            timer = 0f;
        }
    }

    [ClientRpc]
    void StopSwingClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            Camera.main.transform.localPosition = originalCamPos;
        }
    }
}
