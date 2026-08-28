using UnityEngine;

public class PauseSchrem: UiBasic
{
    private Canvas canvas;

    protected override void Start()
    {
        base.Start();

        DontDestroyOnLoad(gameObject);
        canvas = GetComponent<Canvas>();
    }

    protected override void Update()
    {
        base.Update();

        //on pause button switch
        //mabye make it controller
        if (Input.GetKeyDown(KeyCode.Alpha1)) TogglePause();
    }

    //go toggle pause
    public void TogglePause()
    {
        if (LokaalConnecter.connectionType != LokaalConnecter.ConnectionTypes.nothing || !GameMangeren.inGame) return; //make sure that it not gonna pause when it should

        GameMangeren.isPaused = !GameMangeren.isPaused; //make sure that some update stop by here
        Time.timeScale = GameMangeren.isPaused? 0 : 1; //stop the time *get the old timescale for minigame for later
        canvas.enabled = GameMangeren.isPaused;

        //set by beginning but there is not more ui so it is then useless
    }

    //reset all value
    public void QuitToMenu()
    {
        canvas.enabled = false;
        GameMangeren.Exit();
    }
}
