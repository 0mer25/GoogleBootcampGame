using UnityEngine;

public class R_PanelController : MonoBehaviour
{
    public Renderer[] lights;
    public Material redMat;
    public Material greenMat;
    public float interval = 1f;

    private int currentIndex = 0;
    private bool isLocked = false;

    void Start()
    {
        InvokeRepeating("CycleLights", 1f, interval);
    }

    void CycleLights()
    {
        if (isLocked || lights == null || lights.Length == 0) return;

        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i] != null)
                lights[i].material = redMat;
        }

        if (lights[currentIndex] != null)
            lights[currentIndex].material = greenMat;

        currentIndex = (currentIndex + 1) % lights.Length;
    }

    public int GetCurrentLightIndex()
    {
        return currentIndex == 0 ? lights.Length - 1 : currentIndex - 1;
    }

    public void GoToNextStep()
    {
        CancelInvoke("CycleLights");
        isLocked = true;

        int greenIndex = GetCurrentLightIndex();
        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i] != null)
                lights[i].material = (i == greenIndex) ? greenMat : redMat;
        }

        Debug.Log("🎯 Puzzle kilitlendi, doğru ışık sabitlendi!");
    }
    public bool IsPuzzleComplete()
    {
        return isLocked;
    }

}


