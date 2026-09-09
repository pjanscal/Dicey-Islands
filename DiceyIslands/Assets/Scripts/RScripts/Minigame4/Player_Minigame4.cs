using UnityEngine;

public class Player_Minigame4 : MonoBehaviour
{
    [HideInInspector] public float speed;

    //configs
    [SerializeField] private int PlrId;

    const float defaultSpeed = 5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        speed = defaultSpeed;
        Minigame4Mangeren.instance.plrScripts.Add(PlrId, this);
    }

    // Update is called once per frame
    void Update()
    {
        if (!Minigame4Mangeren.instance.isActive) return;

        Move();
    }

    //move the player
    void Move()
    {
        LokaalConnecter.PlayerController playerController = LokaalConnecter.plrsController[PlrId];

        Vector2 moveDir = playerController.GetMoveDir();
        if (moveDir == Vector2.zero) return;

        //move the plr
        Vector3 forward = Minigame4Mangeren.instance.camaraHolder.forward;
        forward.y = 0; //stop going up/down
        forward.Normalize();

        Vector3 right = Minigame4Mangeren.instance.camaraHolder.right;
        right.y = 0; //stop going up/down
        right.Normalize();

        transform.position += (forward * moveDir.y + right * moveDir.x) * speed * Time.deltaTime;
    }
}
