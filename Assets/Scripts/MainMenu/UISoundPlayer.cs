using UnityEngine;

public class UISoundPlayer : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip clickSound;

    void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void PlayClick()
    {
        audioSource.PlayOneShot(clickSound);
    }
}
