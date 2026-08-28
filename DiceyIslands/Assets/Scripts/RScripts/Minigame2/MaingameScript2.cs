using UnityEngine;
using System.Text.RegularExpressions;

public class MaingameScript2 : MonoBehaviour
{
    [SerializeField] int plrId;
    SpawnObject spawnObject;
    public int points;
    LokaalConnecter.PlayerController playerController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spawnObject = FindFirstObjectByType<SpawnObject>();

        playerController = LokaalConnecter.plrsController[plrId];

    }

    // Update is called once per frame
    void Update()
    {
        if (LokaalConnecter.connectionType == LokaalConnecter.ConnectionTypes.nothing)
        {
            if (playerController == null || !playerController.occuplied) return;

            if (playerController.GetButtonDown(LokaalConnecter.InputType.x))
            {
                if (spawnObject.objectToActivate == null) return;

                Match pointMatch = Regex.Match(spawnObject.objectToActivate.name, @"-?\d+");
                if (pointMatch.Success)
                {
                    points += int.Parse(pointMatch.Value);
                    Debug.Log("Points: " + points);
                }

                spawnObject.objectToActivate.SetActive(false);
            }
        }
    }

}
