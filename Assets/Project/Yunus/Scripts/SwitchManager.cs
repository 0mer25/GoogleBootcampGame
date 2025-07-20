using System.Collections.Generic;
using UnityEngine;

public class SwitchManager : MonoBehaviour
{
    private Dictionary<int, bool> switchStates = new Dictionary<int, bool>();

    [Header("Door Logic")]
    [SerializeField] private List<int> mustBeUp; // Bu switch ID'leri yukar�da olmal�
    [SerializeField] private Animator doorAnimator; // Ba�l� kap�n�n animat�r�
    [SerializeField] private string doorTriggerName = "OpenDungeonDoor"; // Kap� a�ma trigger'�

    void Awake()
    {
        // Ba�lang��ta t�m switch'ler false (a�a��da)
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
        // Belirlenen switch'ler yukar�da olmal�
        foreach (int id in mustBeUp)
        {
            if (!switchStates.ContainsKey(id) || !switchStates[id])
                return;
        }

        // Di�er t�m switch'ler a�a��da olmal�
        for (int i = 1; i <= 9; i++)
        {
            if (!mustBeUp.Contains(i) && switchStates[i])
                return;
        }

        // �artlar sa�land�, kap�y� a�
        OpenDoor();
    }

    void OpenDoor()
    {
        Debug.Log("Kap� A��ld�: " + doorTriggerName);
        doorAnimator.SetTrigger(doorTriggerName);
    }
}
