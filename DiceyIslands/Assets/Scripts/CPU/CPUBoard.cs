using System.Collections;
using UnityEngine;

public class CPUBoard: CPUMangeren
{
    [Header("Scripts")]
    public BoardManager boardManager;

    //configs
    private Vector2 throwOffset = new Vector2(1, 2.5f);

    protected override IEnumerator CPUStart(int plrId)
    {
        LokaalConnecter.PlayerController playerController = LokaalConnecter.plrsController[plrId];

        while (true)
        {
            yield return new WaitUntil(() => boardManager.CurrentPlayer.ControllerPlayerId == plrId); //check the turn id with the plrid
        
            float throwOffsetDur = UnityEngine.Random.Range(throwOffset.x, throwOffset.y);
            yield return new WaitForSeconds(throwOffsetDur);

            playerController.CPUSetButton(LokaalConnecter.InputType.jump, true);

            yield return new WaitUntil(() => !boardManager.turnInProgress);
        }
    }
}
