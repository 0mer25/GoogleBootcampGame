using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FinalPortalTrigger : MonoBehaviour
{
    public Image whitePanel;        // Tam ekran beyaz UI panel
    public float fadeSpeed = 2f;    // Açýlma/Kapanma hýzý
    public float delayBeforeFadeOut = 1.5f;
    public float blackOutDelay = 1f;
    public Image blackPanel;        // Ekran kararsýn diye
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        if (other.CompareTag("Player"))
        {
            hasTriggered = true;
            StartCoroutine(FlashAndFadeOut());
        }
    }

    IEnumerator FlashAndFadeOut()
    {
        // Beyaz panele fade-in
        yield return StartCoroutine(FadeImage(whitePanel, 0f, 1f));

        yield return new WaitForSeconds(delayBeforeFadeOut);

        // Beyazdan sonra kararma
        yield return StartCoroutine(FadeImage(blackPanel, 0f, 1f));

        // Burada istersen sahne geçiþi yapabiliriz
        // SceneManager.LoadScene("NextScene");
    }

    IEnumerator FadeImage(Image img, float from, float to)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * fadeSpeed;
            Color c = img.color;
            c.a = Mathf.Lerp(from, to, t);
            img.color = c;
            yield return null;
        }
    }
}

