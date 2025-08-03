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

    [Header("Müzik")]
    [SerializeField] private AudioSource musicAudioSource;
    [SerializeField] private AudioClip mainMenuMusic;

    private bool videoIsPlaying = false;

    void Awake()
    {
        // Baþlangýçta ana panel açýk, lobi kapalý
        mainPanel.SetActive(true);
        lobbyPanel.SetActive(false);

        // MÜZÝÐÝ BAÞLANGIÇTA DURDUR
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
        // Video var mý ve çalýyor mu kontrol et
        if (videoPlayer != null && videoPlayer.gameObject.activeInHierarchy)
        {
            videoIsPlaying = true;

            // Video bittiðinde çaðrýlacak event'i baðla
            videoPlayer.loopPointReached += OnVideoFinished;

            // Müziði durdur (emin olmak için)
            StopMainMenuMusic();

            Debug.Log("Video tespit edildi, müzik durduruldu");
        }
        else
        {
            // Video yoksa müziði baþlat
            videoIsPlaying = false;
            StartMainMenuMusic();
            Debug.Log("Video yok, müzik baþlatýldý");
        }
    }

    // Video bittiðinde çaðrýlacak fonksiyon
    private void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("Video bitti!");

        videoIsPlaying = false;

        // Video objesini kapat
        if (videoObject != null)
        {
            videoObject.SetActive(false);
        }

        // Canvas'ý aktif et
        Canvas.SetActive(true);

        // Ana paneli göster
        mainPanel.SetActive(true);

        // Kýsa bir gecikme sonrasý müziði baþlat
        Invoke("StartMainMenuMusic", 0.1f);
    }

    private void StartMainMenuMusic()
    {
        // Video hala çalýyorsa müzik baþlatma
        if (videoIsPlaying)
        {
            Debug.Log("Video çalýyor, müzik baþlatýlmadý");
            return;
        }

        // Debug kontrolleri
        if (musicAudioSource == null)
        {
            Debug.LogError("AudioSource atanmamýþ!");
            return;
        }

        if (mainMenuMusic == null)
        {
            Debug.LogError("Müzik dosyasý atanmamýþ!");
            return;
        }

        // Eðer zaten çalýyorsa tekrar baþlatma
        if (musicAudioSource.isPlaying)
        {
            Debug.Log("Müzik zaten çalýyor");
            return;
        }

        // AudioSource ayarlarý
        musicAudioSource.clip = mainMenuMusic;
        musicAudioSource.loop = true;
        musicAudioSource.volume = 0.5f;

        // Müziði çal
        musicAudioSource.Play();
        Debug.Log("Ana menü müziði baþlatýldý: " + mainMenuMusic.name);
    }

    // Müziði durdurmak için
    public void StopMainMenuMusic()
    {
        if (musicAudioSource != null && musicAudioSource.isPlaying)
        {
            musicAudioSource.Stop();
            Debug.Log("Müzik durduruldu");
        }
    }

    // Component destroy olduðunda event'i temizle
    void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }

    // Video durumunu manuel kontrol etmek için (opsiyonel)
    void Update()
    {
        if (videoPlayer != null && videoIsPlaying)
        {
            // Video durduysa ama hala playing olarak iþaretliyse
            if (!videoPlayer.isPlaying && videoPlayer.frame > 0)
            {
                OnVideoFinished(videoPlayer);
            }
        }
    }
}