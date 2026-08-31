using UnityEngine;

public class CPUTesting: CPUMangeren
{


    protected override void CpuUpdate(int plrId)
    {
        base.CpuUpdate(plrId);

        //run away from plr1
        GameObject currentChar = plrsChar[plrId];
        GameObject target = plrsChar[1];
        Vector3 diff = currentChar.transform.position - target.transform.position;
        Vector2 moveDir = new Vector2(diff.x, diff.y).normalized;

        LokaalConnecter.plrsController[plrId].cpuMoveDir = moveDir;
    }
}
