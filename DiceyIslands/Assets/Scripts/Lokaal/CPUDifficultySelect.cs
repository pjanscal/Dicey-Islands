using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class CPUDifficultySelect : MonoBehaviour
{
    //for now everyone can controll it

    [Serializable]
    private class DifficultyInfo
    {
        public string name;
        public Color color;
    }

    [Serializable]
    private class DifficultyInfoKeysValues
    {
        public GameMangeren.CPUDifficulty key;
        public DifficultyInfo value;
    }

    [Header("UI")]
    [SerializeField] private GameObject[] arrows; //to change the color of who is controllering?
    [SerializeField] private Transform difficultyFrame;
    [SerializeField] private TextMeshProUGUI primeDifficultyText; //the prime where the difficulty go in

    private TextMeshProUGUI secondaryDifficultyText;
    private bool primeDifficultyIsSelected = true; //help knowing wich one is on the screen
    private bool canTweenAgain = true; //stop it from spamming
    private int currentDifficultyId;
    private bool isActive = false; //help that u not insta start :3
    private Dictionary<int, DifficultyInfo> difficultyInfos = new();

    //configs thingy
    [Header("Configs")]
    [SerializeField] private List<DifficultyInfoKeysValues> difficultyInfosEdit = new(3);

    private float difficultySwitchDur = .5f; //time before it switches
    private float timeBeforeActive = .5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //set up vars
        LokaalConnecter.cPUDifficultySelect = this;

        foreach (DifficultyInfoKeysValues difficultyInfoKeysValues in difficultyInfosEdit)
        {
            difficultyInfos.Add((int)difficultyInfoKeysValues.key, difficultyInfoKeysValues.value);
        }

        //make the secondary DifficultyText so that can be tween
        GameObject newPreview = Instantiate(primeDifficultyText.gameObject, difficultyFrame);
        RectTransform rectTransform = newPreview.GetComponent<RectTransform>();
        rectTransform.localPosition = Vector2.right * primeDifficultyText.rectTransform.rect.width;
        secondaryDifficultyText = newPreview.GetComponent<TextMeshProUGUI>();

        //set up the colors and text
        int difficultyId = (int)GameMangeren.cPUDifficulty;
        DifficultyInfo difficultyInfo = difficultyInfos[difficultyId];
        primeDifficultyText.text = difficultyInfo.name;
        primeDifficultyText.color = difficultyInfo.color;

        currentDifficultyId = difficultyId;
    }

    //init when it is it turn
    public void Init()
    {
        StartCoroutine(InitCourtine());
    }

    //wait some sec so u can't immedaly start
    IEnumerator InitCourtine()
    {
        yield return new WaitForSecondsRealtime(timeBeforeActive);
        isActive = true;
    }

    // Update is called once per frame
    void Update()
    {
        //check if it can go
        if (!isActive || LokaalConnecter.connectionType != LokaalConnecter.ConnectionTypes.CPUDifficultySelect) return;

        TrySwitchingDifficulty();
        TryToStart();
        TryToQuit();
    }

    //move
    void TrySwitchingDifficulty()
    {
        //beta just simple like everyone can select
        
        for (int plrId = 1; plrId <= LokaalConnecter.maxPlr; plrId++) //get all plrs
        {
            LokaalConnecter.PlayerController playerController = LokaalConnecter.plrsController[plrId];

            var (succes, dir) = GetLeftRight(playerController.GetMoveDir());
            if (!succes) continue;

            if (!canTweenAgain) return;
            canTweenAgain = false; //here for the debounce

            ChangeDifficulty(currentDifficultyId + dir);
            SwitchDifficultyUi(new Vector2(dir, 0));
        }
    }

    //requment to start a game
    void TryToStart()
    {
        //beta just simple like everyone can select
        
        for (int plrId = 1; plrId <= LokaalConnecter.maxPlr; plrId++) //get all plrs
        {
            LokaalConnecter.PlayerController playerController = LokaalConnecter.plrsController[plrId];

            if (!playerController.GetButtonDown(LokaalConnecter.InputType.x)) continue;

            StartGame();
            return; //debounce
        }
    }

    //requment to quit to charSelect
    void TryToQuit()
    {
        //beta just simple like everyone can select
        
        for (int plrId = 1; plrId <= LokaalConnecter.maxPlr; plrId++) //get all plrs
        {
            LokaalConnecter.PlayerController playerController = LokaalConnecter.plrsController[plrId];

            if (!playerController.GetButtonDown(LokaalConnecter.InputType.secondAction)) continue;

            QuitToCharSelect();
            return; //debounce
        }
    }

    //change the new text difficulty
    void ChangeDifficulty(int newDifficultyId)
    {
        //set the id in valid reach
        int maxDifficulty = Enum.GetValues(typeof(GameMangeren.CPUDifficulty)).Length;

        if (newDifficultyId > maxDifficulty - 1) newDifficultyId = 0; //1 is the beginning of a array
        if (newDifficultyId < 0) newDifficultyId = maxDifficulty - 1;

        DifficultyInfo difficultyInfo = difficultyInfos[newDifficultyId];
        currentDifficultyId = newDifficultyId;

        //set up the new text
        TextMeshProUGUI newText = !primeDifficultyIsSelected? primeDifficultyText : secondaryDifficultyText;
        newText.text = difficultyInfo.name;
        newText.color = difficultyInfo.color;
    }

    (bool succes, int dir) GetLeftRight(Vector2 moveDir)
    {
        //check of it not up or down
        if (math.abs(moveDir.y) > math.abs(moveDir.x)) return (false, 0);

        int dir = (int)math.sign(moveDir.x);
        if (dir == 0) return (false, dir);

        return (true, dir);
    }

    //go back
    public void QuitToCharSelect()
    {
        if (!isActive || LokaalConnecter.connectionType != LokaalConnecter.ConnectionTypes.CPUDifficultySelect) return; //debounce
        isActive = false;

        LokaalConnecter.CPUDifficultySelectQuit();
    }

    // set in the diffuculty and start the game :3
    public void StartGame()
    {
        if (!isActive || LokaalConnecter.connectionType != LokaalConnecter.ConnectionTypes.CPUDifficultySelect) return; //debounce
        isActive = false;

        GameMangeren.cPUDifficulty = (GameMangeren.CPUDifficulty)currentDifficultyId; //set the difficulty in
        LokaalConnecter.FinishCpuDifficultySelect();
    }

    //help switching the text
    void SwitchDifficultyUi(Vector2 dir)
    {
        RectTransform selected = primeDifficultyIsSelected? primeDifficultyText.rectTransform : secondaryDifficultyText.rectTransform;
        RectTransform newPreview = !primeDifficultyIsSelected? primeDifficultyText.rectTransform : secondaryDifficultyText.rectTransform;
        Vector2 targetPos = new Vector2(primeDifficultyText.rectTransform.rect.width, primeDifficultyText.rectTransform.rect.height) * dir;
        newPreview.localPosition = targetPos * -1; //get to the - side to start

        DOTween.Sequence() //so it can start all at the exact same time
        .Append(selected.DOLocalMove(targetPos, difficultySwitchDur))
        .Join(newPreview.DOLocalMove(Vector2.zero, difficultySwitchDur))
        .SetEase(Ease.OutBounce).SetUpdate(true) //settings
        .OnComplete(() =>
        {
            canTweenAgain = true;
        });

        primeDifficultyIsSelected = !primeDifficultyIsSelected;
    }
}
