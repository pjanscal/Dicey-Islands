using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public static class GameMangeren
{
    //all difficulty a cpu can have
    public enum CPUDifficulty
    {
        Easy,
        normal,
        hard,
    }

    static public bool inGame = false; //tell if it is in game or not
    static public bool isPaused = false; //tell if it pause
    static public bool isLoading = false; //tell if it is loading
    static public int plrInGame = 0; //help the lokaal script to see what it should be when someone disconnect
    static public CPUDifficulty cPUDifficulty = CPUDifficulty.hard; //the difficulty of the cpu
    static public Dictionary<int, CharacterLoader> charLoaderScript = new(); //help with animations

    //load info
    static public CharacterData[] charsData = Resources.LoadAll<CharacterData>("CharactersData");
    static private GameMangerSettings gameMangerSettings = Resources.Load<GameMangerSettings>("GameMangerSettings");
    static public LoadingScreen loadingScreen;

    static public Dictionary<int, PlrData> plrsData = new();
    static public HashSet<int> allCPU = new(); //a list of plrid about who is cpu

    //plrInfo's //soon for characterSelect
    public class PlrData
    {
        public bool occupied = false; //help testing the things else it might bug
        public CharacterData charData;
    }

    //configs
    static private string startSceneName = "StartScene";

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
        GameObject.Instantiate(gameMangerSettings.loadingScreen);
        SetInEventSystem();

        //setup Connection
        SceneManager.sceneLoaded += OnSceneChanged;
    }

    static public void SwitchScene(string sceneName)
    {
        //soon if there come a supporter for the gamemanger i change it 
        LokaalMatchingUi.instance.StartCoroutine(loadingScreen.LoadScene(sceneName));
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
    static public void AddCharLoader(int plrId, CharacterLoader loader)
    {
        charLoaderScript.Remove(plrId); //delete the old one
        charLoaderScript.Add(plrId, loader);
    }
    
    static public CharacterLoader GetCharacterLoaderFromId(int plrId)
    {
        return charLoaderScript[plrId];
    }

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
