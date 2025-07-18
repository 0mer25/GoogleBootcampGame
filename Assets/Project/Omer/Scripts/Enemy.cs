using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float moveSpeed = 3f;
    private Transform player;
    [SerializeField] private int scoreValue = 10;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        if (player == null) return;

        Vector3 direction = (player.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        // Yüzünü oyuncuya çevir (isteğe bağlı)
        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Oyuncuya çarpıldı! Oyun bitti!");
            Time.timeScale = 0f; // Oyun dursun
        }
        else if (other.CompareTag("Bullet"))
        {
            ScoreManager.Instance.AddScore(scoreValue); // Skor ekle

            CameraShaker.Instance.Shake(.2f, .2f); // Kamera sarsıntısı
            Destroy(other.gameObject); // Mermiyi yok et
            Destroy(gameObject); // Düşmanı yok et
        }
    }
}
