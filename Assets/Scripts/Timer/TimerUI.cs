using TMPro;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TimerUI : MonoBehaviour
{
    private TextMeshProUGUI tmp;
    private Coroutine flashRoutine;
    private float currentFlashSpeed;
    private bool critical = false;
    private Vector3 baseScale;

    [Header("Timer Display Settings")]
    public string prefix = "Time";
    public Color normalColor = Color.white;
    public Color warningColor = Color.red;

    [Header("Thresholds (em segundos)")]
    public float warningTime = 10f;
    public float criticalTime = 6f;

    [Header("Flash Speeds")]
    public float warningFlashSpeed = 0.5f;
    public float criticalFlashSpeed = 0.25f; // mais rápido

    [Header("Pulse Settings (apenas crítico)")]
    public float pulseScale = 1.25f;   // quanto cresce
    public float pulseSpeed = 0.8f;    // velocidade da pulsação

    private void Awake()
    {
        tmp = GetComponent<TextMeshProUGUI>();
        baseScale = transform.localScale;
    }

    private void OnEnable()
    {
        StartCoroutine(WaitForManager());
    }

    IEnumerator WaitForManager()
    {
        while (TimerManager.Instance == null)
            yield return null;

        TimerManager.Instance.OnTimeChanged += HandleTimeChanged;
        HandleTimeChanged(TimerManager.Instance.CurrentTime);
    }

    private void OnDisable()
    {
        if (TimerManager.Instance != null)
            TimerManager.Instance.OnTimeChanged -= HandleTimeChanged;
    }

    void HandleTimeChanged(float currentTime)
    {
        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);
        tmp.text = $"{prefix}{minutes:00}: {seconds:00}";

        // Define velocidade de piscagem conforme o tempo
        float desiredSpeed = 0f;
        if (currentTime <= criticalTime){
            desiredSpeed = currentTime/10f;
            critical = true;

            ScreenShaker shake = Camera.main?.GetComponent<ScreenShaker>();
            if (shake != null)
            {
                // intensidade maior para ganhos, menor para perdas
                float mag = 0.03f;
                float dur = 0.15f;
               // shake.Shake(mag, dur);
            }
        }
        else if (currentTime <= warningTime){
            desiredSpeed = warningFlashSpeed;
        }
            

        // Atualiza piscagem se a velocidade desejada mudou
        if (desiredSpeed != currentFlashSpeed)
        {
            if (flashRoutine != null) StopCoroutine(flashRoutine);

            if (desiredSpeed > 0)
                flashRoutine = StartCoroutine(FlashText(desiredSpeed));
            else
                tmp.color = normalColor;

            currentFlashSpeed = desiredSpeed;
        }
    }

    IEnumerator FlashText(float speed)
    {
        while (true)
        {
            float t = Mathf.PingPong(Time.unscaledTime * (1f / speed), 1f);
            tmp.color = Color.Lerp(normalColor, warningColor, t);

            // pulsação de escala (somente no modo crítico)
            if (critical)
            {
                float pulseT = (Mathf.Sin(Time.unscaledTime * Mathf.PI * (1f / pulseSpeed)) + 1f) * 0.5f;
                float scale = Mathf.Lerp(1f, pulseScale, pulseT);
                transform.localScale = baseScale * scale;
            }
            else
            {
                transform.localScale = baseScale;
            }

            yield return null;
        }
    }
}
