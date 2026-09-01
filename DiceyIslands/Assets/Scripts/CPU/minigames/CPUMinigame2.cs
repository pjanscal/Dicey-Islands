using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CPUMinigame2: CPUMangeren
{
    //if hardcoded robert fault :3

    [Serializable]
    private class DifficultyCPUConfigs
    {
        public Vector2 offSet;
        //wa happend if it is that point
        public GameMangeren.CPUDifficulty difficulty;
    }

    //objects needed
    [SerializeField] private SpawnObject spawnObject;
    
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
        LokaalConnecter.PlayerController playerController = LokaalConnecter.plrsController[plrId];
        DifficultyCPUConfigs configs = difficultiesConfigs[GameMangeren.cPUDifficulty];

        //ready up
        yield return new WaitForSeconds(secBeforeReadyUp);

        playerController.CPUSetButton(LokaalConnecter.InputType.x, true);

        //soon unitl it is finish :3
        while (true)
        {
            //caculate so we have less time between the real offset
            float offset = UnityEngine.Random.Range(configs.offSet.x, configs.offSet.y);

            yield return new WaitUntil(() => spawnObject.isActiveAndEnabled);

            //now it spawned in and it wait for the offset while caculating if it want it
            yield return new WaitForSeconds(offset);

            playerController.CPUSetButton(LokaalConnecter.InputType.x, true);
        }

        //yield return null;
    }
}
