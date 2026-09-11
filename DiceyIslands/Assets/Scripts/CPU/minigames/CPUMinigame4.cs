using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class CPUMinigame4: CPUMangeren
{
    [Serializable]
    private class DifficultyCPUConfigs
    {
        public GameMangeren.CPUDifficulty difficulty;
        public float chasingOffset; //offset of how far he think by cutting u off
    }

    [SerializeField] private Minigame4Mangeren minigame4Mangeren;
    private DifficultyCPUConfigs configs;

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
        }

        configs = difficultiesConfigs[GameMangeren.cPUDifficulty];
    }

    protected override IEnumerator CPUStart(int plrId)
    {
        return base.CPUStart(plrId);
    }

    protected override void CpuUpdate(int plrId)
    {
        //check wich behaviour
        if (minigame4Mangeren.potatoTarget == plrId) PotatoBehavoir(plrId);
        else RunnerBehavoir(plrId);
    }

    void PotatoBehavoir(int plrId)
    {
        //get the closet one
        (int target, Transform targetPos) = GetClosestTarget(plrId);

        //caculate where he walk
        Vector3 cuttingPos = targetPos.position + (minigame4Mangeren.plrScripts[target].velocity * configs.chasingOffset); //position he tried to predict
        //Vector3 dir =  

        //move toward that point
    }

    void RunnerBehavoir(int plrId)
    {
        
    }

    //get the closet target of player
    (int, Transform) GetClosestTarget(int owner)
    {
        int closetTarget = owner;
        Transform closetPos = transform;
        float closetDistance = float.PositiveInfinity;

        //check every player in game
        foreach (int plrId in minigame4Mangeren.plrsIngame)
        {
            if (plrId == owner) continue;

            Transform pos = plrsChar[plrId].transform;
            float distance = (transform.position - pos.position).magnitude;

            //check or this is closer
            if (closetDistance > distance)
            {
                closetTarget = plrId;
                closetPos = pos;
                closetDistance = distance;
            }
        }

        return (owner, transform); //follow himself also always move forward or stop
    }
}
