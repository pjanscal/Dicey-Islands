using System.Collections;
using UnityEngine;

public class CharacterLoader : MonoBehaviour
{
    public enum CharactersAnimationEvent
    {
        Idle,
        running,
        jumping
    }

    public int plrId;
    
    private LokaalConnecter.PlayerController playerController;
    private GameMangeren.PlrData plrData;
    private GameObject character; //help finding the char in a instant
    private Animator animator; //char animator

    //configs
    [Header("Configs")]
    [SerializeField] private bool canConnectToMangener = true; //for if u wanna use it for like winner gui
    [Tooltip("let it make the character when the game start")] [SerializeField] private bool autoCharacter = true;

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

        if (canConnectToMangener) GameMangeren.AddCharLoader(plrId, this);
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

    public void UseAnimation(CharactersAnimationEvent animationEvent)
    {
        Debug.LogWarning($"char use {animationEvent} but don't have animmation script yet");
    }

    public void ReLoad()
    {
        plrData = GameMangeren.GetPlrDataFromId(plrId);
        Destroy(character);
        SetUpCharacter();
    }

    public void ReLoadCharacterId(int characterId)
    {
        CharacterData characterData= GameMangeren.GetCharacterDataFromId(characterId);
        Destroy(character);
        PlaceCharacterDown(characterData.character);
    }

    //so 2 function can use it*
    void PlaceCharacterDown(GameObject newCharacter)
    {
        character = Instantiate(newCharacter, transform);

        //set position good
        character.transform.localPosition = Vector3.zero;

        animator = character.GetComponent<Animator>();
    }
}
