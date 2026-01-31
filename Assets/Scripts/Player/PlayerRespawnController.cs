using UnityEngine;

public class PlayerRespawnController : MonoBehaviour
{
    [Header("Respawn Settings")]
    public float fallDeathY = -10f;

    private Vector3 lastCheckpointPosition;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Start position = first checkpoint
        lastCheckpointPosition = transform.position;
    }

    void Update()
    {
        // If player falls below threshold
        if (transform.position.y < fallDeathY)
        {
            Respawn();
        }
    }

    public void SetCheckpoint(Vector3 checkpointPosition)
    {
        lastCheckpointPosition = checkpointPosition;
    }

    void Respawn()
    {
        // Reset physics
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // Teleport player
        transform.position = lastCheckpointPosition;
    }
}

