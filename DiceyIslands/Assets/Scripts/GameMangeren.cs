using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameMangeren
{
    static public bool inGame = false; //tell if it is in game or not
    static public bool isPaused = false; //tell if it pause
    static public int plrInGame = 0; //help the lokaal script to see what it should be when someone disconnect

    //plrInfo's //soon for characterSelect
    public class PlrData
    {
        
    }

    //when the game start it go once
    [RuntimeInitializeOnLoadMethod]
    static void Init()
    {
        
    }

    static public void SwitchScene(string sceneName)
    {
        //here can it switch to loading screen
        SceneManager.LoadScene(sceneName);
    }
}
