using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

public class MinigameResultBridge : MonoBehaviour
{
    [Header("Return Settings")]
    [SerializeField] private float returnToBoardDelay = 3f;

    [SerializeField]
    private string boardSceneName =
        "BoardTestScene";

    private bool resultsProcessed = false;

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

        foreach (MainGameScript player in allPlayers)
        {
            if (player == null)
                continue;

            if (player.playerController == null)
                continue;

            if (!player.playerController.occuplied)
                continue;

            activePlayers.Add(player);
        }

        // No players available yet.
        if (activePlayers.Count == 0)
            return;

        // =========================================
        // WAIT FOR THE MINIGAME TO ACTUALLY START
        // =========================================

        bool everyoneReady = true;

        foreach (MainGameScript player in activePlayers)
        {
            if (!player.ready)
            {
                everyoneReady = false;
                break;
            }
        }

        // Players have not completed the ready/countdown
        // sequence yet, so the minigame is still starting.
        if (!everyoneReady)
            return;

        // =========================================
        // NOW WAIT FOR EVERYONE TO FINISH
        // =========================================

        foreach (MainGameScript player in activePlayers)
        {
            if (player.isRunning)
                return;
        }

        // Everyone was ready AND everyone has stopped.
        resultsProcessed = true;

        StartCoroutine(
            ProcessResults(activePlayers)
        );
    }

    private IEnumerator ProcessResults(
        List<MainGameScript> players
    )
    {
        // Sort by closeness to the target time.
        players.Sort(
            (a, b) =>
            {
                float aDifference =
                    Mathf.Abs(
                        a.elapsed -
                        GetTargetTime()
                    );

                float bDifference =
                    Mathf.Abs(
                        b.elapsed -
                        GetTargetTime()
                    );

                int result =
                    aDifference.CompareTo(
                        bDifference
                    );

                if (result != 0)
                    return result;

                // Same tie-break rule your friend
                // already uses.
                return a.plrId.CompareTo(
                    b.plrId
                );
            }
        );

        // Save 1st -> last in MatchData.
        if (MatchData.Instance == null)
        {
            Debug.LogError(
                "MatchData does not exist!"
            );

            yield break;
        }

        MatchData.Instance.playerOrderNumbers.Clear();

        for (int i = 0; i < players.Count; i++)
        {
            MainGameScript player =
                players[i];

            int place = i + 1;

            Debug.Log(
                "MINIGAME RESULT - Place " +
                place +
                ": Player " +
                player.plrId
            );

            MatchData.Instance.playerOrderNumbers.Add(
                player.plrId
            );
        }

        Debug.Log(
            "Minigame results saved."
        );

        yield return new WaitForSecondsRealtime(
            returnToBoardDelay
        );

        MatchData.Instance.returningFromMinigame =
            true;

        SceneManager.LoadScene(
            boardSceneName
        );
    }

    private float GetTargetTime()
    {
        TimeNeeded timeNeeded =
            FindFirstObjectByType<TimeNeeded>();

        if (timeNeeded == null)
        {
            Debug.LogError(
                "TimeNeeded could not be found!"
            );

            return 0f;
        }

        return timeNeeded.timeNeededSeconds;
    }
}