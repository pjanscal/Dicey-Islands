using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CPUMinigame1 : CPUMangeren
{
    [Serializable]
    private class DifficultyCPUConfigs
    {
        public Vector2 offSet;
        public GameMangeren.CPUDifficulty difficulty;
    }

    [SerializeField] private TimeNeeded timeNeeded;
    
    //configs
    [Header("Configs")]
    [SerializeField] private List<DifficultyCPUConfigs> difficultiesConfigsEdit = new(3); // soon update it better just beta testing
    private Dictionary<GameMangeren.CPUDifficulty, DifficultyCPUConfigs> difficultiesConfigs = new();

    protected override void Start()
    {
        base.Start();

        //beta
        foreach(DifficultyCPUConfigs difficultyCPUConfigs in difficultiesConfigsEdit)
        {
            difficultiesConfigs.Add(difficultyCPUConfigs.difficulty, difficultyCPUConfigs);
        }
    }

    protected override IEnumerator CPUStart(int plrId)
    {
        MainGameScript mainGameScript = plrsChar[plrId].GetComponent<MainGameScript>();
        LokaalConnecter.PlayerController playerController = LokaalConnecter.plrsController[plrId];
        DifficultyCPUConfigs configs = difficultiesConfigs[GameMangeren.cPUDifficulty];

        yield return new WaitForSeconds(secBeforeReadyUp); //so it load the new timeReqeument

        //ready up
        playerController.CPUSetButton(LokaalConnecter.InputType.x, true);

        float timeBeforeItEnds = timeNeeded.timeNeededSeconds;
        float timeOffset = UnityEngine.Random.Range(configs.offSet.x, configs.offSet.y); //get rng number
        //timeOffset *= UnityEngine.Random.Range(0, 2) == 0? -1f : 1f; //make it - or +
        float pressTime = timeBeforeItEnds + timeOffset;

        print($"timeneeded: {timeBeforeItEnds}, timeoffset: {timeOffset}, pressTime: {pressTime}");

        //press it when it reach that time
        yield return null;

        playerController.CPUSetButton(LokaalConnecter.InputType.x, false);

        yield return new WaitUntil(() => mainGameScript.elapsed >= pressTime);

        playerController.CPUSetButton(LokaalConnecter.InputType.x, true);
    }
}
