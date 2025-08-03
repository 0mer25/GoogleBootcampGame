using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Paneller")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject lobbyPanel;

    [Header("Butonlar")]
    [SerializeField] private Button btnPlay;
    [SerializeField] private Button btnBackFromLobby; // Lobby'den geri d�nmek i�in

    void Awake()
    {
        // Ba�lang��ta ana panel a��k, lobi kapal�
        mainPanel.SetActive(true);
        lobbyPanel.SetActive(false);

        btnPlay.onClick.AddListener(() => {
            mainPanel.SetActive(false);
            lobbyPanel.SetActive(true);
        });

        if (btnBackFromLobby != null)
            btnBackFromLobby.onClick.AddListener(() => {
                lobbyPanel.SetActive(false);
                mainPanel.SetActive(true);
            });
    }
}
