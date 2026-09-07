using UnityEngine;

public class TargetScript : MonoBehaviour
{
    [SerializeField] float spinSpeed = 90f;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.Self);
    }
}
