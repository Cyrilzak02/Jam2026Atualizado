using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private bool activated = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated) return;

        if (other.CompareTag("Player"))
        {
            PlayerRespawnController respawn =
                other.GetComponent<PlayerRespawnController>();

            if (respawn != null)
            {
                respawn.SetCheckpoint(transform.position);
                activated = true;

                Debug.Log("Checkpoint reached!");
            }
        }
    }
}
