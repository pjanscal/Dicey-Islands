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

    [SerializeField] private int plrId;
    
    private LokaalConnecter.PlayerController playerController;
    private GameMangeren.PlrData plrData;
    private GameObject character; //help finding the char in a instant
    private Animator animator; //char animator

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerController = LokaalConnecter.plrsController[plrId];
        plrData = GameMangeren.GetPlrDataFromId(plrId);

        //help testing if there is no charData
        #if UNITY_EDITOR
            StartCoroutine(WaitForPlrToLoad());
        #else
            SetUpCharacter();
        #endif

        GameMangeren.AddCharLoader(plrId, this);
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
        character = Instantiate(charData.character, transform);

        //set position good
        character.transform.localPosition = Vector3.zero;

        animator = character.GetComponent<Animator>();
    }

    public void UseAnimation(CharactersAnimationEvent animationEvent)
    {
        Debug.LogWarning($"char use {animationEvent} but don't have animmation script yet");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
