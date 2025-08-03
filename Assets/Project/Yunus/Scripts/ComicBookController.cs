using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class ComicBookController : MonoBehaviour
{
    [Header("Comic Pages")]
    public Image[] comicPages; // 3 tane comic page image'i buraya sürükle

    [Header("UI Elements")]
    public Button nextButton;
    public Button startGameButton;
    public Canvas comicCanvas;

    [Header("Animation Settings")]
    public float animationDuration = 0.8f;
    public float pageSpacing = 800f; // Sayfalar arasýndaki mesafe

    [Header("Settings")]
    public string gameSceneName = "GameScene";

    // Sayfa pozisyonlarý
    private Vector3 leftPosition;
    private Vector3 centerPosition;
    private Vector3 rightPosition;

    private int currentPageIndex = 0;
    private bool isAnimating = false;

    void Start()
    {
        // Pozisyonlarý hesapla
        centerPosition = Vector3.zero;
        leftPosition = new Vector3(-pageSpacing, 0, 0);
        rightPosition = new Vector3(pageSpacing, 0, 0);

        // Ýlk durumu ayarla
        SetupInitialPositions();

        // Buton event'lerini ayarla
        nextButton.onClick.AddListener(NextPage);
        startGameButton.onClick.AddListener(StartGame);

        // Baþlangýçta Start Game butonunu gizle
        startGameButton.gameObject.SetActive(false);
    }

    void SetupInitialPositions()
    {
        // Tüm sayfalarý aktif et
        for (int i = 0; i < comicPages.Length; i++)
        {
            comicPages[i].gameObject.SetActive(true);
            ResetPageTransform(comicPages[i]);
        }

        // Ýlk pozisyonlarý ayarla
        if (comicPages.Length >= 1)
            comicPages[0].transform.localPosition = centerPosition; // Sayfa 1 ortada

        if (comicPages.Length >= 2)
            comicPages[1].transform.localPosition = centerPosition; // Sayfa 2 ortada (üst üste)

        if (comicPages.Length >= 3)
            comicPages[2].transform.localPosition = centerPosition; // Sayfa 3 ortada (üst üste)

        // Z pozisyonlarýný ayarla (hangisi önde görünecek)
        UpdatePageDepth();
    }

    void UpdatePageDepth()
    {
        // Aktif sayfa en önde, diðerleri arkada
        for (int i = 0; i < comicPages.Length; i++)
        {
            Vector3 pos = comicPages[i].transform.localPosition;

            if (i == currentPageIndex)
            {
                // Aktif sayfa en önde
                comicPages[i].transform.localPosition = new Vector3(pos.x, pos.y, 0);
                comicPages[i].color = new Color(1f, 1f, 1f, 1f); // Tam opak
            }
            else
            {
                // Diðer sayfalar arkada ve biraz þeffaf
                comicPages[i].transform.localPosition = new Vector3(pos.x, pos.y, 10);
                comicPages[i].color = new Color(0.7f, 0.7f, 0.7f, 0.8f); // Biraz þeffaf ve gri
            }
        }

        // Buton durumlarýný güncelle
        UpdateButtons();
    }

    void NextPage()
    {
        if (isAnimating || currentPageIndex >= comicPages.Length - 1) return;

        StartCoroutine(AnimateToNextPage());
    }

    IEnumerator AnimateToNextPage()
    {
        isAnimating = true;

        currentPageIndex++;

        // Animasyon hedeflerini belirle
        Vector3[] targetPositions = new Vector3[comicPages.Length];

        if (currentPageIndex == 1)
        {
            // Ýlk geçiþ: Sayfa 1 sola, Sayfa 2 ortada aktif, Sayfa 3 ortada
            targetPositions[0] = leftPosition;   // Sayfa 1 sola
            targetPositions[1] = centerPosition; // Sayfa 2 ortada (aktif)
            targetPositions[2] = centerPosition; // Sayfa 3 ortada (üst üste)
        }
        else if (currentPageIndex == 2)
        {
            // Ýkinci geçiþ: Sayfa 1 sola, Sayfa 2 ortada, Sayfa 3 saða aktif
            targetPositions[0] = leftPosition;   // Sayfa 1 sola
            targetPositions[1] = centerPosition; // Sayfa 2 ortada
            targetPositions[2] = rightPosition;  // Sayfa 3 saða (aktif)
        }

        // Animasyonu çalýþtýr
        float elapsed = 0f;
        Vector3[] startPositions = new Vector3[comicPages.Length];

        // Baþlangýç pozisyonlarýný kaydet
        for (int i = 0; i < comicPages.Length; i++)
        {
            startPositions[i] = comicPages[i].transform.localPosition;
        }

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / animationDuration;
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);

            // Tüm sayfalarý ayný anda hareket ettir
            for (int i = 0; i < comicPages.Length; i++)
            {
                Vector3 currentPos = Vector3.Lerp(startPositions[i], targetPositions[i], easedProgress);
                comicPages[i].transform.localPosition = new Vector3(currentPos.x, currentPos.y,
                                                                   comicPages[i].transform.localPosition.z);
            }

            yield return null;
        }

        // Final pozisyonlarý ayarla
        for (int i = 0; i < comicPages.Length; i++)
        {
            Vector3 finalPos = targetPositions[i];
            comicPages[i].transform.localPosition = new Vector3(finalPos.x, finalPos.y,
                                                               comicPages[i].transform.localPosition.z);
        }

        // Derinlik ve görünümü güncelle
        UpdatePageDepth();

        isAnimating = false;
    }

    void ResetPageTransform(Image page)
    {
        page.transform.localScale = Vector3.one;
        page.color = new Color(page.color.r, page.color.g, page.color.b, 1f);
    }

    void UpdateButtons()
    {
        // Son sayfada Next butonunu gizle, Start Game butonunu göster
        if (currentPageIndex >= comicPages.Length - 1)
        {
            nextButton.gameObject.SetActive(false);
            startGameButton.gameObject.SetActive(true);
        }
        else
        {
            nextButton.gameObject.SetActive(true);
            startGameButton.gameObject.SetActive(false);
        }
    }

    void StartGame()
    {
        // Canvas'ý kapat
        comicCanvas.gameObject.SetActive(false);

        // Farklý sahneye geçmek istiyorsan:
        // SceneManager.LoadScene(gameSceneName);

        // Ya da oyun objesini aktif et
        // GameObject gameplayObjects = GameObject.FindWithTag("Gameplay");
        // gameplayObjects.SetActive(true);
    }

    // Klavye kontrolleri (opsiyonel)
    void Update()
    {
        if (isAnimating) return;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentPageIndex < comicPages.Length - 1)
            {
                NextPage();
            }
            else
            {
                StartGame();
            }
        }
    }
}