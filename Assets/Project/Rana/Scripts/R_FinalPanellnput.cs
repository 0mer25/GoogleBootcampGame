using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FinalPanelInput : MonoBehaviour
{
    public TMP_InputField inputField;
    public TextMeshProUGUI feedbackText;
    public Image fadePanel;
    public float fadeDuration = 2f;

    [SerializeField] private string correctAnswer = "sır";
    private bool transitionStarted = false;

    public void CheckAnswer()
    {
        if (inputField == null || transitionStarted) return;

        string userAnswer = inputField.text.Trim().ToLower();

        if (userAnswer == correctAnswer)
        {
            if (feedbackText != null)
            {
                feedbackText.text = " Doğru cevap! Haritadan kurtuldun.";
                feedbackText.color = Color.green;
            }

            StartCoroutine(FadeToBlackOnly()); // sadece ekranı karartacak
        }
        else
        {
            if (feedbackText != null)
            {
                feedbackText.text = " Yanlış cevap. Bir daha dene.";
                feedbackText.color = Color.red;
            }
        }
    }

    private System.Collections.IEnumerator FadeToBlackOnly()
    {
        transitionStarted = true;

        float t = 0;
        Color color = fadePanel.color;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            color.a = Mathf.Lerp(0, 1, t / fadeDuration);
            fadePanel.color = color;
            yield return null;
        }

        
    }
}


