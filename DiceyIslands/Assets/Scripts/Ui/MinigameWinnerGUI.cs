using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MinigameWinnerGUI : MonoBehaviour
{
    [Serializable]
    private class WinnerSlot
    {
        public int place;
        public CharacterLoader characterLoader;
        public Image background;
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
    [SerializeField] private Light winnerLight;

    private Canvas canvas;
    private Light dirLight;
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

        GetLighting();

        //setup Connection
        SceneManager.sceneLoaded += OnSceneChanged;
    }

    // Update is called once per frame
    void Update()
    {
        Testing();
    }

    void OnSceneChanged(UnityEngine.SceneManagement.Scene scene, LoadSceneMode sceneMode) //scene didn't work...
    {
        Task.Run(() => GetLighting());
    }

    void Testing()
    {
        if (!Input.GetKeyDown(KeyCode.Equals)) return;

        List<int> places= new() {2, 1, 3, 4};
        Toggle(true, places);
    }

    public void Toggle(bool state, List<int> places = null)
    {
        if (state) TurnOn(places);
        else TurnOff();
    }

    public void GetLighting()
    {
        Light[] allLighting = FindObjectsByType<Light>(FindObjectsSortMode.None);

        foreach (Light light in allLighting)
        {
            if (light == winnerLight || light.type != LightType.Directional) continue;
            dirLight = light;
            print(dirLight);
            return;
        }
    }

    void TurnOn(List<int> places)
    {
        GameMangeren.isShowingResult = true;

        foreach (WinnerSlot winnerSlot in winnerSlots)
        {
            int plrId = places[winnerSlot.place - 1];
            winnerSlot.characterLoader.plrId = plrId;
            print($"place {winnerSlot.place} select new plr{plrId}");
            winnerSlot.characterLoader.ReLoad();

            winnerSlot.background.color = playerColors[plrId];
        }

        winnerCharacterLoader.plrId = places[0]; //0 == first
        winnerCharacterLoader.ReLoad();

        canvas.enabled = true;
        winnerLight.enabled = true;
        if (dirLight != null) dirLight.enabled = false;
        StartCoroutine(enumerator());

        IEnumerator enumerator()
        {
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
        dirLight = null;
        winnerLight.enabled = false;
    }
}
