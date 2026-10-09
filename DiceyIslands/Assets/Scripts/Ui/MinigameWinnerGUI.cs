using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//shortcuts
using AnimationTriggers = CharacterLoader.CharactersAnimationTriggerEvent;

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
    [SerializeField] private Light guiLighting; //light of the gui

    private Canvas canvas;
    private Light minigameLighting; //direction light of the minigame
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

        //debug
        #if UNITY_EDITOR
            GetSceneDirectionLighting();
        #endif
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

    //on sceneLoad Init
    public void OnSceneLoad()
    {
        GetSceneDirectionLighting();
    }

    #region Toggle Logic
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
        winnerCharacterLoader.ReLoad();
        winnerCharacterLoader.UseAnimation(AnimationTriggers.ActiveJump);

        foreach (WinnerSlot winnerSlot in winnerSlots)
        {
            winnerSlot.frame.SetActive(true);

            int plrId = places[winnerSlot.place - 1];
            winnerSlot.characterLoader.plrId = plrId;
            winnerSlot.characterLoader.ReLoad();

            winnerSlot.background.color = playerColors[plrId];
            print($"place {winnerSlot.place} select new plr{plrId}");
        }

        StartCoroutine(enumerator());

        IEnumerator enumerator()
        {
            yield return null;
            guiLighting.enabled = true;
            if (minigameLighting != null) minigameLighting.enabled = false;
            canvas.enabled = true; //make sure that it loaded

            yield return new WaitForSeconds(lookTime);

            //go to BoardGame
            if (MatchData.Instance == null) {Debug.LogError("no matchData to return to..."); yield break;}
            MatchData.Instance.returningFromMinigame = true;
            GameMangeren.SwitchScene(GameMangeren.boardGameSceneName);

            TurnOff();
        }
    }

    void TurnOff()
    {
        GameMangeren.isShowingResult = false;
        canvas.enabled = false;
        guiLighting.enabled = false;

        //reduce all lag what is not needed
        foreach (WinnerSlot winnerSlot in winnerSlots)
        {
            winnerSlot.frame.SetActive(false);
        }

        winnerFrame.SetActive(false);
    }
    #endregion

    #region Helper Function
    void GetSceneDirectionLighting()
    {
        foreach (Light light in FindObjectsByType<Light>(FindObjectsSortMode.None))
        {
            if (light == guiLighting || light.type != LightType.Directional) continue;
            minigameLighting = light;
            return;
        }

        minigameLighting = null; //fail safe
        Debug.LogWarning("no dir lighting");
    }
    #endregion
}
