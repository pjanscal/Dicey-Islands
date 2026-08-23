using UnityEngine;
using UnityEngine.InputSystem;

public class PlrControllTesting : MonoBehaviour
{
    [SerializeField] private int plrId;

    private LokaalConnecter.PlayerController playerController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerController = LokaalConnecter.plrsController[plrId];
    }

    // Update is called once per frame
    void Update()
    {
        //return if this is nothing
        if (playerController == null || !playerController.occuplied) return;

        Vector2 movedir = playerController.GetMoveDir();
        transform.position += new Vector3(movedir.x, movedir.y, 0) * Time.deltaTime;

        if (playerController.GetButtonDown(LokaalConnecter.InputType.jump)) print($"plr{plrId} has pressed jump");
        if (playerController.GetButtonUp(LokaalConnecter.InputType.jump)) print($"plr{plrId} has Released jump");
    }
}
