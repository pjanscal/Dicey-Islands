using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public static class GameMangeren
{
    static public bool inGame = false; //tell if it is in game or not
    static public bool isPaused = false; //tell if it pause
    static public int plrInGame = 0; //help the lokaal script to see what it should be when someone disconnect

    //load info
    static public CharacterData[] charsData = Resources.LoadAll<CharacterData>("CharactersData");
    static private GameMangerSettings gameMangerSettings = Resources.Load<GameMangerSettings>("GameMangerSettings");

    static public Dictionary<int, PlrData> plrsData = new();

    //plrInfo's //soon for characterSelect
    public class PlrData
    {
        public bool occupied = false; //help testing the things else it might bug
        public CharacterData charData;
    }

    //configs
    static private string startSceneName = "SampleScene";

    //when the game start it go once
    [RuntimeInitializeOnLoadMethod]
    static void Init()
    {
        //setting up vars
        for (int plrId = 1; plrId <= LokaalConnecter.maxPlr; plrId++)
        {
            plrsData.Add(plrId, new());
        }

        //set in objects
        GameObject.Instantiate(gameMangerSettings.pauseSchrem);
        SetInEventSystem();

        //setup Connection
        SceneManager.sceneLoaded += OnSceneChanged;
    }

    static public void SwitchScene(string sceneName)
    {
        //here can it switch to loading screen
        SceneManager.LoadScene(sceneName);
    }

    static public void Exit()
    {
        SwitchScene(startSceneName);
        ResetAll();
    }

    static void ResetAll()
    {
        //reset value's
        inGame = false;
        isPaused = false; //pause
        Time.timeScale = 1; //pause or minigame or dissconnecter
        
        LokaalConnecter.ResetLokaal();
    }

    //init when it change from scene
    static void OnSceneChanged(Scene scene, LoadSceneMode sceneMode)
    {
        SetInEventSystem();
    }

    static void SetInEventSystem()
    {
        //check if there not alr one
        EventSystem currentEventSystem = GameObject.FindFirstObjectByType<EventSystem>();
        if (currentEventSystem != null) return;

        GameObject.Instantiate(gameMangerSettings.eventSystemUi);
    }

    //help funtion

    static public PlrData GetPlrDataFromId(int plrId)
    {
        return plrsData[plrId];
    }

    static public CharacterData GetCharacterDataFromId(int charId)
    {
        return charsData[charId];
    }

    static public int GetIdFromCharData(CharacterData charData)
    {
        int charId = System.Array.IndexOf(charsData, charData);
        return charId;
    }
}
