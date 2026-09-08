using UnityEngine;

public class MaingameScript3 : MonoBehaviour
{
    [SerializeField] int plrId;
    LokaalConnecter.PlayerController playerController;
    [SerializeField] float rayDistance = 10f;
    [SerializeField] LayerMask raycastLayers = ~0;
    [SerializeField] GameObject hitPrefab;
    [SerializeField] Vector3 rotationOffset;
    GameObject spawnedSword;
    bool canShoot = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerController = LokaalConnecter.plrsController[plrId];
    }

    // Update is called once per frame
    void Update()
    {
        if (LokaalConnecter.connectionType == LokaalConnecter.ConnectionTypes.nothing)
        {
            if (playerController == null || !playerController.occuplied) return;

            if (canShoot && playerController.GetButtonDown(LokaalConnecter.InputType.x))
            {
                SpawnAtRaycastHit();
            }
        }
    }

    void SpawnAtRaycastHit()
    {
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;
        float distance = Mathf.Max(0f, rayDistance);

        if (Physics.Raycast(origin, direction, out RaycastHit hit, distance, raycastLayers))
        {
            if (hit.transform.CompareTag("Sword"))
            {
                canShoot = false;
                return;
            }

            if (hitPrefab == null) return;

            Quaternion rotation = Quaternion.FromToRotation(Vector3.down, hit.normal) * Quaternion.Euler(rotationOffset);
            spawnedSword = Instantiate(hitPrefab, hit.point, rotation);
            spawnedSword.transform.SetParent(hit.transform, true);
        }
    }
}
