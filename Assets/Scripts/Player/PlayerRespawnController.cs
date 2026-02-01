using UnityEngine;
using System.Collections;

public class PlayerRespawnController : MonoBehaviour
{
    [Header("Respawn Settings")]
    public float fallDeathY = -10f;
    public float fadeDuration = 1.2f;
    public float deathAnimDuration = 1f;

    [Header("Fade Reference")]
    public CanvasGroup fadeCanvasGroup;

    private Vector3 lastCheckpointPosition;
    private Rigidbody2D rb;
    private Animator anim;
    private PlayerController2D playerController;
    private PlayerAbilities playerAbilities;
    public TimerManager timer;
    public ScoreManager score; // pode ser arrastado no inspector

    [HideInInspector] public bool dying = false;
    private bool isRespawning = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        playerController = GetComponent<PlayerController2D>();
        playerAbilities = GetComponent<PlayerAbilities>();

        // conecta automaticamente ao ScoreManager se houver um na cena
        if (score == null && ScoreManager.Instance != null)
            score = ScoreManager.Instance;

        lastCheckpointPosition = transform.position;

        if (anim != null)
            anim.updateMode = AnimatorUpdateMode.UnscaledTime;
    }

    void Update()
    {
        if (isRespawning) return;

        if (transform.position.y < fallDeathY || (timer != null && timer.CurrentTime <= 0))
        {
            Debug.Log("[Respawn] Chamando rotina de respawn...");
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
        dying = true;

        Time.timeScale = 0f;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.simulated = false;

        if (timer != null)
            timer.PauseTimer();

        if (playerController != null)
            playerController.enabled = false;
        if (playerAbilities != null)
            playerAbilities.enabled = false;

        // 🧮 Reseta o score
        if (score == null && ScoreManager.Instance != null)
            score = ScoreManager.Instance;

        if (score != null)
        {
            score.ResetScore();
            Debug.Log("[Respawn] Score resetado após morte!");
        }
        else
        {
            Debug.LogWarning("[Respawn] Nenhum ScoreManager encontrado!");
        }

        // animação de morte
        if (anim != null)
        {
            anim.updateMode = AnimatorUpdateMode.UnscaledTime;
            anim.SetInteger("NoMask", 6);
        }

        yield return new WaitForSecondsRealtime(deathAnimDuration);

        yield return StartCoroutine(Fade(1f, fadeDuration, true));

        transform.position = lastCheckpointPosition;

        yield return new WaitForSecondsRealtime(0.5f);
        yield return StartCoroutine(Fade(0f, fadeDuration, true));

        rb.simulated = true;
        Time.timeScale = 1f;

        if (playerController != null)
            playerController.enabled = true;
        if (playerAbilities != null)
            playerAbilities.enabled = true;

        if (timer != null)
            timer.ResetAndStartTimer();

        dying = false;
        if (anim != null)
        {
            anim.updateMode = AnimatorUpdateMode.Normal;
            anim.SetInteger("NoMask", 0);
        }

        isRespawning = false;
    }

    IEnumerator Fade(float targetAlpha, float duration, bool useUnscaledTime = false)
    {
        float startAlpha = fadeCanvasGroup.alpha;
        float time = 0f;

        while (time < duration)
        {
            time += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;
    }
}
