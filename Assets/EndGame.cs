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
        if (!IsServer) return; // Unity Netcode i�in
                               // if (!PhotonNetwork.IsMasterClient) return; // Photon i�in

        if (other.CompareTag("Player") && !gameEnded)
        {
            gameEnded = true;

            // T�m client'lara EndGame ba�lat
            TriggerEndGameClientRpc();
        }
    }

    [ClientRpc] // Unity Netcode
    // [PunRPC] // Photon i�in
    private void TriggerEndGameClientRpc()
    {
        StartEndGameSequence();
    }

    private void StartEndGameSequence()
    {
        // Video canvas'�n� aktif et
        videoCanvas.SetActive(true);
       

        // Video bitti�inde oyunu kapat
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
        // T�m oyuncu kontrollerini deaktif et
        // �rnek: Input sistemini kapat, movement scriptlerini durdur
    }
}