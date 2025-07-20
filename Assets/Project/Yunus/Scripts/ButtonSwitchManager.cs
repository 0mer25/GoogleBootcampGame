using System.Collections.Generic;
using UnityEngine;

public class ButtonSwitchManager : MonoBehaviour
{
    public static ButtonSwitchManager Instance;

    private Dictionary<int, bool> buttonswitchStates = new Dictionary<int, bool>();

    [Header("Door Logic")]
    [SerializeField] private List<int> mustBeUp; // Inspector'dan ayarlanabilir
    [SerializeField] private Animator doorAnimator;

    void Awake()
    {
        if (Instance == null) Instance = this;

        // Baþlangýçta tüm switch'ler false (aþaðýda)
        for (int i = 1; i <= 9; i++)
            buttonswitchStates[i] = false;
    }

    public void UpdateSwitchState(int id, bool isUp)
    {
        buttonswitchStates[id] = isUp;
        CheckDoorCondition();
    }

    void CheckDoorCondition()
    {
        // Belirlenen switch'ler yukarýda olmalý
        foreach (int id in mustBeUp)
        {
            if (!buttonswitchStates.ContainsKey(id) || !buttonswitchStates[id])
                return; // Þart saðlanmýyor
        }

        // Diðer tüm switch'ler aþaðýda olmalý
        for (int i = 1; i <= 9; i++)
        {
            if (!mustBeUp.Contains(i) && buttonswitchStates[i])
                return; // Aþaðýda olmasý gereken switch yukarýda
        }

        // Þartlar saðlandý, kapýyý aç
        OpenDoor();
    }

    void OpenDoor()
    {
        Debug.Log("Kapý Açýldý!");
        doorAnimator.SetTrigger("MapDoor");
    }
}
