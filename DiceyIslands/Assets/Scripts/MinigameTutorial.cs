using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MinigameTutorial : MonoBehaviour
{
    [Serializable]
    private class PlayerSlotInfo
    {
        public int plrId;
        public Image slot;
        public Color color;
    }

    [Header("Ui")]
    //soon for the setting th etutorial up

    private Canvas canvas;

    private bool isActive = false;
    private HashSet<int> plrsReadyUp = new();

    [Header("configs")]
    [SerializeField] private List<PlayerSlotInfo> playerSlotInfos;
    [SerializeField] private Sprite controllerImage;
    [SerializeField] private Sprite controllerReadyUpImage;

    private float timeBeforeCpuReadyUp = .5f;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        GameMangeren.minigameTutorial = this;
        
        //debugs
        if (playerSlotInfos.Count != LokaalConnecter.maxPlr) Debug.LogError("4 plr need to be in minigameTutorialInfo");
        
        //getting obj/ setting value's
        canvas = GetComponent<Canvas>();

        //set up the color
        foreach (PlayerSlotInfo playerSlotInfo in playerSlotInfos)
        {
            playerSlotInfo.slot.color = playerSlotInfo.color;
        }

        Reset();

        //debug
        #if UNITY_EDITOR
            //start if u are in minigame
            if (GameMangeren.IsMiniGame(SceneManager.GetActiveScene().name)) Init(true);
        #endif
    }

    //init when it need to show or not to show
    public void Init(bool state)
    {
        if (!state)
        {
            Reset();
            return;
        }

        Time.timeScale = 0f; //stop it so somethings can't happend
        canvas.enabled = true;
        isActive = true;
        StartCoroutine(CPUTryToReadyUp());
    }

    // Update is called once per frame
    void Update()
    {
        if (!isActive || GameMangeren.isLoading || GameMangeren.isPaused || !GameMangeren.inGame ||
         LokaalConnecter.connectionType != LokaalConnecter.ConnectionTypes.nothing) return;
        
        //check every single one
        foreach (PlayerSlotInfo playerSlotInfo in playerSlotInfos)
        {
            TryToReadyUp(playerSlotInfo);
        }
    }

    //check or the plr want to ready-up
    void TryToReadyUp(PlayerSlotInfo playerSlotInfo)
    {
        int plrId = playerSlotInfo.plrId;
        LokaalConnecter.PlayerController playerController = LokaalConnecter.plrsController[plrId];
        
        if (!playerController.occuplied || playerController.isCPU || plrsReadyUp.Contains(plrId)) return;
        if (!playerController.GetButtonDown(LokaalConnecter.InputType.x)) return;

        plrsReadyUp.Add(plrId);
        playerSlotInfo.slot.sprite = controllerReadyUpImage;
        CanStartMinigame();
    }
    
    //check or cpu want to ready (it always want)
    IEnumerator CPUTryToReadyUp()
    {
        yield return new WaitUntil (() => GameMangeren.inGame);
        yield return new WaitForSecondsRealtime(timeBeforeCpuReadyUp);

        foreach (PlayerSlotInfo playerSlotInfo in playerSlotInfos)
        {
            int plrId = playerSlotInfo.plrId;
            LokaalConnecter.PlayerController playerController = LokaalConnecter.plrsController[plrId];
            if (!playerController.isCPU) continue;

            plrsReadyUp.Add(plrId);
            playerSlotInfo.slot.sprite = controllerReadyUpImage;
            CanStartMinigame();
        }
    }

    //check or everyone is readyup
    void CanStartMinigame()
    {
        //if (plrsReadyUp.Count != LokaalConnecter) help improve but if there is no cpu then ye it might be hard
        for (int plrId = 1; plrId <= LokaalConnecter.maxPlr; plrId++)
        {
            LokaalConnecter.PlayerController playerController = LokaalConnecter.plrsController[plrId];
            if (!playerController.occuplied) continue;

            if (!plrsReadyUp.Contains(plrId)) return;
        }

        Debug.LogWarning("Everyone is ready for minigame");
        StartMiniGame();
    }

    //start the minigame
    void StartMiniGame()
    {
        if (!isActive) return; //debug
        isActive = false;
        Reset();

        //send message to the minigame
        GameMangeren.startMiniGame?.Invoke();
        Time.timeScale = 1;
    }

    //reset when done
    void Reset()
    {
        foreach (PlayerSlotInfo playerSlotInfo in playerSlotInfos)
        {
            playerSlotInfo.slot.sprite = controllerImage;
            plrsReadyUp.Clear();
            canvas.enabled = false;
        }
    }
}
