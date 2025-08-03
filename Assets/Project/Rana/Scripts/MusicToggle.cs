using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class MusicToggle : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Toggle muteToggle;

    private AudioSource audioSource;

    // Önceki ses seviyesi saklanýr ki unmute'layýnca geri gelsin
    private float previousVolume;

    // PlayerPrefs anahtarlarý
    private const string MutedKey = "MenuMusicMuted";
    private const string VolumeKey = "MenuMusicVolume";

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        // Daha önce kaydedilmiþ ses seviyesi varsa uygula, yoksa default 0.7
        float savedVolume = PlayerPrefs.GetFloat(VolumeKey, 0.7f);
        audioSource.volume = savedVolume;
        previousVolume = savedVolume;

        // Kaydedilmiþ mute durumu
        bool isMuted = PlayerPrefs.GetInt(MutedKey, 0) == 1;

        // Toggle'ý baþlat
        if (muteToggle != null)
        {
            muteToggle.isOn = isMuted;
            muteToggle.onValueChanged.AddListener(OnToggleChanged);
        }

        // Eðer mute'luysa sesi kapat
        if (isMuted)
        {
            audioSource.volume = 0f;
        }
    }

    private void OnToggleChanged(bool isMuted)
    {
        if (isMuted)
        {
            // Þu anki ses seviyesi kaydedilir, sonra sýfýrla
            previousVolume = audioSource.volume > 0f ? audioSource.volume : previousVolume;
            audioSource.volume = 0f;
            PlayerPrefs.SetInt(MutedKey, 1);
        }
        else
        {
            // Önceki seviyeyi geri ver
            audioSource.volume = previousVolume;
            PlayerPrefs.SetInt(MutedKey, 0);
        }

        PlayerPrefs.Save();
    }

    // Dýþarýdan ses ayarlarsan (örneðin slider), buradan güncelle
    public void SetVolume(float v)
    {
        // Mute deðilse uygula
        if (muteToggle != null && muteToggle.isOn)
            return;

        audioSource.volume = v;
        previousVolume = v;
        PlayerPrefs.SetFloat(VolumeKey, v);
        PlayerPrefs.Save();
    }
}

