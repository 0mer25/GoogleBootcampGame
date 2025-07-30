using TMPro;
using UnityEngine;

public class SayiliPanelController : MonoBehaviour
{
    [Header("Referanslar")]
    public TMP_Text panelText;     // "Sayı: X" yazısı
    public GameObject panelUI;     // Panel UI objesi
    public TMP_Text numberText;    // Sayacın gösterildiği UI

    [Header("Sayı Ayarları")]
    public int hedefSayi { get; private set; }
    public int mevcutSayi = 0;

    private bool cozuldu = false;

    void Start()
    {
        panelUI.SetActive(false);
        numberText.text = mevcutSayi.ToString();
    }

    public void PaneliAktifEt()
    {
        hedefSayi = Random.Range(2, 6); // 2 ile 5 arası hedef sayı
        panelText.text = "Sayı: " + hedefSayi;
        panelUI.SetActive(true);
        mevcutSayi = 0;
        numberText.text = mevcutSayi.ToString();
        cozuldu = false;
    }

    public void SayiyiArtir()
    {
        if (cozuldu) return;

        mevcutSayi++;
        numberText.text = mevcutSayi.ToString();

        if (mevcutSayi == hedefSayi)
        {
            cozuldu = true;
            numberText.text = "✔️";
            Debug.Log("🎉 Sayılı panel çözüldü!");
        }
    }

    public bool PuzzleCozulduMu()
    {
        return cozuldu;
    }

    public int GetHedefSayi()
    {
        return hedefSayi;
    }
}


