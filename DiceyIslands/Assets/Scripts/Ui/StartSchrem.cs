using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StartSchrem: UiBasic
{
    [SerializeField] protected GameObject startButton;

    protected override void Start()
    {
        base.Start();

        LokaalConnecter.outOfMatchMaking += OnMatchMakingFinished;
    }

    protected void OnMatchMakingFinished(bool state)
    {
        //deleted old vers it is for now so it can switch
        if (state) SwitchScene(GameMangeren.boardGameSceneName); //need this so if it is not startscene then u won't change scene :3
        else EventSystem.current.SetSelectedGameObject(startButton);
    }

    //MatchButton
    public void MakeMatch()
    {
        LokaalConnecter.SwitchMatchMaking(true);
    }

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
