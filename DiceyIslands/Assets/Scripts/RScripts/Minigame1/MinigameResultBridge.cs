using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SceneManager =
    UnityEngine.SceneManagement.SceneManager;

public class MinigameResultBridge : MonoBehaviour
{
    [Header("Return To Board")]
    [SerializeField]
    private float returnToBoardDelay = 3f;

    [SerializeField]
    private string boardSceneName =
        "BoardTestScene";

    private bool minigameHasStarted = false;
    private bool resultsProcessed = false;

    private TimeNeeded timeNeeded;

    private void Start()
    {
        timeNeeded =
            FindFirstObjectByType<TimeNeeded>();
    }

    private void Update()
    {
        if (resultsProcessed)
            return;

        MainGameScript[] allPlayers =
            Object.FindObjectsByType<MainGameScript>(
                FindObjectsSortMode.None
            );

        List<MainGameScript> activePlayers =
            new List<MainGameScript>();

        foreach (
            MainGameScript player
            in allPlayers
        )
        {
            if (player == null)
                continue;

            if (player.playerController == null)
                continue;

            if (!player.playerController.occuplied)
                continue;

            activePlayers.Add(player);
        }

        if (activePlayers.Count == 0)
            return;

        // =========================================
        // WAIT UNTIL THE GAME HAS ACTUALLY STARTED
        // =========================================

        if (!minigameHasStarted)
        {
            foreach (
                MainGameScript player
                in activePlayers
            )
            {
                if (player.ready ||
                    player.isRunning)
                {
                    minigameHasStarted = true;

                    Debug.Log(
                        "MinigameResultBridge: " +
                        "minigame has started."
                    );

                    break;
                }
            }

            return;
        }

        // =========================================
        // WAIT FOR EVERY PLAYER TO FINISH
        // =========================================

        foreach (
            MainGameScript player
            in activePlayers
        )
        {
            if (player.isRunning)
                return;
        }

        // Everybody has stopped.
        resultsProcessed = true;

        StartCoroutine(
            ProcessResults(activePlayers)
        );
    }

    private IEnumerator ProcessResults(
        List<MainGameScript> players
    )
    {
        if (MatchData.Instance == null)
        {
            Debug.LogError(
                "MatchData does not exist!"
            );

            yield break;
        }

        if (timeNeeded == null)
        {
            Debug.LogError(
                "TimeNeeded could not be found!"
            );

            yield break;
        }

        float target =
            timeNeeded.timeNeededSeconds;

        // =========================================
        // SORT PLAYERS BY RESULT
        // =========================================

        players.Sort(
            (a, b) =>
            {
                float aDifference =
                    Mathf.Abs(
                        a.elapsed - target
                    );

                float bDifference =
                    Mathf.Abs(
                        b.elapsed - target
                    );

                int comparison =
                    aDifference.CompareTo(
                        bDifference
                    );

                if (comparison != 0)
                    return comparison;

                // Tie breaker:
                // lower player number wins.
                return a.plrId.CompareTo(
                    b.plrId
                );
            }
        );

        // =========================================
        // SAVE NEW TURN ORDER
        // =========================================

        MatchData.Instance
            .playerOrderNumbers.Clear();

        for (
            int i = 0;
            i < players.Count;
            i++
        )
        {
            int playerNumber =
                players[i].plrId;

            MatchData.Instance
                .playerOrderNumbers.Add(
                    playerNumber
                );

            Debug.Log(
                "MINIGAME RESULT - Place " +
                (i + 1) +
                ": Player " +
                playerNumber
            );
        }

        // =========================================
        // SHOW RESULTS FOR A MOMENT
        // =========================================

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