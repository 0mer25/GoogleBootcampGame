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

        // Ba�lang��ta t�m switch'ler a�a��da
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
        // 1, 3, 7, 8 => yukar�da (true) olmal�
        int[] mustBeUp = { 1, 3, 7, 8 };

        foreach (int id in mustBeUp)
        {
            if (!switchStates.ContainsKey(id) || !switchStates[id])
                return; // �art sa�lanm�yor
        }

        // Di�er switch'ler => a�a��da (false) olmal�
        for (int i = 1; i <= 9; i++)
        {
            if (System.Array.IndexOf(mustBeUp, i) == -1)
            {
                if (switchStates[i]) return; // A�a��da olmas� gereken switch yukar�da
            }
        }

        // �artlar sa�land�, kap�y� a�
        OpenDoor();
    }

    void OpenDoor()
    {
        Debug.Log("Kap� A��ld�!");
        doorAnimator.SetTrigger("OpenDungeonDoor");
    }
}
