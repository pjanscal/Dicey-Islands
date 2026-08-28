using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameMangeren
{
    static public bool inGame = false; //tell if it is in game or not
    static public bool isPaused = false; //tell if it pause
    static public int plrInGame = 0; //help the lokaal script to see what it should be when someone disconnect

    //load info
    static public CharacterData[] charsData = Resources.LoadAll<CharacterData>("CharactersData");
    static public Dictionary<int, PlrData> plrsData = new();

    //plrInfo's //soon for characterSelect
    public class PlrData
    {
        public bool occupied = false; //help testing the things else it might bug
        public CharacterData charData;
    }

    //when the game start it go once
    [RuntimeInitializeOnLoadMethod]
    static void Init()
    {
        //setting up vars
        for (int plrId = 1; plrId <= LokaalConnecter.maxPlr; plrId++)
        {
            plrsData.Add(plrId, new());
        }
    }

    static public void SwitchScene(string sceneName)
    {
        //here can it switch to loading screen
        SceneManager.LoadScene(sceneName);
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
