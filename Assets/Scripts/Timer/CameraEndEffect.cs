using UnityEngine;
using System.Collections;

public class CameraEndEffect : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform player;  // referência ao player (arraste aqui)

    [Header("Rotation Settings")]
    public float rotationSpeed = 30f;   // graus por segundo
    public float tiltAngle = 10f;       // inclinação da câmera

    [Header("Zoom Settings")]
    public float zoomDuration = 4f;     // tempo até o zoom completar
    public float zoomAmount = 3f;       // o quanto a câmera se aproxima
    public float zoomSmooth = 2f;       // suavização da interpolação

    private bool isZooming = false;
    private Camera cam;
    private float originalSize;
    private float targetSize;
    private Quaternion originalRot;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        originalRot = transform.rotation;

        if (cam.orthographic)
        {
            originalSize = cam.orthographicSize;
            targetSize = Mathf.Max(0.1f, originalSize - zoomAmount);
        }
        else
        {
            originalSize = cam.fieldOfView;
            targetSize = Mathf.Max(1f, originalSize - zoomAmount);
        }
    }

    private void OnEnable()
    {
        StartCoroutine(WaitForTimer());
    }

    IEnumerator WaitForTimer()
    {
        while (TimerManager.Instance == null)
            yield return null;

        TimerManager.Instance.OnTimerEnded += OnTimerEnded;
        Debug.Log("[CameraEndEffect] Conectado ao TimerManager.");
    }

    private void OnDisable()
    {
        if (TimerManager.Instance != null)
            TimerManager.Instance.OnTimerEnded -= OnTimerEnded;
    }

    private void OnTimerEnded()
    {
        Debug.Log("[CameraEndEffect] Timer chegou a zero — iniciando efeito de câmera...");
        if (player == null)
        {
            Debug.LogWarning("[CameraEndEffect] Nenhum player atribuído!");
            return;
        }

        if (!isZooming)
            StartCoroutine(EndEffect());
    }

    IEnumerator EndEffect()
    {
        isZooming = true;

        float elapsed = 0f;

        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;

            // gira a câmera em torno do jogador
            transform.RotateAround(player.position, Vector3.forward, rotationSpeed * Time.deltaTime);

            // faz a câmera olhar para o jogador com leve inclinação
            Vector3 dir = (player.position - transform.position).normalized;
            Quaternion targetRot = Quaternion.LookRotation(Vector3.forward, dir); // 2D-style look
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot * Quaternion.Euler(0f, 0f, tiltAngle), Time.deltaTime * zoomSmooth);

            // zoom
            if (cam.orthographic)
            {
                cam.orthographicSize = Mathf.Lerp(originalSize, targetSize, elapsed / zoomDuration);
            }
            else
            {
                cam.fieldOfView = Mathf.Lerp(originalSize, targetSize, elapsed / zoomDuration);
            }

            yield return null;
        }

        isZooming = false;
        Debug.Log("[CameraEndEffect] Efeito finalizado.");
    }

    // teste manual
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
            OnTimerEnded();
    }
}
