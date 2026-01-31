using System.Collections;
using TMPro;
using UnityEngine;


public class ScorePopAnimation : MonoBehaviour
{
    [Tooltip("Target RectTransform to animate. If empty, the component's RectTransform will be used.")]
    public RectTransform target;


    [Tooltip("Optional TextMeshPro to briefly tint during the pop.")]
    public TextMeshProUGUI tmpToColor;


    [Tooltip("Optional particle system to play on pop (e.g. small sparkles)")]
    public ParticleSystem popParticles;


    [Tooltip("Optional audio to play on pop")]
    public AudioClip popSfx;
    private AudioSource audioSource;


    [Header("Scale animation settings")]
    public float popScale = 1.6f;
    public float popInDuration = 0.08f;
    public float popOutDuration = 0.28f;


    [Header("Color flash settings")]
    public bool flashColor = true;
    public Color flashColorValue = Color.yellow;
    public float flashDuration = 0.18f;


    private Coroutine running;
    private Vector3 originalScale;
    private Color originalColor;


    private void Awake(){
        if (target == null) target = GetComponent<RectTransform>();
        if (tmpToColor == null) tmpToColor = GetComponent<TextMeshProUGUI>();


        originalScale = target != null ? target.localScale : Vector3.one;
        if (tmpToColor != null) originalColor = tmpToColor.color;


        if (popSfx != null){
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }


    public void Play(){
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(PopRoutine());


        if (popParticles != null) popParticles.Play();
        if (popSfx != null && audioSource != null) audioSource.PlayOneShot(popSfx);
    }


    private IEnumerator PopRoutine(){
        // scale up quickly
        float t = 0f;
        Vector3 from = originalScale;
        Vector3 to = originalScale * popScale;


        while (t < popInDuration){
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / popInDuration);
            // ease out
            float ease = Mathf.Sin(p * Mathf.PI * 0.5f);
            target.localScale = Vector3.LerpUnclamped(from, to, ease);
            yield return null;
        }


        // scale back with an overshoot/bounce feel
        t = 0f;
        while (t < popOutDuration){
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / popOutDuration);
            // ease out elastic-ish using Mathf.SmoothStep then + small overshoot
            float smooth = Mathf.SmoothStep(0f, 1f, p);
            float overshoot = Mathf.Sin(p * Mathf.PI) * 0.15f; // small wobble
            target.localScale = Vector3.LerpUnclamped(to, originalScale, smooth - overshoot);
            yield return null;
        }


        target.localScale = originalScale;


        // optional color flash
        if (flashColor && tmpToColor != null){
            t = 0f;
            while (t < flashDuration){
                t += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(t / flashDuration);
                tmpToColor.color = Color.Lerp(flashColorValue, originalColor, p);
                yield return null;
            }
            tmpToColor.color = originalColor;
        }


        running = null;
    }
}