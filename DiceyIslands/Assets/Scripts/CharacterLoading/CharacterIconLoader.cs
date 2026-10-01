using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CharacterIconLoader : MonoBehaviour
{
    [SerializeField] private int plrId;

    private GameMangeren.PlrData plrData;
    private LokaalConnecter.PlayerController playerController;
    private Image icon;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        icon = GetComponent<Image>();
        playerController = LokaalConnecter.plrsController[plrId];
        plrData = GameMangeren.GetPlrDataFromId(plrId);

        //help testing if there is no charData
        #if UNITY_EDITOR
            StartCoroutine(WaitForPlrToLoad());
        #else
            SetUpIcon();
        #endif
    }

    //wait until the plr is here
    IEnumerator WaitForPlrToLoad()
    {
        yield return new WaitUntil(() => playerController.occuplied && GameMangeren.inGame);

        SetUpIcon();
    }

    //set the char in game
    void SetUpIcon()
    {
        CharacterData charData = plrData.charData; //get the charInfo
        icon.sprite = charData.characterIcon;
    }
}
