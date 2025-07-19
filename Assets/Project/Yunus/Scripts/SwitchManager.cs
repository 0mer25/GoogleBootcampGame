using System.Collections.Generic;
using UnityEngine;

public class SwitchManager : MonoBehaviour
{
    public static SwitchManager Instance;

    private Dictionary<int, bool> switchStates = new Dictionary<int, bool>();
    [SerializeField] private Animator doorAnimator;

    void Awake()
    {
        if (Instance == null) Instance = this;

        // Baþlangýçta tüm switch'ler aþaðýda
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
        // 1, 3, 7, 8 => yukarýda (true) olmalý
        int[] mustBeUp = { 1, 3, 7, 8 };

        foreach (int id in mustBeUp)
        {
            if (!switchStates.ContainsKey(id) || !switchStates[id])
                return; // Þart saðlanmýyor
        }

        // Diðer switch'ler => aþaðýda (false) olmalý
        for (int i = 1; i <= 9; i++)
        {
            if (System.Array.IndexOf(mustBeUp, i) == -1)
            {
                if (switchStates[i]) return; // Aþaðýda olmasý gereken switch yukarýda
            }
        }

        // Þartlar saðlandý, kapýyý aç
        OpenDoor();
    }

    void OpenDoor()
    {
        Debug.Log("Kapý Açýldý!");
        doorAnimator.SetTrigger("OpenDungeonDoor");
    }
}
