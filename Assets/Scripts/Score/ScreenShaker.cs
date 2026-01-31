using UnityEngine;
using System.Collections;

public class ScreenShaker : MonoBehaviour
{
    private Vector3 originalPos;
    private Coroutine shakeRoutine;

    [Header("Shake Settings")]
    public float duration = 0.15f;     // tempo da tremida
    public float magnitude = 0.1f;     // intensidade da tremida

    private void Awake()
    {
        originalPos = transform.localPosition;
    }

    public void Shake(float customMagnitude = -1f, float customDuration = -1f)
    {
        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        float mag = (customMagnitude > 0) ? customMagnitude : magnitude;
        float dur = (customDuration > 0) ? customDuration : duration;
        shakeRoutine = StartCoroutine(DoShake(dur, mag));
    }

    private IEnumerator DoShake(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = originalPos + new Vector3(x, y, 0f);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
        shakeRoutine = null;
    }
}
