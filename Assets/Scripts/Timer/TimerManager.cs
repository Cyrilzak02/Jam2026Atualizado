using System;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    public static TimerManager Instance { get; private set; }

    [Header("Timer Settings")]
    public float startTime = 60f;
    public bool autoStart = true;

    public float CurrentTime { get; private set; }
    public bool IsRunning { get; private set; }

    public event Action<float> OnTimeChanged;
    public event Action OnTimerEnded;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        CurrentTime = startTime;
    }

    private void Start()
    {
        if (autoStart)
        {
            StartTimer();
            Debug.Log("[TimerManager] Timer iniciado automaticamente.");
        }
        else
        {
            Debug.Log("[TimerManager] AutoStart está desativado.");
        }

        OnTimeChanged?.Invoke(CurrentTime);
    }

    public void StartTimer() => IsRunning = true;
    public void PauseTimer() => IsRunning = false;

    private void Update()
    {
        if (!IsRunning) return;

        CurrentTime -= Time.deltaTime;

        // Impede valores negativos
        if (CurrentTime <= 0f)
        {
            CurrentTime = 0f;
            IsRunning = false;
            OnTimeChanged?.Invoke(CurrentTime);
            OnTimerEnded?.Invoke();
            Debug.Log("[TimerManager] Tempo acabou!");
            return;
        }

        OnTimeChanged?.Invoke(CurrentTime);
    }

    public void ResetAndStartTimer()
    {
        CurrentTime = startTime;
        IsRunning = true;
        OnTimeChanged?.Invoke(CurrentTime);
        Debug.Log("[TimerManager] Timer resetado e iniciado.");
    }


}
