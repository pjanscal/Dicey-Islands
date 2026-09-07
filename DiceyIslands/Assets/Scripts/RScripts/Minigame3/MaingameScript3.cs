using UnityEngine;

public class MaingameScript3 : MonoBehaviour
{
    [SerializeField] int plrId;
    LokaalConnecter.PlayerController playerController;
    [SerializeField] float rayDistance = 10f;
    [SerializeField] LayerMask raycastLayers = ~0;
    [SerializeField] float rayDisplayDuration = 1f;
    LineRenderer rayLine;
    float rayVisibleUntil;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerController = LokaalConnecter.plrsController[plrId];

        GameObject rayObject = new GameObject("Memory Raycast");
        rayObject.transform.SetParent(transform);
        rayLine = rayObject.AddComponent<LineRenderer>();
        rayLine.positionCount = 2;
        rayLine.startWidth = 0.05f;
        rayLine.endWidth = 0.05f;
        rayLine.material = new Material(Shader.Find("Sprites/Default"));
        rayLine.startColor = Color.white;
        rayLine.endColor = Color.white;
        rayLine.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerController == null || !playerController.occuplied) return;

        if (playerController.GetButtonDown(LokaalConnecter.InputType.x))
        {
            ShowRaycast();
        }

        if (rayLine != null && Time.time >= rayVisibleUntil)
        {
            rayLine.enabled = false;
        }
    }

    void ShowRaycast()
    {
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;
        float distance = Mathf.Max(0f, rayDistance);
        Vector3 endPoint = origin + direction * distance;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, distance, raycastLayers))
        {
            endPoint = hit.point;
        }

        rayLine.SetPosition(0, origin);
        rayLine.SetPosition(1, endPoint);
        rayLine.enabled = true;
        rayVisibleUntil = Time.time + rayDisplayDuration;
    }
}
