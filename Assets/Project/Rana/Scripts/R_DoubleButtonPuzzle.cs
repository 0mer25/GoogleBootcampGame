using UnityEngine;

public class R_DoubleButtonPuzzle : MonoBehaviour
{
    [Header("Buton Takibi")]
    public bool[] buttonStates = new bool[2];

    [Header("Efektler")]
    public GameObject portalLight;              // Kapının ortasındaki ışık
    public ParticleSystem portalParticles;      // İsteğe bağlı görsel efekt

    [Header("Kapanış Trigger")]
    public GameObject finalTrigger;             // Karanlık için aktif olacak trigger

    private bool isPuzzleSolved = false;

    public void SetButtonState(int index, bool state)
    {
        buttonStates[index] = state;

        if (!isPuzzleSolved && buttonStates[0] && buttonStates[1])
        {
            SolvePuzzle();
        }
    }

    void SolvePuzzle()
    {
        isPuzzleSolved = true;
        Debug.Log("🎉 Puzzle çözüldü! Geçit ışığı yanıyor!");

        if (portalLight != null)
            portalLight.SetActive(true);

        if (portalParticles != null)
            portalParticles.Play();

        if (finalTrigger != null)
            finalTrigger.SetActive(true);
    }
}




