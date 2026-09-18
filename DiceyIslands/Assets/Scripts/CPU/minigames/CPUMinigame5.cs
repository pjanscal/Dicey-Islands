using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class CPUMinigame5: CPUMangeren
{
    [Serializable]
    private class DifficultyCPUConfigs
    {
        public GameMangeren.CPUDifficulty difficulty;
        public Vector2 finalScore;
    }

    [SerializeField] private RawImage mainColor; //get the color of the target
    [SerializeField] private List<MaingameScript5> allMaingameScript5;
    
    private bool isActive = false;

    //configs
    [Header("Configs")]
    [SerializeField] private List<DifficultyCPUConfigs> difficultiesConfigsEdit = new(3); // soon update it better just beta testing
    private Dictionary<GameMangeren.CPUDifficulty, DifficultyCPUConfigs> difficultiesConfigs = new();
    private Vector2Int fastDistanceOffset = new(3, 10);
    private Vector2 startTime = new(.5f, 3);
    const float switchWaitTime = .2f;

    protected override void Start()
    {
        base.Start();

        GameMangeren.startMiniGame += Init;

        //beta
        foreach (DifficultyCPUConfigs difficultyCPUConfigs in difficultiesConfigsEdit)
        {
            difficultiesConfigs.Add(difficultyCPUConfigs.difficulty, difficultyCPUConfigs);
        }
    }

    public void Init()
    {
        isActive = true;
    }

    protected override IEnumerator CPUStart(int plrId)
    {
        DifficultyCPUConfigs config = difficultiesConfigs[GameMangeren.cPUDifficulty];
        LokaalConnecter.PlayerController playerController = LokaalConnecter.plrsController[plrId];

        float rngStartWaitDur = UnityEngine.Random.Range(startTime.x, startTime.y) + plrId / 10; //make a little delay
        yield return new WaitUntil(() => isActive);
        yield return new WaitForSeconds(rngStartWaitDur);

        Vector3Int rightTargetColor = new Vector3Int(Mathf.RoundToInt(mainColor.color.r * 255f), Mathf.RoundToInt(mainColor.color.g * 255f), Mathf.RoundToInt(mainColor.color.b * 255f));
        Vector3Int targetColor = GetTargetColorWithOffset(rightTargetColor);
        Slider[] sliders = GetSliderFromPlrId(plrId);
        //target offset color btw

        //go do it one by one
        for (int axis = 0; axis <= 2; axis++)
        {
            playerController.cpuMoveDir = Vector2.right;
            Slider slider = sliders[axis];
            //bool rightSide = UnityEngine.Random.Range(0, 2) == 0? false : true;
            //int offSet = UnityEngine.Random.Range(fastDistanceOffset.x, fastDistanceOffset.y + 1) * (rightSide? 1 : -1);
            int target = math.clamp(targetColor[axis], 0, 255);

            yield return new WaitUntil(() => HavePassedSlider());
            playerController.cpuMoveDir = Vector2.zero;
            yield return new WaitForSeconds(switchWaitTime);

            playerController.cpuMoveDir = Vector2.down;
            yield return null;
            playerController.cpuMoveDir = Vector2.zero;
            //wait a little
            yield return new WaitForSeconds(switchWaitTime);

            bool HavePassedSlider()
            {
                bool succes = slider.value >= target;
                return succes;
            }
        }

        //ready up
        playerController.CPUSetButton(LokaalConnecter.InputType.x, true);

        Vector3Int GetTargetColorWithOffset(Vector3Int target)
        {
            Vector3Int result = new();
            for (int axis = 0; axis <= 2; axis++)
            {
                int rightSideValue = UnityEngine.Random.Range(0, 2) * 2 - 1; //-1 or 1
                float rngFinalResult = UnityEngine.Random.Range(config.finalScore.x, config.finalScore.y) / 100f;
                float maxValue = GetMainColorMaxDistance(axis);
                int offset = Mathf.RoundToInt(maxValue * (1 - rngFinalResult));
                //print(offset);
                
                int targetResult = target[axis] + offset * rightSideValue;
                result[axis] = math.clamp(targetResult, 0, 255);
            }

            //print((result, plrId));
            return result;
        }
    }

    //helper function
    Slider[] GetSliderFromPlrId(int plrId)
    {
        MaingameScript5 maingameScript5 = allMaingameScript5[plrId - 1];
        return maingameScript5.sliders;
    }

    //get target distance
    int GetMainColorMaxDistance(int colorId)
    {
        int colorValue = Mathf.RoundToInt(mainColor.color[colorId] * 255); //make from .7 to a number like 0-255
        int distance = colorValue < (255 / 2)? 255 - colorValue : colorValue; //255 is like the color things
        return distance;
    }
}
