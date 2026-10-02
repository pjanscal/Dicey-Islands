using UnityEngine;

public class SpawnObject : MonoBehaviour
{
    [SerializeField] GameObject objectToSpawnParent;

    public GameObject objectToActivate;

    Coroutine spawnCoroutine;

    private Minigame2ResultBridge resultBridge;

    private void Start()
    {
        resultBridge =
            FindFirstObjectByType<Minigame2ResultBridge>();
    }

    public void StartSpawning()
    {
        if (LokaalConnecter.connectionType !=
            LokaalConnecter.ConnectionTypes.nothing)
            return;

        if (spawnCoroutine != null)
            return;

        spawnCoroutine =
            StartCoroutine(SpawnTimer());
    }

    System.Collections.IEnumerator SpawnTimer()
    {
        while (true)
        {
            yield return new WaitForSeconds(
                Random.Range(2, 5)
            );

            SpawnRandomObject();
        }
    }

    public void SpawnRandomObject()
    {
        foreach (
            Transform child
            in objectToSpawnParent.transform
        )
        {
            child.gameObject.SetActive(false);
        }

        Transform chosenChild =
            objectToSpawnParent.transform.GetChild(
                Random.Range(
                    0,
                    objectToSpawnParent.transform.childCount
                )
            );

        objectToActivate =
            chosenChild.gameObject;

        objectToActivate.SetActive(true);

        // Play pickup spawn SFX.
        if (resultBridge != null)
        {
            resultBridge.PlayPickupSpawnSound();
        }
    }

    public bool TryClaimObject(
        out GameObject claimedObject
    )
    {
        claimedObject = objectToActivate;

        if (claimedObject == null ||
            !claimedObject.activeSelf)
            return false;

        claimedObject.SetActive(false);

        objectToActivate = null;

        return true;
    }

    public void StopSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        if (objectToActivate != null)
        {
            objectToActivate.SetActive(false);
        }
    }
}