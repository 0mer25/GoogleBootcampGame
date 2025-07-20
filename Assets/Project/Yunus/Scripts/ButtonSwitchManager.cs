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

        // Ba�lang��ta t�m switch'ler false (a�a��da)
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
        // Belirlenen switch'ler yukar�da olmal�
        foreach (int id in mustBeUp)
        {
            if (!buttonswitchStates.ContainsKey(id) || !buttonswitchStates[id])
                return; // �art sa�lanm�yor
        }

        // Di�er t�m switch'ler a�a��da olmal�
        for (int i = 1; i <= 9; i++)
        {
            if (!mustBeUp.Contains(i) && buttonswitchStates[i])
                return; // A�a��da olmas� gereken switch yukar�da
        }

        // �artlar sa�land�, kap�y� a�
        OpenDoor();
    }

    void OpenDoor()
    {
        Debug.Log("Kap� A��ld�!");
        doorAnimator.SetTrigger("MapDoor");
    }
}
