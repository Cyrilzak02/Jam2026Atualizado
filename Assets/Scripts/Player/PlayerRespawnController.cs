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

    [HideInInspector] public bool dying = false;
    private bool isRespawning = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        playerController = GetComponent<PlayerController2D>();
        playerAbilities = GetComponent<PlayerAbilities>();

        lastCheckpointPosition = transform.position;

        // 🔥 Torna o Animator independente do Time.timeScale
        if (anim != null)
            anim.updateMode = AnimatorUpdateMode.UnscaledTime;
    }

    void Update()
    {
        if (isRespawning) return;

        if (transform.position.y < fallDeathY || (timer != null && timer.CurrentTime <= 0))
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
        dying = true;

        // 🔥 Pausa o tempo e a física
        Time.timeScale = 0f;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.simulated = false;

        // 🔥 Pausa o timer
        if (timer != null)
            timer.PauseTimer();

        // 🔥 Bloqueia controles e troca de habilidades
        if (playerController != null)
            playerController.enabled = false;
        if (playerAbilities != null)
            playerAbilities.enabled = false;

        // 🔥 Animação de morte (roda em tempo real)
        if (anim != null)
        {
            anim.updateMode = AnimatorUpdateMode.UnscaledTime; // garante independência do timeScale
            anim.SetInteger("NoMask", 6);
        }

        // Espera 2 segundos em tempo real
        yield return new WaitForSecondsRealtime(deathAnimDuration);

        // Fade OUT
        yield return StartCoroutine(Fade(1f, fadeDuration, true));

        // Teleporta o jogador para o último checkpoint
        transform.position = lastCheckpointPosition;

        // Fade IN
        yield return new WaitForSecondsRealtime(0.5f);
        yield return StartCoroutine(Fade(0f, fadeDuration, true));

        // 🔁 Restaura física e controles
        rb.simulated = true;
        Time.timeScale = 1f;

        if (playerController != null)
            playerController.enabled = true;
        if (playerAbilities != null)
            playerAbilities.enabled = true;

        // 🔁 Reseta o timer
        if (timer != null)
            timer.ResetAndStartTimer();

        // 🔁 Retorna à idle
        dying = false;
        if (anim != null)
        {
            anim.updateMode = AnimatorUpdateMode.Normal; // volta ao modo padrão
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
