using UnityEngine;

public class StartSchrem: UiBasic
{
    [SerializeField] protected GameObject charSelect; //so if it succes with making it go there

    protected override void Start()
    {
        base.Start();

        LokaalConnecter.outOfMatchMaking += OnMatchMakingFinished;
    }

    protected void OnMatchMakingFinished(bool state)
    {
        if (state) SwitchGui(charSelect);
        else SwitchGui(beginGui);
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
