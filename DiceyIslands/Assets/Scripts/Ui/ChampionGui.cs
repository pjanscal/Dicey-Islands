using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ChampionGui : MinigameWinnerGUI
{
    [Header("UI")]
    [SerializeField] protected GameObject startNavigation;
    [SerializeField] protected RectTransform chartFrame;

    protected override IEnumerator DelayToTurnOffAndLoad()
    {
        yield return null;
        guiLighting.enabled = true;
        if (minigameLighting != null) minigameLighting.enabled = false;
        canvas.enabled = true; //make sure that it loaded
        
        EventSystem.current.SetSelectedGameObject(startNavigation);
    }

    protected override void Testing()
    {
        if (!Input.GetKeyDown(KeyCode.Backspace)) return;

        List<int> places= new() {2, 1, 3, 4};
        Toggle(true, places);
    }

    #region ButtonsFunction
    public void Retry()
    {
        
    }

    public void Exit()
    {
        GameMangeren.Exit();
    }
    #endregion
}
