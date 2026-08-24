using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField]
    private Vector3 offset =
        new Vector3(0, 8, -6);

    [SerializeField] private float followSpeed = 5f;

    private Transform target;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 desiredPosition =
            target.position + offset;

        transform.position =
            Vector3.Lerp(
                transform.position,
                desiredPosition,
                followSpeed * Time.deltaTime
            );

        transform.LookAt(target);
    }
}