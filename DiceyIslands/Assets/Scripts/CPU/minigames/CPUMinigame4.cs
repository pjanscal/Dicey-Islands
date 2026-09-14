using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class CPUMinigame4: CPUMangeren
{
    [Serializable]
    private class DifficultyCPUConfigs
    {
        public GameMangeren.CPUDifficulty difficulty;
        public float chasingOffset; //offset of how far he think by cutting u off
        public float chasingDecectRange; //range to know where to not go
    }

    [Serializable]
    private class CPUSaveFile //help saving in a update
    {
        public float oldAngle;
        public Vector3? oldPos;
        public float lastChase = -1;
    }

    private DifficultyCPUConfigs configs;
    private Dictionary<int, CPUSaveFile> cpuSaveFiles = new(); //save of the position **if more then 1 then a class

    //minigameMangeren4Script that is use manytime
    private int potatoTarget => Minigame4Mangeren.instance.potatoTarget;
    private Dictionary<int, Player_Minigame4> plrScripts => Minigame4Mangeren.instance.plrScripts;

    //configs
    [Header("Configs")]
    [SerializeField] private List<DifficultyCPUConfigs> difficultiesConfigsEdit = new(3); // soon update it better just beta testing
    const float mapSize = 10;
    const float mapSideMarge = .5f;
    const float minAngleToSwitch = 45;
    const float maxChaseTime = 1;
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
        cpuSaveFiles.Add(plrId, new());

        return base.CPUStart(plrId);
    }

    protected override void Update()
    {
        if (!Minigame4Mangeren.instance.isActive) return;
        base.Update();
    }

    protected override void CpuUpdate(int plrId)
    {
        //check wich behaviour
        if (Minigame4Mangeren.instance.potatoTarget == plrId) PotatoBehavoir(plrId);
        else RunnerBehavoir(plrId);
    }

    void PotatoBehavoir(int plrId)
    {
        LokaalConnecter.PlayerController playerController = LokaalConnecter.plrsController[plrId];

        //get the closet one
        (int target, Transform targetPos) = GetClosestTarget(plrId);

        //caculate where he walk
        Vector3 cuttingPos = targetPos.position + (plrScripts[target].velocity * configs.chasingOffset); //position he tried to predict
        Vector3 dir = (cuttingPos - plrsChar[plrId].transform.position).normalized;

        //move toward that point
        Vector2 moveDir = GetDirFromCam(dir);
        playerController.cpuMoveDir = moveDir;

        //debugging
        //print($"targeting{target}, moving toward: {cuttingPos}, moveDir: {moveDir}");
    }

    //Idea: ik i have kinda a pos inside the baseplate they can't run to chaser
    // and when they getting close to chaser it switch to running away
    // when by barrier it act also like a chaser where he walk other way from the wall/chaser
    void RunnerBehavoir(int plrId)
    {
        LokaalConnecter.PlayerController playerController = LokaalConnecter.plrsController[plrId];
        CPUSaveFile cpuSaveFile = cpuSaveFiles[plrId];

        //check or the chaser is close
        if (IsCloseToChaser(plrId)) cpuSaveFile.lastChase = maxChaseTime;
        if (!Minigame4Mangeren.instance.plrsIngame.Contains(potatoTarget)) cpuSaveFile.lastChase = -1;
        if (cpuSaveFile.lastChase > 0)
        {
            Vector3 dir = GetRunnerDirectionWhenRunningAway(plrId);
            VisualeTargetPos(plrsChar[plrId].transform.position + dir, false);
            playerController.cpuMoveDir = GetDirFromCam(dir);
            
            cpuSaveFile.lastChase -= Time.deltaTime;
        }
        else
        {
            //move to rng place
            Vector3 targetPos = cpuSaveFile.oldPos == null? GetRunnerTargetPosition(plrId) : cpuSaveFiles[plrId].oldPos.Value;
            Vector3 dir = (targetPos - plrsChar[plrId].transform.position).normalized;
            dir.y = 0;
            dir = dir.normalized;
            playerController.cpuMoveDir = GetDirFromCam(dir);

            //delete if finish or something else
            float angle = GetAngleFromDir(dir);
            float diffent = math.abs(Mathf.DeltaAngle(angle, cpuSaveFile.oldAngle));
            //print($"plr{plrId}: angle toward{angle}, old angle{cpuSaveFile.oldAngle}, posItionDiff = {(targetPos - plrsChar[plrId].transform.position).magnitude}, angle diff{diffent}");
            if (diffent > minAngleToSwitch) cpuSaveFile.oldPos = null;
            if (IsCloseToChaser(plrId, cpuSaveFile.oldPos)) cpuSaveFile.oldPos = null;
        }
    }

    //helper function
    //get the closet target of player
    (int, Transform) GetClosestTarget(int owner)
    {
        int closetTarget = owner; //follow himself also always move forward or stop
        Transform closetPos = plrsChar[owner].transform; //follow himself also always move forward or stop
        float closetDistance = float.PositiveInfinity;

        //check every player in game
        foreach (int plrId in Minigame4Mangeren.instance.plrsIngame)
        {
            if (plrId == owner) continue;

            Transform pos = plrsChar[plrId].transform;
            float distance = (plrsChar[owner].transform.position - pos.position).magnitude;

            //check or this is closer
            if (closetDistance > distance)
            {
                closetTarget = plrId;
                closetPos = pos;
                closetDistance = distance;
            }
        }

        if (owner == closetTarget) Debug.LogError("no target found");

        return (closetTarget, closetPos);
    }

    //position when the running is not close to the chaser
    Vector3 GetRunnerTargetPosition(int plrId)
    {
        float rngAngle = UnityEngine.Random.Range(-180f, 180f); //get random angle
        Vector3 dir = AngleToDirection(rngAngle);

        //get a random distance he can walk.
        Vector3 plrPosition = plrsChar[plrId].transform.position;
        float halfMapValidDistance = mapSize / 2 - mapSideMarge;
        float maxXDis = GetMaxDistant(0); //get distance how long it take to get to the boundries
        float maxYDis = GetMaxDistant(2);
        float distance = UnityEngine.Random.Range(0, math.min(maxXDis, maxYDis));
        Vector3 targetPos = plrPosition + dir * distance;

        //0 = x, 1 = y, 2 = z
        //get distance how long it take to get to the boundries
        float GetMaxDistant(int axis)
        {
            float dirAxis = math.abs(dir[axis]);
            if (dirAxis < .0001f) dirAxis = float.MaxValue;
            float maxDisant = math.abs(halfMapValidDistance - plrPosition[axis] * math.sign(dir[axis])) / dirAxis;

            if (!Minigame4Mangeren.instance.plrsIngame.Contains(potatoTarget)) return maxDisant;

            //look at the enemie *hulp from ai here
            Vector3 chaserPos = plrsChar[potatoTarget].transform.position;
            Vector3 chaserDir = chaserPos - plrPosition;
            
            // How far the enemy is along our movement direction
            float alongDir = Vector3.Dot(chaserDir, dir);

            if (alongDir > 0f)
            {
                // Distance from enemy to our movement line
                float perpendicularDist =
                    Vector3.Distance(
                        plrPosition + dir * alongDir,
                        chaserPos
                    );

                if (perpendicularDist < configs.chasingDecectRange)
                {
                    // Distance from closest point to circle edge
                    float offset = Mathf.Sqrt(
                        configs.chasingDecectRange * configs.chasingDecectRange -
                        perpendicularDist * perpendicularDist
                    );

                    // Distance until we enter the enemy's radius
                    float chaserMaxDistance = alongDir - offset;

                    maxDisant = math.min(maxDisant, math.max(0f, chaserMaxDistance));
                }
            }

            return maxDisant;
        }
        
        cpuSaveFiles[plrId].oldPos = targetPos;
        cpuSaveFiles[plrId].oldAngle = rngAngle; //help 
        VisualeTargetPos(targetPos, true);
        //print($"plr{plrId}, distance: {distance}, plrPosition: {plrPosition}, dir: {dir}, maxDisant: {math.min(maxXDis, maxYDis)}");
        return targetPos;
    }

    //position when running away from the chaser
    Vector3 GetRunnerDirectionWhenRunningAway(int plrId)
    {
        Vector3 plrPos = plrsChar[plrId].transform.position;
        Vector3 chaserPos = plrsChar[potatoTarget].transform.position;
        Vector3 dirFromChaser = (plrPos - chaserPos);
        dirFromChaser.y = 0;
        dirFromChaser = dirFromChaser.normalized;
        List<(int axis, int posSide)> wallInfos = new();

        GetWallDir(0);
        GetWallDir(2);

        void GetWallDir(int axis)
        {
            int plrMapeSide = (int)math.sign(plrPos[axis]);
            float wallPosAxis = (mapSize / 2 - mapSideMarge) * plrMapeSide;
            bool succes = math.abs(wallPosAxis - plrPos[axis]) < mapSideMarge *2.5;
            if (!succes) return;

            Vector3 dir = Vector3.zero;
            dir[axis] = math.sign(wallPosAxis);
            wallInfos.Add((axis, -plrMapeSide));
        }

        //get closet one
        if (wallInfos.Count == 0) return dirFromChaser; //only do this if no wall are insolve

        List<float> wallAngles = new();
        //find out where the 2 wall boundies can be
        //X+ = 0 angle
        if (wallInfos.Count == 1)
        {
            int axis = wallInfos[0].axis;
            int angle = axis == 0? -90 : 0;
            wallAngles.Add(angle);
            wallAngles.Add(angle + 180); //make 180 0, 90 -90

            //put dir right
            dirFromChaser[axis] = math.abs(dirFromChaser[axis]) * -wallInfos[0].posSide;
        }
        else
        {
            //it always start at x...
            wallAngles.Add(wallInfos[0].posSide == 1? 0 : 180);
            wallAngles.Add(wallInfos[1].posSide == 1? 90 : -90);

            //put dir right
            dirFromChaser.x = math.abs(dirFromChaser.x) * -wallInfos[0].posSide;
            dirFromChaser.z = math.abs(dirFromChaser.z) * -wallInfos[1].posSide;
        }

        float chaserAngle = GetAngleFromDir(dirFromChaser);
        if (wallInfos.Count == 1 && wallInfos[0].axis == 2 && math.sign(chaserAngle) == -1) wallAngles[1] *= -1;

        float biggestDiffent = 0;
        int biggestId = 0;
        for (int i = 0; i < wallAngles.Count; i++)
        {
            float currentAngle = wallAngles[i];

            float angleDiffent = Mathf.DeltaAngle(chaserAngle, currentAngle);
            if (math.abs(angleDiffent) > math.abs(biggestDiffent))
            {
                biggestDiffent = angleDiffent;
                biggestId = i;
            }
        }
        float targetAngle = Mathf.DeltaAngle(0, GetAngleFromDir(-dirFromChaser) + biggestDiffent / 2);
        Vector3 dir = AngleToDirection(targetAngle);

        //get angle toward it
        void DebugLine(Vector3 dir)
        {
            dir += plrPos;
            Debug.DrawLine(plrPos, dir, Color.blue, .05f);
        }
        DebugLine(dir);

        /*
        Debug.Log(
            $"RUNNER DEBUG\n" +
            $"Player Pos: {plrPos}\n" +
            $"Chaser Pos: {chaserPos}\n" +
            $"Dir Chaser: {dirFromChaser}\n" +
            $"Chaser Angle: {chaserAngle}\n" +
            $"Wall Count: {wallInfos.Count}\n" +
            $"Wall Infos: {string.Join(", ", wallInfos.Select(w => $"Axis={w.axis}, PosSide={w.posSide}"))}\n" +
            $"Wall Angles: {string.Join(", ", wallAngles)}\n" +
            $"Biggest Difference: {biggestDiffent}\n" +
            $"Target Angle: {targetAngle}\n" +
            $"Target Direction: {dir}"
        );
        */

        return dir.normalized;
    }

    float GetAngleFromDir(Vector3 dir)
    {
        float angle = Vector2.SignedAngle(Vector2.right, new Vector2(dir.x, dir.z));
        return angle;
    }

    Vector3 AngleToDirection(float angle)
    {
        float radians = angle * Mathf.Deg2Rad; //make a radians from to do sins and cos
        Vector3 dir = new Vector3(Mathf.Cos(radians), 0, Mathf.Sin(radians));
        return dir;
    }

    //is in distance
    bool IsCloseToChaser(int plrId, Vector3? pos = null)
    {
        if (!Minigame4Mangeren.instance.plrsIngame.Contains(potatoTarget)) return false;
        if (pos == null) pos = plrsChar[plrId].transform.position;

        Vector3 chaserPosition = plrsChar[potatoTarget].transform.position;

        float distance = (pos.Value - chaserPosition).magnitude;
        return distance < configs.chasingDecectRange;
    }

    Vector2 GetDirFromCam(Vector3 dir)
    {
        Vector3 camDir = Minigame4Mangeren.instance.camaraHolder.InverseTransformDirection(dir); //somehow when multiplere vector3 with rotation it try making that vector3 looking toward the dir
        camDir.y = 0;
        camDir = camDir.normalized;
        return new Vector2(camDir.x, camDir.z);
    }

    //Debug function
    void VisualeTargetPos(Vector3 targetPos, bool isRunner)
    {
        Debug.DrawLine(targetPos, targetPos + Vector3.up * 1, isRunner? Color.green : Color.red, .1f);
    }
}
