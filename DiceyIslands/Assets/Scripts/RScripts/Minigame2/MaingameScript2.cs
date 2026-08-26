using UnityEngine;

public class MaingameScript2 : MonoBehaviour
{
    [SerializeField] int plrId;

    LokaalConnecter.PlayerController playerController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerController = LokaalConnecter.plrsController[plrId];
    }

    // Update is called once per frame
    void Update()
    {
        if (playerController == null || !playerController.occuplied) return;

        if (playerController.GetButtonDown(LokaalConnecter.InputType.x))
        {
            
        }
    }
}
