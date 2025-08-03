using UnityEngine;
using UnityEngine.Video;
using Unity.Netcode; // veya Photon PUN2

public class EndGame : NetworkBehaviour // veya MonoBehaviourPunPV
{
    [Header("Video")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private GameObject videoCanvas;

    private bool gameEnded = false;

    private void OnTriggerEnter(Collider other)
    {
        // Sadece server EndGame'i kontrol eder
        if (!IsServer) return; // Unity Netcode için
                               // if (!PhotonNetwork.IsMasterClient) return; // Photon için

        if (other.CompareTag("Player") && !gameEnded)
        {
            gameEnded = true;

            // Tüm client'lara EndGame baþlat
            TriggerEndGameClientRpc();
        }
    }

    [ClientRpc] // Unity Netcode
    // [PunRPC] // Photon için
    private void TriggerEndGameClientRpc()
    {
        StartEndGameSequence();
    }

    private void StartEndGameSequence()
    {
        // Video canvas'ýný aktif et
        videoCanvas.SetActive(true);
       

        // Video bittiðinde oyunu kapat
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoFinished;
            videoPlayer.Play();
        }

        // Oyuncu kontrollerini deaktif et
        DisablePlayerControls();
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        // Oyunu kapat
        QuitGame();
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    private void DisablePlayerControls()
    {
        // Tüm oyuncu kontrollerini deaktif et
        // Örnek: Input sistemini kapat, movement scriptlerini durdur
    }
}