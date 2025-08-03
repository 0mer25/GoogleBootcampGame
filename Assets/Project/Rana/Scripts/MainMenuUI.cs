using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class MainMenuUI : MonoBehaviour
{
    [Header("Paneller")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject lobbyPanel;

    [Header("Butonlar")]
    [SerializeField] private Button btnPlay;
    [SerializeField] private Button btnBackFromLobby;
    [SerializeField] private Button btnCreate;
    [SerializeField] private Button btnJoin;
    [SerializeField] private GameObject Canvas;

    [Header("Video")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private GameObject videoObject;

    [Header("M�zik")]
    [SerializeField] private AudioSource musicAudioSource;
    [SerializeField] private AudioClip mainMenuMusic;

    private bool videoIsPlaying = false;

    void Awake()
    {
        // Ba�lang��ta ana panel a��k, lobi kapal�
        mainPanel.SetActive(true);
        lobbyPanel.SetActive(false);

        // M�Z��� BA�LANGI�TA DURDUR
        if (musicAudioSource != null)
        {
            musicAudioSource.Stop();
        }

        btnPlay.onClick.AddListener(() => {
            mainPanel.SetActive(false);
            Canvas.SetActive(false);
        });
    }

    void Start()
    {
        // Video var m� ve �al�yor mu kontrol et
        if (videoPlayer != null && videoPlayer.gameObject.activeInHierarchy)
        {
            videoIsPlaying = true;

            // Video bitti�inde �a�r�lacak event'i ba�la
            videoPlayer.loopPointReached += OnVideoFinished;

            // M�zi�i durdur (emin olmak i�in)
            StopMainMenuMusic();

            Debug.Log("Video tespit edildi, m�zik durduruldu");
        }
        else
        {
            // Video yoksa m�zi�i ba�lat
            videoIsPlaying = false;
            StartMainMenuMusic();
            Debug.Log("Video yok, m�zik ba�lat�ld�");
        }
    }

    // Video bitti�inde �a�r�lacak fonksiyon
    private void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("Video bitti!");

        videoIsPlaying = false;

        // Video objesini kapat
        if (videoObject != null)
        {
            videoObject.SetActive(false);
        }

        // Canvas'� aktif et
        Canvas.SetActive(true);

        // Ana paneli g�ster
        mainPanel.SetActive(true);

        // K�sa bir gecikme sonras� m�zi�i ba�lat
        Invoke("StartMainMenuMusic", 0.1f);
    }

    private void StartMainMenuMusic()
    {
        // Video hala �al�yorsa m�zik ba�latma
        if (videoIsPlaying)
        {
            Debug.Log("Video �al�yor, m�zik ba�lat�lmad�");
            return;
        }

        // Debug kontrolleri
        if (musicAudioSource == null)
        {
            Debug.LogError("AudioSource atanmam��!");
            return;
        }

        if (mainMenuMusic == null)
        {
            Debug.LogError("M�zik dosyas� atanmam��!");
            return;
        }

        // E�er zaten �al�yorsa tekrar ba�latma
        if (musicAudioSource.isPlaying)
        {
            Debug.Log("M�zik zaten �al�yor");
            return;
        }

        // AudioSource ayarlar�
        musicAudioSource.clip = mainMenuMusic;
        musicAudioSource.loop = true;
        musicAudioSource.volume = 0.5f;

        // M�zi�i �al
        musicAudioSource.Play();
        Debug.Log("Ana men� m�zi�i ba�lat�ld�: " + mainMenuMusic.name);
    }

    // M�zi�i durdurmak i�in
    public void StopMainMenuMusic()
    {
        if (musicAudioSource != null && musicAudioSource.isPlaying)
        {
            musicAudioSource.Stop();
            Debug.Log("M�zik durduruldu");
        }
    }

    // Component destroy oldu�unda event'i temizle
    void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }

    // Video durumunu manuel kontrol etmek i�in (opsiyonel)
    void Update()
    {
        if (videoPlayer != null && videoIsPlaying)
        {
            // Video durduysa ama hala playing olarak i�aretliyse
            if (!videoPlayer.isPlaying && videoPlayer.frame > 0)
            {
                OnVideoFinished(videoPlayer);
            }
        }
    }
}