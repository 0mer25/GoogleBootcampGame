using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class MusicToggle : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Toggle muteToggle;

    private AudioSource audioSource;

    // �nceki ses seviyesi saklan�r ki unmute'lay�nca geri gelsin
    private float previousVolume;

    // PlayerPrefs anahtarlar�
    private const string MutedKey = "MenuMusicMuted";
    private const string VolumeKey = "MenuMusicVolume";

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        // Daha �nce kaydedilmi� ses seviyesi varsa uygula, yoksa default 0.7
        float savedVolume = PlayerPrefs.GetFloat(VolumeKey, 0.7f);
        audioSource.volume = savedVolume;
        previousVolume = savedVolume;

        // Kaydedilmi� mute durumu
        bool isMuted = PlayerPrefs.GetInt(MutedKey, 0) == 1;

        // Toggle'� ba�lat
        if (muteToggle != null)
        {
            muteToggle.isOn = isMuted;
            muteToggle.onValueChanged.AddListener(OnToggleChanged);
        }

        // E�er mute'luysa sesi kapat
        if (isMuted)
        {
            audioSource.volume = 0f;
        }
    }

    private void OnToggleChanged(bool isMuted)
    {
        if (isMuted)
        {
            // �u anki ses seviyesi kaydedilir, sonra s�f�rla
            previousVolume = audioSource.volume > 0f ? audioSource.volume : previousVolume;
            audioSource.volume = 0f;
            PlayerPrefs.SetInt(MutedKey, 1);
        }
        else
        {
            // �nceki seviyeyi geri ver
            audioSource.volume = previousVolume;
            PlayerPrefs.SetInt(MutedKey, 0);
        }

        PlayerPrefs.Save();
    }

    // D��ar�dan ses ayarlarsan (�rne�in slider), buradan g�ncelle
    public void SetVolume(float v)
    {
        // Mute de�ilse uygula
        if (muteToggle != null && muteToggle.isOn)
            return;

        audioSource.volume = v;
        previousVolume = v;
        PlayerPrefs.SetFloat(VolumeKey, v);
        PlayerPrefs.Save();
    }
}

