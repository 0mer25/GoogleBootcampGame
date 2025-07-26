using UnityEngine;
using TMPro;

public class PuzzleManager : MonoBehaviour
{
    [Header("Genel Ayarlar")]
    public GameObject cardakObject;
    public TextMeshProUGUI puzzlePromptText;
    public int totalRequired = 3;

    [Header("3. Panel Ayarları")]
    public GameObject cardakSpecialObject;
    public GameObject thirdPanelObject;

    private int interactionCount = 0;

    private void Start()
    {
        if (cardakSpecialObject != null)
            cardakSpecialObject.SetActive(false);

        if (thirdPanelObject != null)
            thirdPanelObject.SetActive(false);
    }

    public void RegisterInteraction(GameObject interactedObject)
    {
        interactionCount++;
        string name = interactedObject.name;

        ShowPrompt($" {name} ile etkileşim sağlandı! ({interactionCount}/{totalRequired})");

        if (interactionCount >= totalRequired)
        {
            ShowPrompt(" Tüm objeler tamamlandı! Çardağa git!");

            if (cardakObject != null)
                cardakObject.SetActive(true);

            if (cardakSpecialObject != null)
                cardakSpecialObject.SetActive(true);

            if (thirdPanelObject != null)
                thirdPanelObject.SetActive(true);
        }
    }

    private void ShowPrompt(string message)
    {
        if (puzzlePromptText == null) return;

        puzzlePromptText.text = message;
        puzzlePromptText.gameObject.SetActive(true);

        CancelInvoke(nameof(HidePrompt));
        Invoke(nameof(HidePrompt), 6f);
    }

    private void HidePrompt()
    {
        if (puzzlePromptText != null)
            puzzlePromptText.gameObject.SetActive(false);
    }
}



