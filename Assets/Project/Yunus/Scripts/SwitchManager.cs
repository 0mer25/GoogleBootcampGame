using System.Collections.Generic;
using UnityEngine;

public class SwitchManager : MonoBehaviour
{
    private Dictionary<int, bool> switchStates = new Dictionary<int, bool>();

    [Header("Door Logic")]
    [SerializeField] private List<int> mustBeUp; // Bu switch ID'leri yukarýda olmalý
    [SerializeField] private Animator doorAnimator; // Baðlý kapýnýn animatörü
    [SerializeField] private string doorTriggerName = "OpenDungeonDoor"; // Kapý açma trigger'ý

    void Awake()
    {
        // Baþlangýçta tüm switch'ler false (aþaðýda)
        for (int i = 1; i <= 9; i++)
            switchStates[i] = false;
    }

    public void UpdateSwitchState(int id, bool isUp)
    {
        switchStates[id] = isUp;
        CheckDoorCondition();
    }

    void CheckDoorCondition()
    {
        // Belirlenen switch'ler yukarýda olmalý
        foreach (int id in mustBeUp)
        {
            if (!switchStates.ContainsKey(id) || !switchStates[id])
                return;
        }

        // Diðer tüm switch'ler aþaðýda olmalý
        for (int i = 1; i <= 9; i++)
        {
            if (!mustBeUp.Contains(i) && switchStates[i])
                return;
        }

        // Þartlar saðlandý, kapýyý aç
        OpenDoor();
    }

    void OpenDoor()
    {
        Debug.Log("Kapý Açýldý: " + doorTriggerName);
        doorAnimator.SetTrigger(doorTriggerName);
    }
}
