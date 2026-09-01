using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.Mathematics;
using UnityEngine;

public class CPUMinigame2: CPUMangeren
{

    [Serializable]
    private class DifficultyCPUConfigs
    {
        //ik a class in a class in a class
        [Serializable]
        public class PointChanceToSelect
        {
            public int pointGain;
            public float chanceToSelect;
        }

        public Vector2 offSet;
        public GameMangeren.CPUDifficulty difficulty;

        [Header("pointChange to select")]
        public List<PointChanceToSelect> pointChanceToSelects = new();
        [HideInInspector] public Dictionary<int, float> pointsChance = new();
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
        foreach (DifficultyCPUConfigs difficultyCPUConfigs in difficultiesConfigsEdit)
        {
            difficultiesConfigs.Add(difficultyCPUConfigs.difficulty, difficultyCPUConfigs);

            //set up the dicenary of pointchance
            foreach (DifficultyCPUConfigs.PointChanceToSelect pointChanceToSelect in difficultyCPUConfigs.pointChanceToSelects)
            {
                difficultyCPUConfigs.pointsChance.Add(pointChanceToSelect.pointGain, pointChanceToSelect.chanceToSelect);
            }
        }
    }

    protected override IEnumerator CPUStart(int plrId)
    {
        LokaalConnecter.PlayerController playerController = LokaalConnecter.plrsController[plrId];
        DifficultyCPUConfigs configs = difficultiesConfigs[GameMangeren.cPUDifficulty];

        int oldPointGain = int.MaxValue;

        //ready up
        yield return new WaitForSeconds(secBeforeReadyUp);

        playerController.CPUSetButton(LokaalConnecter.InputType.x, true);

        yield return null;

        playerController.CPUSetButton(LokaalConnecter.InputType.x, false);

        //soon unitl it is finish :3
        while (true)
        {
            //caculate so we have less time between the real offset
            float offset = UnityEngine.Random.Range(configs.offSet.x, configs.offSet.y);

            //bbug if it go the same or no one press nothing happend
            yield return new WaitUntil(() => spawnObject.objectToActivate == null || !spawnObject.objectToActivate.activeSelf || oldPointGain != getPointGain());
            yield return new WaitUntil(() => spawnObject.objectToActivate != null && spawnObject.objectToActivate.activeSelf);

            //now it spawned in and it wait for the offset while caculating if it want it
            int pointGain = getPointGain();
            oldPointGain = pointGain;
            float chanceToSelect = configs.pointsChance[pointGain];
            float rngChance = UnityEngine.Random.Range(0f, 1f); //1 is th emax

            if (rngChance > chanceToSelect) continue;

            yield return new WaitForSeconds(offset);

            playerController.CPUSetButton(LokaalConnecter.InputType.x, true);

            yield return null;
        }

        //yield return null;
    }

    int getPointGain()
    {
        Match pointMatch = Regex.Match(spawnObject.objectToActivate.name, @"-?\d+");
        if (!pointMatch.Success) return -3; //if it false jut return the smalest of them all

        return int.Parse(pointMatch.Value);
    }
}
