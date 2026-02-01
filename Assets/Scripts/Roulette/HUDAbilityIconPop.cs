using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HUDAbilityIconPop : MonoBehaviour
{
    public Image maskImage;          // A máscara que aparece
    public float popDuration = 0.25f;
    public float popStartScale = 1.5f; // tamanho inicial da animação

    private Coroutine popCoroutine;

    public void ShowAbilityIcon(Sprite sprite)
    {
        maskImage.sprite = sprite;
        maskImage.gameObject.SetActive(true);

        if (popCoroutine != null)
            StopCoroutine(popCoroutine);

        popCoroutine = StartCoroutine(PopIn());
    }

    public void HideAbilityIcon()
    {
        maskImage.gameObject.SetActive(false);
    }

    private IEnumerator PopIn()
    {
        float t = 0f;
        Vector3 startScale = Vector3.one * popStartScale;
        Vector3 endScale = Vector3.one * 0.30703f; // tamanho final correto
        maskImage.transform.localScale = startScale;

        while (t < popDuration)
        {
            t += Time.deltaTime;
            float progress = Mathf.Clamp01(t / popDuration);
            maskImage.transform.localScale = Vector3.Lerp(startScale, endScale, progress);
            yield return null;
        }

        // força o tamanho final exato
        maskImage.transform.localScale = endScale;
    }
}

