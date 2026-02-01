using TMPro;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(TextMeshProUGUI))]
public class ScoreUI : MonoBehaviour
{
    private TextMeshProUGUI tmp;
    private Coroutine animRoutine;
    private Vector3 baseScale;
    private Color baseColor;

    [Header("Animation Settings")]
    public float scaleAmount = 1.3f;   // quanto o texto cresce
    public float animSpeed = 0.25f;    // velocidade da animação
    public string prefix = "Score";
    public Color gainColor = Color.green;
    public Color lossColor = Color.red;

    void Awake()
    {
        tmp = GetComponent<TextMeshProUGUI>();
        baseScale = transform.localScale;
        baseColor = tmp.color;
    }

    void OnEnable()
    {
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnScoreChanged += HandleScoreChange;
    }

    void OnDisable()
    {
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnScoreChanged -= HandleScoreChange;
    }

    void HandleScoreChange(int newScore, int delta)
    {
        tmp.text = $"{prefix}: {newScore.ToString()}";
        if (animRoutine != null)
            StopCoroutine(animRoutine);

        animRoutine = StartCoroutine(Animate(delta));

        // chama o shake da câmera ----
        if (delta != 0)
        {
            ScreenShaker shake = Camera.main?.GetComponent<ScreenShaker>();
            if (shake != null)
            {
                // intensidade maior para ganhos, menor para perdas
                float mag = delta > 0 ? 0.1f : 0.07f;
                float dur = 0.15f;
              //  shake.Shake(mag, dur);
            }
        }
    }

    IEnumerator Animate(int delta)
    {
        // Escolhe cor conforme ganho/perda
        Color targetColor = delta >= 0 ? gainColor : lossColor;
        Vector3 targetScale = baseScale * scaleAmount;

        float t = 0;
        // Cresce e muda cor
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / animSpeed;
            transform.localScale = Vector3.Lerp(baseScale, targetScale, Mathf.Sin(t * Mathf.PI * 0.5f));
            tmp.color = Color.Lerp(baseColor, targetColor, Mathf.Sin(t * Mathf.PI));
            yield return null;
        }

        // Volta ao normal
        t = 0;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / (animSpeed * 1.2f);
            transform.localScale = Vector3.Lerp(targetScale, baseScale, Mathf.SmoothStep(0, 1, t));
            tmp.color = Color.Lerp(targetColor, baseColor, t);
            yield return null;
        }

        transform.localScale = baseScale;
        tmp.color = baseColor;
    }
}
