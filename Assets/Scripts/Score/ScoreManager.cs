using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [SerializeField] private int startScore = 0;
    [SerializeField] private int minScore = 0;

    public int Score { get; private set; }
    public event Action<int, int> OnScoreChanged; // agora também envia delta (mudança)

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        Score = startScore;
    }

    private void Start()
    {
        OnScoreChanged?.Invoke(Score, 0);
    }

    public void AddScore(int amount)
    {
        int oldScore = Score;
        Score += amount;
        if (Score < minScore) Score = minScore;
        int delta = Score - oldScore;
        OnScoreChanged?.Invoke(Score, delta);
    }

    public void SubtractScore(int amount)
    {
        AddScore(-amount);
    }
}
