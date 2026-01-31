using UnityEngine;
using System.Collections;

public class PlayerRespawnController : MonoBehaviour
{
    [Header("Respawn Settings")]
    public float fallDeathY = -10f;
    public float fadeDuration = 2f;

    [Header("Fade Reference")]
    public CanvasGroup fadeCanvasGroup;

    private Vector3 lastCheckpointPosition;
    private Rigidbody2D rb;
    private bool isRespawning = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        lastCheckpointPosition = transform.position;
    }

    void Update()
    {
        if (isRespawning) return;

        if (transform.position.y < fallDeathY)
        {
            StartCoroutine(RespawnRoutine());
        }
    }

    public void SetCheckpoint(Vector3 checkpointPosition)
    {
        lastCheckpointPosition = checkpointPosition;
    }

    IEnumerator RespawnRoutine()
    {
        isRespawning = true;

        // Fade OUT
        yield return StartCoroutine(Fade(1f));

        // Reset physics
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // Teleport
        transform.position = lastCheckpointPosition;

        // Small pause (optional but feels better)
        yield return new WaitForSeconds(0.8f);

        // Fade IN
        yield return StartCoroutine(Fade(0f));
        
        isRespawning = false;
    }

    IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = fadeCanvasGroup.alpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(
                startAlpha,
                targetAlpha,
                time / fadeDuration
            );
            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;
    }
}
