using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("Player Follow")]
    [SerializeField]
    private Vector3 playerOffset =
        new Vector3(0, 8, -6);

    [SerializeField] private float followSpeed = 5f;

    private Transform target;

    private bool usingFixedPosition = false;
    private Transform fixedCameraPosition;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;

        usingFixedPosition = false;
        fixedCameraPosition = null;
    }

    public void SetFixedPosition(Transform newPosition)
    {
        fixedCameraPosition = newPosition;

        usingFixedPosition = true;
        target = null;
    }

    private void LateUpdate()
    {
        if (usingFixedPosition)
        {
            if (fixedCameraPosition == null)
                return;

            transform.position = Vector3.Lerp(
                transform.position,
                fixedCameraPosition.position,
                followSpeed * Time.deltaTime
            );

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                fixedCameraPosition.rotation,
                followSpeed * Time.deltaTime
            );

            return;
        }

        if (target == null)
            return;

        Vector3 desiredPosition =
            target.position + playerOffset;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            followSpeed * Time.deltaTime
        );

        Vector3 direction =
            target.position - transform.position;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                followSpeed * Time.deltaTime
            );
        }
    }
}