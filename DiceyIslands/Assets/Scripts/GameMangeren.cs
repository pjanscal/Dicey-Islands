using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameMangeren
{
    //plrInfo's
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
