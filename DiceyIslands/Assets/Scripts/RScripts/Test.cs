using UnityEngine;

public class Test : MonoBehaviour
{
    [SerializeField] int playerId;
    LokaalConnecter.PlayerController playercontroller;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playercontroller = LokaalConnecter.plrsController[playerId];
    }

    // Update is called once per frame
    void Update()
    {
        if (playercontroller.GetButtonDown(LokaalConnecter.InputType.x))
        {
            print("player " + playerId + " has pressed x");
        }
    }
}
