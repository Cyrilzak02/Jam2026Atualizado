using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 6f;

    float fixedY;
    float z;

    void Start()
    {
        fixedY = transform.position.y;
        z = transform.position.z;
    }

    void LateUpdate()
    {
        if (target == null) return;

        float smoothX = Mathf.Lerp(
            transform.position.x,
            target.position.x,
            smoothSpeed * Time.deltaTime
        );

        transform.position = new Vector3(
            smoothX,
            fixedY,
            z
        );
    }
}
