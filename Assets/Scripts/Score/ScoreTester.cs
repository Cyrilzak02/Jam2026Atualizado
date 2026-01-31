using UnityEngine;
using UnityEngine.InputSystem; 

public class ScoreTester : MonoBehaviour
{
    [Tooltip("How many points to add when pressing Space")]
    public int gainAmount = 10;
    [Tooltip("How many points to subtract when pressing L")]
    public int lossAmount = 5;

    private void Update()
    {
        // Novo Input System usa Keyboard.current
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (ScoreManager.Instance != null)
                ScoreManager.Instance.AddScore(gainAmount);
        }

        if (Keyboard.current.lKey.wasPressedThisFrame)
        {
            if (ScoreManager.Instance != null)
                ScoreManager.Instance.SubtractScore(lossAmount);
        }
    }
}
