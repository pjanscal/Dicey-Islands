using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterLoader : MonoBehaviour
{
    public enum CharactersAnimationTriggerEvent
    {
        ActivePress,
        ActiveJump
    }

    public enum CharactersAnimationBooleanEvent
    {
        IsRunning
    }

    public int plrId;
    
    private LokaalConnecter.PlayerController playerController;
    private GameMangeren.PlrData plrData;
    private GameObject character; //help finding the char in a instant
    [HideInInspector] public Animator animator; //char animator
    private VisualeEffectAnimatorSupport vfxController;
    private int layer; //the layer it gonna to be

    //configs
    [Header("Configs")]
    [SerializeField] private bool canConnectToMangener = true; //for if u wanna use it for like winner gui
    [Tooltip("let it make the character when the game start")] [SerializeField] private bool autoCharacter = true;


    void Awake()
    {
        layer = gameObject.layer;
        if (canConnectToMangener) GameMangeren.AddCharLoader(plrId, this);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!autoCharacter) return; //make it not make it when u don't want it
        playerController = LokaalConnecter.plrsController[plrId];
        plrData = GameMangeren.GetPlrDataFromId(plrId);

        //help testing if there is no charData
        #if UNITY_EDITOR
            StartCoroutine(WaitForPlrToLoad());
        #else
            SetUpCharacter();
        #endif
    }

    //wait until the plr is here
    IEnumerator WaitForPlrToLoad()
    {
        yield return new WaitUntil(() => playerController.occuplied && GameMangeren.inGame);

        SetUpCharacter();
    }

    //set the char in game
    void SetUpCharacter()
    {
        CharacterData charData = plrData.charData; //get the charInfo
        PlaceCharacterDown(charData.character);
    }

    public void ReLoad()
    {
        plrData = GameMangeren.GetPlrDataFromId(plrId);
        Destroy(character);
        SetUpCharacter();
        
        /*set the lighting good
        if (!isLightLevelOne) return;

        foreach (SkinnedMeshRenderer skinnedMeshRenderer in character.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            skinnedMeshRenderer.renderingLayerMask = 1u << 1;
        }*/
    }

    public void ReLoadCharacterId(int characterId)
    {
        CharacterData characterData= GameMangeren.GetCharacterDataFromId(characterId);
        Destroy(character);
        PlaceCharacterDown(characterData.character);
    }

    #region Animation
    public void UseAnimation(CharactersAnimationTriggerEvent animationEvent)
    {
        if (!animator) {Debug.LogError($"no animator is in {character} or in plr{plrId}"); return;}
        Debug.LogWarning($"char use {animationEvent}");
        
        vfxController.StopAllVisualeEffect();
        animator.SetTrigger(animationEvent.ToString());
    }

    public void SetAnimationBool(CharactersAnimationBooleanEvent booleanEvent, bool state)
    {
        if (!animator) {Debug.LogError($"no animator is in {character} or in plr{plrId}"); return;}
        //Debug.LogWarning($"char switch {booleanEvent} to {state}");
        
        animator.SetBool(booleanEvent.ToString(), state);
    }
    #endregion

    //so 2 function can use it*
    void PlaceCharacterDown(GameObject newCharacter)
    {
        character = Instantiate(newCharacter, transform);
        character.transform.localPosition = Vector3.zero; //set pos good
        SetLayer(character); //reduce the lag if this is ui

        animator = character.GetComponent<Animator>();
        vfxController = character.GetComponent<VisualeEffectAnimatorSupport>();

        void SetLayer(GameObject gameObject)
        {
            gameObject.layer = layer;
            
            foreach (Transform child in gameObject.transform)
            {
                SetLayer(child.gameObject);
            }
        }
    }
}
