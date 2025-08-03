using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Paneller")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject lobbyPanel;

    [Header("Butonlar")]
    [SerializeField] private Button btnPlay;
    [SerializeField] private Button btnBackFromLobby; // Lobby'den geri dönmek için

    void Awake()
    {
        // Baþlangýçta ana panel açýk, lobi kapalý
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
