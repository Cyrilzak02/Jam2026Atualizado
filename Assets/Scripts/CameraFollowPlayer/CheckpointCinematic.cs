using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


[RequireComponent(typeof(Collider2D))]
public class CheckpointCinematic : MonoBehaviour
{
    [Header("Camera References")]
    public FollowPlayer followScript;
    public Transform cameraTransform;
    public float cameraMoveDistance = 3f;
    public float cameraMoveDuration = 2f;

    [Header("Player Reference")]
    public PlayerRespawnController playerRespawn;

    [Header("Text UI")]
    public TextMeshProUGUI cinematicText;   // Texto do seu Canvas padrão
    [TextArea] public string mainMessage = "Você chegou ao fim de sua jornada.";
    public float textFadeDuration = 1.5f;
    public float textDisplayTime = 5f;

    [Header("Score / Timer")]
    public TimerManager timer;
    public ScoreManager playerScore;
    public GameObject scoreText;
    public GameObject timerText;

    [Header("Optional")]
    public CameraEndEffect cameraEndEffect;
    public float delayBeforeCameraEffect = 2f;
    public AudioSource mscFaseFinal;

    private bool triggered = false;
    private Color originalColor;

    void Start()
    {
        if (cinematicText != null)
        {
            originalColor = cinematicText.color;
            cinematicText.text = "";
            cinematicText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        }

        if (timer == null && TimerManager.Instance != null)
            timer = TimerManager.Instance;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (triggered) return;

        if (col.CompareTag("Player"))
        {
            triggered = true;
            StartCoroutine(CinematicSequence());
        }
    }

    IEnumerator CinematicSequence()
    {
        Debug.Log("[CheckpointCinematic] Sequência iniciada!");

        // 1️⃣ Para o follow da câmera
        if (followScript != null)
            followScript.enabled = false;

        // 2️⃣ Trava completamente o player
        if (playerRespawn != null)
        {
            playerRespawn.dying = true;

            Rigidbody2D rb = playerRespawn.GetComponent<Rigidbody2D>();
            Animator anim = playerRespawn.GetComponent<Animator>();
            PlayerController2D playerController = playerRespawn.GetComponent<PlayerController2D>();

            playerController.mscFase2.Stop();
            mscFaseFinal.Play();


            anim.SetInteger("NoMask", 0); // Sem máscara (Idle)
            if (rb != null)
                rb.linearVelocity = Vector2.zero;

            PlayerController2D controller = playerRespawn.GetComponent<PlayerController2D>();
            PlayerAbilities abilities = playerRespawn.GetComponent<PlayerAbilities>();

            if (controller != null)
                controller.enabled = false;
            if (abilities != null)
                abilities.enabled = false;
        }

        // 3️⃣ Move a câmera suavemente
        if (cameraTransform != null)
        {
            scoreText.SetActive(false);
            timerText.SetActive(false);
            Vector3 startPos = cameraTransform.position;
            Vector3 endPos = startPos - new Vector3(cameraMoveDistance, 0, 0);
            float t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime / cameraMoveDuration;
                cameraTransform.position = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0, 1, t));
                yield return null;
            }
        }

        // 4️⃣ Monta texto com score + tempo
        string timeText = FormatTime(timer != null ? (timer.startTime - timer.CurrentTime) : 0f);
        string finalText =
            $"{mainMessage}\n\n" +
            $"<size=65%><color=#000000>🏆 Pontuação: {playerScore.Score}</color></size>\n" +
            $"<size=65%><color=#000000>⏱ Tempo total: {timeText}</color></size>";

        // 5️⃣ Fade direto no texto TMP
        if (cinematicText != null)
        {
            cinematicText.text = finalText;

            // fade in
            yield return StartCoroutine(FadeTMPText(0f, 1f, textFadeDuration));

            // mantém visível
            yield return new WaitForSeconds(textDisplayTime);

            // fade out
            yield return StartCoroutine(FadeTMPText(1f, 0f, textFadeDuration));

            cinematicText.text = "";
        }

        // 6️⃣ (Opcional) Efeito de câmera
        if (cameraEndEffect != null)
        {
            yield return new WaitForSeconds(delayBeforeCameraEffect);
            cameraEndEffect.OnTimerEnded();
        }

        Debug.Log("[CheckpointCinematic] Cena final concluída.");
        // 7️⃣ Espera 10 segundos e troca para o menu
        yield return new WaitForSeconds(10f);
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");

    }

    IEnumerator FadeTMPText(float startAlpha, float targetAlpha, float duration)
    {
        float time = 0f;
        Color c = cinematicText.color;

        while (time < duration)
        {
            time += Time.deltaTime;
            c.a = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            cinematicText.color = c;
            yield return null;
        }

        c.a = targetAlpha;
        cinematicText.color = c;
    }

    string FormatTime(float totalSeconds)
    {
        int minutes = Mathf.FloorToInt(totalSeconds / 60f);
        int seconds = Mathf.FloorToInt(totalSeconds % 60f);
        return $"{minutes:00}:{seconds:00}";
    }
}
