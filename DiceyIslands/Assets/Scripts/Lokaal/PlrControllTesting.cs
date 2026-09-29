using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

//shortcuts
using BooleanEvents = CharacterLoader.CharactersAnimationBooleanEvent; //help ez get acces
using TriggerEvent = CharacterLoader.CharactersAnimationTriggerEvent; //help ez get acces

public class PlrControllTesting : MonoBehaviour
{
    [SerializeField] private int plrId; //the id of the player

    private LokaalConnecter.PlayerController playerController; //the controller that have all data
    private CharacterLoader animationControll; //get the script that controll also animations

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerController = LokaalConnecter.plrsController[plrId]; //connect the player controller to the real controller;
        animationControll = GameMangeren.GetCharacterLoaderFromId(plrId);
    }

    // Update is called once per frame
    void Update()
    {
        //return if this is nothing or in menu
        if (!GameMangeren.inGame || GameMangeren.isPaused || !playerController.occuplied) return;

        Vector2 movedir = playerController.GetMoveDir();
        transform.position += new Vector3(movedir.x, movedir.y, 0) * Time.deltaTime;

        if (playerController.GetButtonDown(LokaalConnecter.InputType.jump)) print($"plr{plrId} has pressed jump");
        if (playerController.GetButtonUp(LokaalConnecter.InputType.jump)) print($"plr{plrId} has Released jump");

        //animation tutorial
        animationControll.SetAnimationBool(BooleanEvents.IsRunning, movedir != Vector2.zero);

        //test trigger
        if (playerController.GetButtonDown(LokaalConnecter.InputType.x))
         animationControll.UseAnimation(TriggerEvent.ActivePress);

        //and u can get info over animator by doing
        Animator animator = animationControll.animator;
    }
}
