using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CollectibleItem : MonoBehaviour
{
    [Header("Collectible Settings")]
    public int scoreValue = 10;
    public AudioClip collectSound;
    public GameObject collectEffect;
    public float destroyDelay = 0.1f;

    private bool collected = false;

    void Reset()
    {
        // Garante que o collider seja configurado como trigger
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player") && !collected)
        {
            collected = true;
            Collect();
        }
    }

    void Collect()
    {
        // Adiciona pontuação
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(scoreValue);
            Debug.Log($"[Collectible] +{scoreValue} pontos (total: {ScoreManager.Instance.Score})");
        }

        // Som opcional
        if (collectSound != null)
            AudioSource.PlayClipAtPoint(collectSound, transform.position);

        // Efeito visual opcional
        if (collectEffect != null)
            Instantiate(collectEffect, transform.position, Quaternion.identity);

        // Desativa o visual e depois destrói
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;

        Destroy(gameObject, destroyDelay);
    }
}
