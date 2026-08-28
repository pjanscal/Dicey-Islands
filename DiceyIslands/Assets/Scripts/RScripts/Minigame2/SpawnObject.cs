using UnityEngine;

public class SpawnObject : MonoBehaviour
{
    [SerializeField] GameObject objectToSpawnParent;
    public GameObject objectToActivate;

    void Start()
    {
        if (LokaalConnecter.connectionType != LokaalConnecter.ConnectionTypes.nothing) return;
        if (FindFirstObjectByType<SpawnObject>() != this) return;

        StartCoroutine(SpawnTimer());
    }

    System.Collections.IEnumerator SpawnTimer()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(3, 6));
            SpawnRandomObject();
        }
    }

    public void SpawnRandomObject()
    {
        foreach (Transform child in objectToSpawnParent.transform)
        {
            child.gameObject.SetActive(false);
        }
        objectToActivate = objectToSpawnParent.transform.GetChild(Random.Range(0, objectToSpawnParent.transform.childCount)).gameObject;
        Debug.Log(objectToActivate.name);
        objectToActivate.SetActive(true);
    }
}
