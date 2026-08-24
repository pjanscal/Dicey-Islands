using UnityEngine;

public class StartSchrem: UiBasic
{

    //Quit the game
    public void Quit()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
