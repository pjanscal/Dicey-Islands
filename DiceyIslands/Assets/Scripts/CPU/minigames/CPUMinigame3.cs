using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CPUMinigame3: CPUMangeren
{
    [Serializable]
    private class DifficultyCPUConfigs
    {
        public GameMangeren.CPUDifficulty difficulty;
        public int minRoundToSurvive = 3;
        public float chanceToDie = 80;
        public float checkingRange = 1f;
        public float checkingAfterRange = .5f; //after the sword passes*
    }

    private Dictionary<GameMangeren.CPUDifficulty, DifficultyCPUConfigs> difficultiesConfigs = new();
    [SerializeField] private GameObject targetBlock;

    //configs
    [Header("Configs")]
    [SerializeField] private List<DifficultyCPUConfigs> difficultiesConfigsEdit = new(3); // soon update it better just beta testing
    const float maxTimeToThrow = 5f;
    private Vector2 beginThrowOffset = new Vector2(.5f, 1.5f);
    const float rayDistance = 10f;
    const string swordTag = "Sword";
    private LayerMask raycastLayers = ~0;

    protected override void Start()
    {
        base.Start();

        //beta
        foreach (DifficultyCPUConfigs difficultyCPUConfigs in difficultiesConfigsEdit)
        {
            difficultiesConfigs.Add(difficultyCPUConfigs.difficulty, difficultyCPUConfigs);
        }
    }

    protected override IEnumerator CPUStart(int plrId)
    {
        LokaalConnecter.PlayerController playerController = LokaalConnecter.plrsController[plrId];
        MaingameScript3 maingameScript3 = plrsChar[plrId].GetComponent<MaingameScript3>();
        DifficultyCPUConfigs config = difficultiesConfigs[GameMangeren.cPUDifficulty];

        //get the check box
        Transform plrTransform = plrsChar[plrId].transform;
        Vector3 origin = plrTransform.position;
        Vector3 direction = plrTransform.forward;
        bool hitSucces = Physics.Raycast(origin, direction, out RaycastHit hit, rayDistance, raycastLayers);
        if (!hitSucces) {Debug.LogError($"no thing touch return cpu{plrId} out of power"); yield break;}

        //raycastign
        Vector3 hitpos = hit.point;
        float checkRadius =  (config.checkingRange - -config.checkingAfterRange) / 2;
        Vector3 checkPos = hitpos + plrTransform.right * (config.checkingRange - checkRadius);
        Debug.DrawLine(checkPos, checkPos + Vector3.forward, Color.green, 10f);


        yield return new WaitUntil(() => maingameScript3.init);

        int index = 0;
        while (true)
        {
            index += 1;

            yield return new WaitUntil(() => !maingameScript3.hasThrownThisRound);
            float rngWaitTime = UnityEngine.Random.Range(beginThrowOffset.x, beginThrowOffset.y);
            yield return new WaitForSeconds(rngWaitTime);
            float targetTick = Time.time + maxTimeToThrow; 
            bool garadeertHit = index < config.minRoundToSurvive; //so it can make the random only use when needed and faster down
            float rngDeath = garadeertHit? 0 : UnityEngine.Random.Range(0, 100); //random number between 0 - 100
            yield return new WaitUntil(() => CanThrow());

            playerController.CPUSetButton(LokaalConnecter.InputType.x, true);

            bool CanThrow()
            {
                if (targetTick <= Time.time) return true;

                //check or there is a sword
                Collider[] allObjectInDecect = Physics.OverlapSphere(checkPos, checkRadius, raycastLayers);
                foreach (Collider collider in allObjectInDecect)
                {
                    if (collider.CompareTag(swordTag))
                    {
                        //gamble or he die or not
                        if (garadeertHit) return true;

                        if (rngDeath < config.chanceToDie) return false;
                    }
                }

                return true;
            }
        }
    }
}
