using DG.Tweening;
using UnityEngine;

//shortcut
using BooleanEvents = CharacterLoader.CharactersAnimationBooleanEvent;

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
    private CharacterLoader animatorController;

    //configs
    public int plrId;
    //--times 2 for the extra size :3
    const float defaultSpeed = 5f * 2; //normal walkspeed
    const float runSpeed = 7f * 2; //run speed
    const float potatoHoldSpeed = 5.5f * 2; //speed with potato
    const float rotateSpeed = .7f; //speed of rotating ur character
    const float acceleration = 12f * 2; //momento acceleration speed

    //animation
    const float minDistanceToRun = .5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cc = GetComponent<CharacterController>();
        Minigame4Mangeren.instance.plrScripts.Add(plrId, this);
        playerController = LokaalConnecter.plrsController[plrId];
        animatorController = GameMangeren.GetCharacterLoaderFromId(plrId);
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

        //animation
        animatorController.SetAnimationBool(BooleanEvents.IsRunning, velocity.magnitude >= minDistanceToRun);
    }

    //let the player rotate based on where he move
    void LookAtMoveDir(Vector3 move)
    {
        // return if there is nothing
        if (move == Vector3.zero) return;

        //get the value's
        Quaternion dir = Quaternion.LookRotation(move);
        float angle = Quaternion.Angle(dir, transform.rotation);
        
        transform.rotation = Quaternion.RotateTowards(transform.rotation, dir, 360 * rotateSpeed * Time.deltaTime);
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //check or it is a player
        Player_Minigame4 playerMinigame4 = hit.gameObject.GetComponent<Player_Minigame4>();
        if (playerMinigame4 == null) return;

        Minigame4Mangeren.instance.plrHittedPlr.Add((plrId, playerMinigame4.plrId));
    }

    //get the speed based on ur movementType
    float GetSpeed()
    {
        if (movementType == MovementType.walking) return defaultSpeed;
        else if (movementType == MovementType.potatoRunning) return potatoHoldSpeed;
        else return runSpeed;
    }
}
