using DG.Tweening;
using UnityEngine;

public class Player_Minigame4 : MonoBehaviour
{
    //can do with one public speed or a enum that can check look soon wich one is better

    [HideInInspector] public enum MovementType
    {
        walking,
        potatoRunning,
        Running
    }

    [HideInInspector] public MovementType movementType = MovementType.walking;
    private CharacterController cc;
    LokaalConnecter.PlayerController playerController;
    [HideInInspector] public Vector3 velocity = Vector3.zero;

    //configs
    public int PlrId;
    const float defaultSpeed = 5f; //normal walkspeed
    const float runSpeed = 7f; //run speed
    const float potatoHoldSpeed = 5.5f; //speed with potato
    const float rotateSpeed = .7f; //speed of rotating ur character
    const float acceleration = 10f; //momento acceleration speed

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cc = GetComponent<CharacterController>();
        Minigame4Mangeren.instance.plrScripts.Add(PlrId, this);
        playerController = LokaalConnecter.plrsController[PlrId];
    }

    // Update is called once per frame
    void Update()
    {
        if (!Minigame4Mangeren.instance.isActive || GameMangeren.isPaused || !playerController.occuplied
         || LokaalConnecter.connectionType != LokaalConnecter.ConnectionTypes.nothing) return;

        Move();
    }

    //move the player
    void Move()
    {
        Vector2 moveDir = playerController.GetMoveDir();

        //move the plr
        Vector3 forward = Minigame4Mangeren.instance.camaraHolder.forward;
        forward.y = 0; //stop going up/down
        forward.Normalize();

        Vector3 right = Minigame4Mangeren.instance.camaraHolder.right;
        right.y = 0; //stop going up/down
        right.Normalize();

        //caculate the movement
        Vector3 move = (forward * moveDir.y + right * moveDir.x) * GetSpeed();
        velocity = Vector3.MoveTowards(velocity, move, acceleration * Time.deltaTime); //momento
        cc.Move(velocity * Time.deltaTime);
        LookAtMoveDir(move);
    }

    //let the player rotate based on where he move
    void LookAtMoveDir(Vector3 move)
    {
        // return if there is nothing
        if (move == Vector3.zero) return;

        //get the value's
        Quaternion dir = Quaternion.LookRotation(move);
        float angle = Quaternion.Angle(dir, transform.rotation);
        float dur = angle / (360 * rotateSpeed); //180 = max angle

        transform.DORotateQuaternion(dir, dur)
        .SetEase(Ease.Linear); //setting
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //check or it is a player
        Player_Minigame4 playerMinigame4 = hit.gameObject.GetComponent<Player_Minigame4>();
        if (playerMinigame4 == null) return;

        Minigame4Mangeren.instance.plrHittedPlr.Add((PlrId, playerMinigame4.PlrId));
    }

    //get the speed based on ur movementType
    float GetSpeed()
    {
        if (movementType == MovementType.walking) return defaultSpeed;
        else if (movementType == MovementType.potatoRunning) return potatoHoldSpeed;
        else return runSpeed;
    }
}
