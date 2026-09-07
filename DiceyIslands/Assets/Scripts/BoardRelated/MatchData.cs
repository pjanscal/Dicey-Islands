using System.Collections.Generic;
using UnityEngine;

public class MatchData : MonoBehaviour
{
    public static MatchData Instance;

    [Header("Saved Board Positions")]
    public List<int> playerWaypointPositions =
        new List<int>();

    [Header("Player Order")]
    public List<int> playerOrderNumbers =
        new List<int>();

    [Header("Round History")]
    public List<int> historyPlayerNumbers =
        new List<int>();

    public List<List<int>> playerRoundHistories =
        new List<List<int>>();

    [Header("Current Round")]
    public int currentRound = 1;

    [Header("Selected Minigame")]
    public string selectedMinigameScene = "";

    [Header("Minigame Return")]
    public bool returningFromMinigame = false;

    public bool minigameTransitionStarted = false;

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    public void ClearData()
    {
        playerWaypointPositions.Clear();
        playerOrderNumbers.Clear();

        historyPlayerNumbers.Clear();
        playerRoundHistories.Clear();

        selectedMinigameScene = "";

        currentRound = 1;

        returningFromMinigame = false;
        minigameTransitionStarted = false;
    }
}