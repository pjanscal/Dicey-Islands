using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SceneManager =
    UnityEngine.SceneManagement.SceneManager;

public class Minigame2ResultBridge : MonoBehaviour
{
    [Header("Return To Board")]
    [SerializeField]
    private float returnToBoardDelay = 3f;

    [SerializeField]
    private string boardSceneName =
        "BoardTestScene";

    private bool resultsProcessed = false;

    private void Update()
    {
        if (resultsProcessed)
            return;

        MaingameScript2[] allPlayers =
            Object.FindObjectsByType<MaingameScript2>(
                FindObjectsSortMode.None
            );

        List<MaingameScript2> activePlayers =
            new List<MaingameScript2>();

        foreach (
            MaingameScript2 player
            in allPlayers
        )
        {
            if (player == null)
                continue;

            if (!player.IsOccupied)
                continue;

            activePlayers.Add(player);
        }

        if (activePlayers.Count == 0)
            return;

        // =========================================
        // WAIT UNTIL SOMEONE WINS
        // =========================================

        bool someoneFinished = false;

        foreach (
            MaingameScript2 player
            in activePlayers
        )
        {
            if (player.IsGameFinished)
            {
                someoneFinished = true;
                break;
            }
        }

        if (!someoneFinished)
            return;

        resultsProcessed = true;

        StartCoroutine(
            ProcessResults(activePlayers)
        );
    }

    private IEnumerator ProcessResults(
        List<MaingameScript2> players
    )
    {
        if (MatchData.Instance == null)
        {
            Debug.LogError(
                "MatchData does not exist!"
            );

            yield break;
        }

        // =========================================
        // SORT BY POINTS
        // =========================================

        players.Sort(
            (a, b) =>
            {
                // Higher points should come first.
                int comparison =
                    b.points.CompareTo(
                        a.points
                    );

                if (comparison != 0)
                    return comparison;

                // Tie breaker:
                // lower player number wins.
                return a.PlayerId.CompareTo(
                    b.PlayerId
                );
            }
        );


        MatchData.Instance
            .playerOrderNumbers.Clear();

        for (
            int i = 0;
            i < players.Count;
            i++
        )
        {
            MatchData.Instance
                .playerOrderNumbers.Add(
                    players[i].PlayerId
                );

            Debug.Log(
                "MINIGAME 2 RESULT - Place " +
                (i + 1) +
                ": Player " +
                players[i].PlayerId +
                " with " +
                players[i].points +
                " points."
            );
        }


        yield return new WaitForSecondsRealtime(
            returnToBoardDelay
        );

        // =========================================
        // RETURN TO BOARD
        // =========================================

        MatchData.Instance
            .returningFromMinigame = true;

        SceneManager.LoadScene(
            boardSceneName
        );
    }
}