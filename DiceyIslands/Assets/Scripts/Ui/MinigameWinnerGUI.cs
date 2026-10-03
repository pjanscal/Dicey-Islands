using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinigameWinnerGUI : MonoBehaviour
{
    [Serializable]
    private class WinnerSlot
    {
        public int place;
        public CharacterLoader characterLoader;
        public Image background;
        [Tooltip("the ui that hold everything")] public GameObject frame; //reduce lag
    }

    [Serializable]
    private class PlayerColorInfo
    {
        public int plrId;
        public Color color;
    }

    [Header("UI")]
    [SerializeField] private WinnerSlot[] winnerSlots;
    [SerializeField] private CharacterLoader winnerCharacterLoader;
    [SerializeField] private GameObject winnerFrame; //reduce lag to disable it

    private Canvas canvas;
    private Dictionary<int, Color> playerColors = new();

    //configs
    [SerializeField] private List<PlayerColorInfo> playerColorInfos; 
    const float lookTime = 10f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        canvas = GetComponent<Canvas>();
        GameMangeren.minigameWinnerGui = this;

        foreach (PlayerColorInfo playerColorInfo in playerColorInfos)
        {
            playerColors.Add(playerColorInfo.plrId, playerColorInfo.color);
        }
    }

    // Update is called once per frame
    #if UNITY_EDITOR
    void Update()
    {
        Testing();
    }

    void Testing()
    {
        if (!Input.GetKeyDown(KeyCode.Equals)) return;

        List<int> places= new() {2, 1, 3, 4};
        Toggle(true, places);
    }
    #endif


    public void Toggle(bool state, List<int> places = null)
    {
        if (state) TurnOn(places);
        else TurnOff();
    }

    void TurnOn(List<int> places)
    {
        GameMangeren.isShowingResult = true;

        winnerFrame.SetActive(true);
        winnerCharacterLoader.plrId = places[0]; //0 == first
        winnerCharacterLoader.ReLoad(true);

        foreach (WinnerSlot winnerSlot in winnerSlots)
        {
            winnerSlot.frame.SetActive(true);

            int plrId = places[winnerSlot.place - 1];
            winnerSlot.characterLoader.plrId = plrId;
            winnerSlot.characterLoader.ReLoad(true);

            winnerSlot.background.color = playerColors[plrId];
            print($"place {winnerSlot.place} select new plr{plrId}");
        }

        StartCoroutine(enumerator());

        IEnumerator enumerator()
        {
            yield return null;
            canvas.enabled = true; //make sure that it loaded

            yield return new WaitForSeconds(lookTime);

            //go to BoardGame
            if (MatchData.Instance == null) {Debug.LogError("no matchData to return to...");}
            MatchData.Instance.returningFromMinigame = true;
            GameMangeren.SwitchScene(GameMangeren.boardGameSceneName);

            TurnOff();
        }
    }

    void TurnOff()
    {
        GameMangeren.isShowingResult = false;
        canvas.enabled = false;

        //reduce all lag what is not needed
        foreach (WinnerSlot winnerSlot in winnerSlots)
        {
            winnerSlot.frame.SetActive(false);
        }

        winnerFrame.SetActive(false);
    }
}
